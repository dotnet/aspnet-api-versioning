namespace Microsoft.Extensions.DependencyInjection
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.Routing;
    using Microsoft.AspNetCore.Mvc.Versioning;
    using Microsoft.Extensions.Options;
    using System;
    using System.Diagnostics.Contracts;

    sealed class AutoRegisterMiddleware : IStartupFilter
    {
        readonly IApiVersionRoutePolicy routePolicy;
        readonly IOptions<ApiVersioningOptions> options;

        public AutoRegisterMiddleware( IApiVersionRoutePolicy routePolicy, IOptions<ApiVersioningOptions> options )
        {
            Contract.Requires( routePolicy != null );
            Contract.Requires( options != null );

            this.routePolicy = routePolicy;
            this.options = options;
        }

        public Action<IApplicationBuilder> Configure( Action<IApplicationBuilder> next )
        {
            Contract.Requires( next != null );
            Contract.Ensures( Contract.Result<Action<IApplicationBuilder>>() != null );

            return app =>
            {
                if ( options.Value.RegisterMiddleware )
                {
                    app.UseApiVersioning();
                }

                next( app );
                app.UseRouter( builder => builder.Routes.Add( new CatchAllRouteHandler( routePolicy ) ) );
            };
        }
    }
}