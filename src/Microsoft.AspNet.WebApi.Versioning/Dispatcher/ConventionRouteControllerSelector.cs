namespace Microsoft.Web.Http.Dispatcher
{
    using Microsoft.Web.Http.Versioning;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Text;
    using System.Web.Http;
    using System.Web.Http.Controllers;
    using System.Web.Http.Routing;
    using static System.Environment;

    sealed class ConventionRouteControllerSelector : ControllerSelector
    {
        readonly HttpControllerTypeCache controllerTypeCache;

        internal ConventionRouteControllerSelector( ApiVersioningOptions options, HttpControllerTypeCache controllerTypeCache )
            : base( options ) => this.controllerTypeCache = controllerTypeCache;

        internal override ControllerSelectionResult SelectController( ControllerSelectionContext context )
        {
            Contract.Requires( context != null );
            Contract.Ensures( Contract.Result<ControllerSelectionResult>() != null );

            var request = context.Request;
            var requestedVersion = context.RequestedApiVersion;
            var controllerName = context.ControllerName;
            var result = new ControllerSelectionResult()
            {
                RequestedVersion = requestedVersion,
                ControllerName = controllerName,
                HasCandidates = context.HasConventionBasedRoutes,
            };

            if ( !result.HasCandidates )
            {
                return result;
            }

            var ambiguousException = new Lazy<Exception>( () => CreateAmbiguousControllerException( context.RouteData.Route, controllerName, controllerTypeCache.GetControllerTypes( controllerName ) ) );
            var versionNeutralController = result.Controller = GetVersionNeutralController( context.ConventionRouteCandidates, ambiguousException );

            if ( requestedVersion == null )
            {
                if ( !AssumeDefaultVersionWhenUnspecified )
                {
                    return result;
                }

                requestedVersion = ApiVersionSelector.SelectVersion( request, context.AllVersions );

                if ( requestedVersion == null )
                {
                    return result;
                }
            }

            var versionedController = GetVersionedController( context, requestedVersion, ambiguousException );

            if ( versionedController == null )
            {
                return result;
            }

            if ( versionNeutralController != null )
            {
                throw ambiguousException.Value;
            }

            request.ApiVersionProperties().RequestedApiVersion = requestedVersion;
            result.RequestedVersion = requestedVersion;
            result.Controller = versionedController;

            return result;
        }

        static HttpControllerDescriptor GetVersionNeutralController( IEnumerable<HttpControllerDescriptor> candidates, Lazy<Exception> ambiguousException )
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

        static HttpControllerDescriptor GetVersionedController( ControllerSelectionContext context, ApiVersion requestedVersion, Lazy<Exception> ambiguousException )
        {
            Contract.Requires( context != null );
            Contract.Requires( requestedVersion != null );
            Contract.Requires( ambiguousException != null );

            var candidates = context.ConventionRouteCandidates;
            var controller = candidates[0];

            if ( candidates.Count == 1 )
            {
                if ( !controller.GetDeclaredApiVersions().Contains( requestedVersion ) )
                {
                    return null;
                }
            }
            else
            {
                if ( ( controller = ResolveController( candidates, requestedVersion, ambiguousException ) ) == null )
                {
                    return null;
                }
            }

            controller.SetRelatedCandidates( candidates );
            return controller;
        }

        static HttpControllerDescriptor ResolveController( IEnumerable<HttpControllerDescriptor> candidates, ApiVersion requestedVersion, Lazy<Exception> ambiguousException )
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

        static Exception CreateAmbiguousControllerException( IHttpRoute route, string controllerName, ICollection<Type> matchingTypes )
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