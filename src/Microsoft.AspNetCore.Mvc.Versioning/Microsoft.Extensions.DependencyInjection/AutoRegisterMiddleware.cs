namespace Microsoft.Extensions.DependencyInjection
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Routing;
    using Microsoft.AspNetCore.Mvc.Versioning;
    using Microsoft.Extensions.Options;
    using System;
    using System.Diagnostics.Contracts;

    sealed class AutoRegisterMiddleware : IStartupFilter
    {
        readonly IApiVersionRoutePolicy routePolicy;
        readonly IOptions<ApiVersioningOptions> options;
        readonly IOptions<MvcOptions> mvcOptions;

        public AutoRegisterMiddleware(
            IApiVersionRoutePolicy routePolicy,
            IOptions<ApiVersioningOptions> options,
            IOptions<MvcOptions> mvcOptions )
        {
            Contract.Requires( routePolicy != null );
            Contract.Requires( options != null );
            Contract.Requires( mvcOptions != null );

            this.routePolicy = routePolicy;
            this.options = options;
            this.mvcOptions = mvcOptions;
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

                if ( !mvcOptions.Value.EnableEndpointRouting )
                {
                    app.UseRouter( builder => builder.Routes.Add( new CatchAllRouteHandler( routePolicy ) ) );
                }
            };
        }
    }
}