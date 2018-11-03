namespace Microsoft.Web.Http.Dispatcher
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Text;
    using System.Web.Http;
    using System.Web.Http.Controllers;
    using System.Web.Http.Routing;
    using static Microsoft.Web.Http.Versioning.ApiVersionMapping;
    using static System.Environment;

    sealed class ConventionRouteControllerSelector : IControllerSelector
    {
        readonly HttpControllerTypeCache controllerTypeCache;

        internal ConventionRouteControllerSelector( HttpControllerTypeCache controllerTypeCache ) => this.controllerTypeCache = controllerTypeCache;

        public ControllerSelectionResult SelectController( ControllerSelectionContext context )
        {
            Contract.Requires( context != null );
            Contract.Ensures( Contract.Result<ControllerSelectionResult>() != null );

            var request = context.Request;
            var requestedVersion = context.RequestedVersion;
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

            var bestMatch = default( HttpActionDescriptor );
            var bestMatches = new HashSet<HttpControllerDescriptor>();
            var implicitMatches = new HashSet<HttpControllerDescriptor>();

            for ( var i = 0; i < context.ConventionRouteCandidates.Length; i++ )
            {
                var action = context.ConventionRouteCandidates[i].ActionDescriptor;

                switch ( action.MappingTo( requestedVersion ) )
                {
                    case Explicit:
                        bestMatch = action;
                        bestMatches.Add( action.ControllerDescriptor );
                        break;
                    case Implicit:
                        implicitMatches.Add( action.ControllerDescriptor );
                        break;
                }
            }

            switch ( bestMatches.Count )
            {
                case 0:
                    bestMatches.UnionWith( implicitMatches );
                    break;
                case 1:
                    if ( bestMatch.GetApiVersionModel().IsApiVersionNeutral )
                    {
                        bestMatches.UnionWith( implicitMatches );
                    }

                    break;
            }

            switch ( bestMatches.Count )
            {
                case 0:
                    break;
                case 1:
                    result.Controller = bestMatches.Single();
                    result.Controller.SetPossibleCandidates( context.ConventionRouteCandidates.Select( c => c.ActionDescriptor.ControllerDescriptor ).ToArray() );
                    break;
                default:
                    throw CreateAmbiguousControllerException( context.RouteData.Route, controllerName, controllerTypeCache.GetControllerTypes( controllerName ) );
            }

            return result;
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