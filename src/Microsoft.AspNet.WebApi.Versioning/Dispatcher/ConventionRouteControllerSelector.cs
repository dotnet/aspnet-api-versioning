namespace Microsoft.Web.Http.Dispatcher
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Web.Http;
    using System.Web.Http.Routing;
    using static System.Environment;

    sealed class ConventionRouteControllerSelector : ControllerSelector
    {
        readonly HttpControllerTypeCache controllerTypeCache;

        internal ConventionRouteControllerSelector( HttpControllerTypeCache controllerTypeCache ) => this.controllerTypeCache = controllerTypeCache;

        public override ControllerSelectionResult SelectController( ControllerSelectionContext context )
        {
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

            var bestMatches = SelectBestCandidates( context.ConventionRouteCandidates!, requestedVersion );

            switch ( bestMatches.Count )
            {
                case 0:
                    break;
                case 1:
                    result.Controller = bestMatches.Single();
                    result.Controller.SetPossibleCandidates( context.ConventionRouteCandidates!.Select( c => c.ActionDescriptor.ControllerDescriptor ).ToArray() );
                    break;
                default:
                    if ( TryDisambiguateControllerByAction( request, bestMatches, out var resolvedController ) )
                    {
                        result.Controller = resolvedController;
                        break;
                    }

                    throw CreateAmbiguousControllerException( context.RouteData.Route, controllerName, controllerTypeCache.GetControllerTypes( controllerName ) );
            }

            return result;
        }

        static Exception CreateAmbiguousControllerException( IHttpRoute route, string? controllerName, ICollection<Type> matchingTypes )
        {
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