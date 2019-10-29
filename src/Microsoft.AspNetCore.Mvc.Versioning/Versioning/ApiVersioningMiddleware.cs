#pragma warning disable CA1812

namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using Microsoft.AspNetCore.Http;
    using System.Threading.Tasks;

    sealed class ApiVersioningMiddleware
    {
        readonly RequestDelegate next;

        public ApiVersioningMiddleware( RequestDelegate next ) => this.next = next;

        public Task InvokeAsync( HttpContext context )
        {
            var feature = context.Features.Get<IApiVersioningFeature>();

            if ( feature == null )
            {
                feature = new ApiVersioningFeature( context );
                context.Features.Set( feature );
            }

            return next( context );
        }
    }
}