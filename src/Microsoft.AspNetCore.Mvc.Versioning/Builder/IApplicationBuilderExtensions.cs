namespace Microsoft.AspNetCore.Builder
{
    using Microsoft.AspNetCore.Mvc.Versioning;
    using System;

    /// <summary>
    /// Provides extension methods for the <see cref="IApplicationBuilder"/> interface.
    /// </summary>
    [CLSCompliant( false )]
    public static class IApplicationBuilderExtensions
    {
        /// <summary>
        /// Uses the API versioning middleware.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder">application</see> to configure.</param>
        /// <returns>The original <see cref="IApplicationBuilder"/>.</returns>
        /// <remarks>
        /// <para>
        /// Configuration of the API versioning middleware is not necessary unless you explicitly
        /// want to control when the middleware is added in the pipeline. API versioning automatically configures
        /// the required middleware, which is usually early enough for dependent middleware.
        /// </para>
        /// <para>
        /// In addition to invoking <see cref="UseApiVersioning(IApplicationBuilder)"/>, you must also set
        /// <see cref="ApiVersioningOptions.RegisterMiddleware"/> to <c>false</c>.
        /// </para>
        /// </remarks>
        public static IApplicationBuilder UseApiVersioning( this IApplicationBuilder app ) =>
            app.UseMiddleware<ApiVersioningMiddleware>();
    }
}