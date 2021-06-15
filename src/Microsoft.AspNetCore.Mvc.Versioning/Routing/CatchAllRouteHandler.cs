namespace Microsoft.AspNetCore.Mvc.Routing
{
    using Microsoft.AspNetCore.Mvc.Versioning;
    using Microsoft.AspNetCore.Routing;
    using System.Threading.Tasks;
    using static System.Threading.Tasks.Task;

    sealed class CatchAllRouteHandler : IRouter
    {
        readonly IApiVersionRoutePolicy routePolicy;

        public CatchAllRouteHandler( IApiVersionRoutePolicy routePolicy ) => this.routePolicy = routePolicy;

        /// <inheritdoc />
        public VirtualPathData? GetVirtualPath( VirtualPathContext context ) => null;

        /// <inheritdoc />
        public Task RouteAsync( RouteContext context )
        {
            var feature = context.HttpContext.ApiVersioningFeature();
            routePolicy.Evaluate( context, feature.SelectionResult );
            return CompletedTask;
        }
    }
}