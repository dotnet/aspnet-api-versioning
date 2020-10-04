namespace Microsoft.AspNetCore.Mvc.Routing
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc.Versioning;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;

    static class RouteContextExtensions
    {
        // HACK: supports OData which will call back through IActionSelector in its endpoint routing implementation
        internal static void SetHandlerOrEndpoint( this RouteContext context, RequestHandler? handler )
        {
            var httpContext = context.HttpContext;
            var options = httpContext.RequestServices.GetRequiredService<IOptions<MvcOptions>>().Value;

            if ( options.EnableEndpointRouting )
            {
                httpContext.SetEndpoint( handler );
            }
            else
            {
                context.Handler = handler!;
            }
        }
    }
}