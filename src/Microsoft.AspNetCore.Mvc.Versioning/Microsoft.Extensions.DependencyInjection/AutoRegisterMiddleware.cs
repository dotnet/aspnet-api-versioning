#pragma warning disable CA1812

namespace Microsoft.Extensions.DependencyInjection
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Routing;
    using Microsoft.AspNetCore.Mvc.Versioning;
    using Microsoft.Extensions.Options;
    using System;

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
            this.routePolicy = routePolicy;
            this.options = options;
            this.mvcOptions = mvcOptions;
        }

        public Action<IApplicationBuilder> Configure( Action<IApplicationBuilder> next )
        {
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