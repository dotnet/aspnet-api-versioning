namespace Microsoft.Web.Http.Dispatcher
{
    using Routing;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Text;
    using System.Web.Http;
    using System.Web.Http.Controllers;
    using Versioning;
    using static System.Environment;

    internal sealed class DirectRouteControllerSelector : ControllerSelector
    {
        internal DirectRouteControllerSelector( ApiVersioningOptions options )
            : base( options )
        {
        }

        internal override ControllerSelectionResult SelectController( ApiVersionControllerAggregator aggregator )
        {
            Contract.Requires( aggregator != null );
            Contract.Ensures( Contract.Result<ControllerSelectionResult>() != null );

            var request = aggregator.Request;
            var requestedVersion = aggregator.RequestedApiVersion;
            var result = new ControllerSelectionResult()
            {
                HasCandidates = aggregator.HasAttributeBasedRoutes,
                RequestedVersion = requestedVersion
            };

            if ( !result.HasCandidates )
            {
                return result;
            }

            var versionNeutralController = result.Controller = GetVersionNeutralController( aggregator.DirectRouteCandidates );

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

            var versionedController = GetVersionedController( aggregator, requestedVersion );

            if ( versionedController == null )
            {
                return result;
            }

            if ( versionNeutralController != null )
            {
                throw CreateAmbiguousControllerException( new[] { versionNeutralController, versionedController } );
            }

            request.SetRequestedApiVersion( requestedVersion );
            result.RequestedVersion = requestedVersion;
            result.Controller = versionedController;

            return result;
        }

        private static HttpControllerDescriptor GetVersionNeutralController( CandidateAction[] directRouteCandidates )
        {
            Contract.Requires( directRouteCandidates != null );
            Contract.Requires( directRouteCandidates.Length > 0 );

            HttpControllerDescriptor controllerDescriptor = null;

            using ( var iterator = directRouteCandidates.Where( c => c.ActionDescriptor.IsApiVersionNeutral() ).GetEnumerator() )
            {
                if ( !iterator.MoveNext() )
                {
                    return controllerDescriptor;
                }

                controllerDescriptor = iterator.Current.ActionDescriptor.ControllerDescriptor;

                while ( iterator.MoveNext() )
                {
                    var candidate = iterator.Current;

                    if ( candidate.ActionDescriptor.ControllerDescriptor != controllerDescriptor )
                    {
                        throw CreateAmbiguousControllerException( directRouteCandidates );
                    }
                }
            }

            return controllerDescriptor;
        }

        private static HttpControllerDescriptor GetVersionedController( ApiVersionControllerAggregator aggregator, ApiVersion requestedVersion )
        {
            Contract.Requires( aggregator != null );
            Contract.Requires( requestedVersion != null );

            var directRouteCandidates = aggregator.DirectRouteCandidates;
            var controller = directRouteCandidates[0].ActionDescriptor.ControllerDescriptor;

            if ( directRouteCandidates.Length == 1 )
            {
                if ( !controller.GetDeclaredApiVersions().Contains( requestedVersion ) )
                {
                    return null;
                }
            }
            else
            {
                if ( ( controller = ResolveController( directRouteCandidates, requestedVersion ) ) == null )
                {
                    return null;
                }
            }

            if ( !controller.HasApiVersionInfo() )
            {
                controller.SetApiVersionModel( aggregator.AllVersions );
            }

            return controller;
        }

        private static HttpControllerDescriptor ResolveController( CandidateAction[] directRouteCandidates, ApiVersion requestedVersion )
        {
            Contract.Requires( directRouteCandidates != null );
            Contract.Requires( directRouteCandidates.Length > 0 );
            Contract.Requires( requestedVersion != null );

            var controllerDescriptor = default( HttpControllerDescriptor );
            var matches = from candidate in directRouteCandidates
                          let controller = candidate.ActionDescriptor.ControllerDescriptor
                          where controller.GetDeclaredApiVersions().Contains( requestedVersion )
                          select controller;

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
                        throw CreateAmbiguousControllerException( directRouteCandidates );
                    }
                }
            }

            return controllerDescriptor;
        }

        private static Exception CreateAmbiguousControllerException( CandidateAction[] candidates ) =>
            CreateAmbiguousControllerException( candidates.Select( c => c.ActionDescriptor.ControllerDescriptor ) );

        private static Exception CreateAmbiguousControllerException( IEnumerable<HttpControllerDescriptor> candidates )
        {
            Contract.Requires( candidates != null );
            Contract.Ensures( Contract.Result<Exception>() != null );

            var set = new HashSet<Type>( candidates.Select( c => c.ControllerType ) );
            var builder = new StringBuilder();

            foreach ( var type in set )
            {
                builder.AppendLine();
                builder.Append( type.FullName );
            }

            return new InvalidOperationException( SR.DirectRoute_AmbiguousController.FormatDefault( builder, NewLine ) );
        }
    }
}