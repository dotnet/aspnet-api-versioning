namespace Microsoft.Web.Http.Dispatcher
{
    using Microsoft.Web.Http.Routing;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Text;
    using System.Web.Http;
    using System.Web.Http.Controllers;
    using Microsoft.Web.Http.Versioning;
    using static System.Environment;

    sealed class DirectRouteControllerSelector : ControllerSelector
    {
        internal DirectRouteControllerSelector( ApiVersioningOptions options ) : base( options ) { }

        internal override ControllerSelectionResult SelectController( ControllerSelectionContext context )
        {
            Contract.Requires( context != null );
            Contract.Ensures( Contract.Result<ControllerSelectionResult>() != null );

            var request = context.Request;
            var requestedVersion = context.RequestedApiVersion;
            var result = new ControllerSelectionResult()
            {
                HasCandidates = context.HasAttributeBasedRoutes,
                RequestedVersion = requestedVersion,
            };

            if ( !result.HasCandidates )
            {
                return result;
            }

            var versionNeutralController = result.Controller = GetVersionNeutralController( context.DirectRouteCandidates );

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

            var versionedController = GetVersionedController( context, requestedVersion );

            if ( versionedController == null )
            {
                return result;
            }

            if ( versionNeutralController != null )
            {
                throw CreateAmbiguousControllerException( new[] { versionNeutralController, versionedController } );
            }

            request.ApiVersionProperties().RequestedApiVersion = requestedVersion;
            result.RequestedVersion = requestedVersion;
            result.Controller = versionedController;

            return result;
        }

        static HttpControllerDescriptor GetVersionNeutralController( CandidateAction[] directRouteCandidates )
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

        static HttpControllerDescriptor GetVersionedController( ControllerSelectionContext context, ApiVersion requestedVersion )
        {
            Contract.Requires( context != null );
            Contract.Requires( requestedVersion != null );

            var directRouteCandidates = context.DirectRouteCandidates;
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

            return controller;
        }

        static HttpControllerDescriptor ResolveController( CandidateAction[] directRouteCandidates, ApiVersion requestedVersion )
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

        static Exception CreateAmbiguousControllerException( CandidateAction[] candidates ) =>
            CreateAmbiguousControllerException( candidates.Select( c => c.ActionDescriptor.ControllerDescriptor ) );

        static Exception CreateAmbiguousControllerException( IEnumerable<HttpControllerDescriptor> candidates )
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