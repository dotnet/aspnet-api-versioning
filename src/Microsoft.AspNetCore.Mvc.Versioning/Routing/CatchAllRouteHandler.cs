namespace Microsoft.AspNetCore.Mvc.Routing
{
    using Microsoft.AspNetCore.Mvc.Versioning;
    using Microsoft.AspNetCore.Routing;
    using System.Threading.Tasks;

    sealed class CatchAllRouteHandler : IRouter
    {
        static readonly Task CompletedTask = Task.FromResult( default( object ) );
        readonly IApiVersionRoutePolicy routePolicy;

        public CatchAllRouteHandler( IApiVersionRoutePolicy routePolicy ) => this.routePolicy = routePolicy;

        /// <inheritdoc />
        public VirtualPathData? GetVirtualPath( VirtualPathContext context ) => null;

        /// <inheritdoc />
        public Task RouteAsync( RouteContext context )
        {
            var feature = context.HttpContext.Features.Get<IApiVersioningFeature>();
            routePolicy.Evaluate( context, feature.SelectionResult );
            return CompletedTask;
        }
    }
}