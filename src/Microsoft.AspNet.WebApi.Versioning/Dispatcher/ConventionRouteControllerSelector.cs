namespace Microsoft.Web.Http.Dispatcher
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Text;
    using System.Web.Http;
    using System.Web.Http.Controllers;
    using System.Web.Http.Routing;
    using Versioning;
    using static System.Environment;

    internal sealed class ConventionRouteControllerSelector : ControllerSelector
    {
        private readonly HttpControllerTypeCache controllerTypeCache;

        internal ConventionRouteControllerSelector( ApiVersioningOptions options, HttpControllerTypeCache controllerTypeCache )
            : base( options )
        {
            Contract.Requires( controllerTypeCache != null );
            this.controllerTypeCache = controllerTypeCache;
        }

        [SuppressMessage( "Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Handled by the caller." )]
        internal override ControllerSelectionResult SelectController( ApiVersionControllerAggregator aggregator )
        {
            Contract.Requires( aggregator != null );
            Contract.Ensures( Contract.Result<ControllerSelectionResult>() != null );

            var request = aggregator.Request;
            var requestedVersion = aggregator.RequestedApiVersion;
            var controllerName = aggregator.ControllerName;
            var result = new ControllerSelectionResult()
            {
                RequestedVersion = requestedVersion,
                ControllerName = controllerName,
                HasCandidates = aggregator.HasConventionBasedRoutes
            };

            if ( !result.HasCandidates )
            {
                return result;
            }

            var ambiguousException = new Lazy<Exception>( () => CreateAmbiguousControllerException( aggregator.RouteData.Route, controllerName, controllerTypeCache.GetControllerTypes( controllerName ) ) );
            var versionNeutralController = result.Controller = GetVersionNeutralController( aggregator.ConventionRouteCandidates, ambiguousException );

            if ( requestedVersion == null )
            {
                if ( !AssumeDefaultVersionWhenUnspecified )
                {
                    return result;
                }

                requestedVersion = ApiVersionSelector.SelectVersion( request, aggregator.AllVersions );

                if ( requestedVersion == null )
                {
                    return result;
                }
            }

            var versionedController = GetVersionedController( aggregator, requestedVersion, ambiguousException );

            if ( versionedController == null )
            {
                return result;
            }

            if ( versionNeutralController != null )
            {
                throw ambiguousException.Value;
            }

            request.SetRequestedApiVersion( requestedVersion );
            result.Controller = versionedController;

            return result;
        }

        private static HttpControllerDescriptor GetVersionNeutralController( IEnumerable<HttpControllerDescriptor> candidates, Lazy<Exception> ambiguousException )
        {
            Contract.Requires( candidates != null );
            Contract.Requires( ambiguousException != null );

            var controllerDescriptor = default( HttpControllerDescriptor );

            using ( var iterator = candidates.Where( c => c.IsApiVersionNeutral() ).GetEnumerator() )
            {
                if ( !iterator.MoveNext() )
                {
                    return controllerDescriptor;
                }

                controllerDescriptor = iterator.Current;

                while ( iterator.MoveNext() )
                {
                    var candidate = iterator.Current;

                    if ( candidate != controllerDescriptor )
                    {
                        throw ambiguousException.Value;
                    }
                }
            }

            return controllerDescriptor;
        }

        private static HttpControllerDescriptor GetVersionedController( ApiVersionControllerAggregator aggregator, ApiVersion requestedVersion, Lazy<Exception> ambiguousException )
        {
            Contract.Requires( aggregator != null );
            Contract.Requires( requestedVersion != null );
            Contract.Requires( ambiguousException != null );

            var candidates = aggregator.ConventionRouteCandidates;
            var controller = candidates[0];

            if ( candidates.Count == 1 )
            {
                if ( controller.GetDeclaredApiVersions().Contains( requestedVersion ) )
                {
                    return controller;
                }

                return null;
            }

            controller = ResolveController( candidates, requestedVersion, ambiguousException );

            if ( controller != null && !controller.HasApiVersionInfo() )
            {
                controller.SetApiVersionInfo( aggregator.AllVersions );
            }

            return controller;
        }

        private static HttpControllerDescriptor ResolveController( IEnumerable<HttpControllerDescriptor> candidates, ApiVersion requestedVersion, Lazy<Exception> ambiguousException )
        {
            Contract.Requires( candidates != null );
            Contract.Requires( requestedVersion != null );
            Contract.Requires( ambiguousException != null );

            var controllerDescriptor = default( HttpControllerDescriptor );
            var matches = candidates.Where( c => c.GetDeclaredApiVersions().Contains( requestedVersion ) );

            using ( var iterator = matches.GetEnumerator() )
            {
                if ( !iterator.MoveNext() )
                {
                    return null;
                }

                controllerDescriptor = iterator.Current;

                while ( iterator.MoveNext() )
                {
                    if ( iterator.Current != controllerDescriptor )
                    {
                        throw ambiguousException.Value;
                    }
                }
            }

            return controllerDescriptor;
        }

        private static Exception CreateAmbiguousControllerException( IHttpRoute route, string controllerName, ICollection<Type> matchingTypes )
        {
            Contract.Requires( route != null );
            Contract.Requires( !string.IsNullOrEmpty( controllerName ) );
            Contract.Requires( matchingTypes != null );
            Contract.Ensures( Contract.Result<Exception>() != null );

            var builder = new StringBuilder();

            foreach ( var type in matchingTypes )
            {
                builder.AppendLine();
                builder.Append( type.FullName );
            }

            var format = SR.DefaultControllerFactory_ControllerNameAmbiguous_WithRouteTemplate;
            return new InvalidOperationException( format.FormatDefault( controllerName, route.RouteTemplate, builder, NewLine ) );
        }
    }
}
