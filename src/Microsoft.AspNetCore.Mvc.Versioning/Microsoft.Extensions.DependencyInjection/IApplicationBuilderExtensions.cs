namespace Microsoft.Extensions.DependencyInjection
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Mvc.Routing;
    using System;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Provides extension methods for the <see cref="IApplicationBuilder"/> interface.
    /// </summary>
    [CLSCompliant( false )]
    public static class IApplicationBuilderExtensions
    {
        /// <summary>
        /// Adds API versioning to the <see cref="IApplicationBuilder"/> request execution pipeline.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder">application builder</see> to add API versioning to.</param>
        /// <returns>The original <see cref="IApplicationBuilder"/>.</returns>
        public static IApplicationBuilder UseApiVersioning( this IApplicationBuilder app )
        {
            Arg.NotNull( app, nameof( app ) );
            Contract.Ensures( Contract.Result<IApplicationBuilder>() != null );

            app.UseMvc( builder => builder.Routes.Add( builder.ServiceProvider.GetRequiredService<IApiVersionRoutePolicy>() ) );
            return app;
        }
    }
}