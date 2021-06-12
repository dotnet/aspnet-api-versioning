#pragma warning disable CA1812

namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Options;
    using System.Threading.Tasks;

    sealed class ApiVersioningMiddleware
    {
        readonly RequestDelegate next;
        readonly bool usingLegacyRouting;

        public ApiVersioningMiddleware( RequestDelegate next, IOptions<MvcOptions> options )
        {
            this.next = next;
            usingLegacyRouting = !options.Value.EnableEndpointRouting;
        }

        public Task InvokeAsync( HttpContext context )
        {
            var feature = context.Features.Get<IApiVersioningFeature>();

            if ( feature == null )
            {
                feature = new ApiVersioningFeature( context );
                context.Features.Set( feature );
            }
            else
            {
                feature.RouteParameter = null;

                if ( usingLegacyRouting )
                {
                    feature.SelectionResult.Clear();
                }
            }

            return next( context );
        }
    }
}