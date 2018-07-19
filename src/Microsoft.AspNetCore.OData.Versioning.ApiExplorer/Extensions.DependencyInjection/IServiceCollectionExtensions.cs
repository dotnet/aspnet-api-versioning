namespace Microsoft.Extensions.DependencyInjection
{
    using Microsoft.AspNetCore.Mvc.ApiExplorer;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Options;
    using System;
    using System.Diagnostics.Contracts;
    using static ServiceDescriptor;

    /// <summary>
    /// Provides extension methods for the <see cref="IServiceCollection"/> interface.
    /// </summary>
    [CLSCompliant( false )]
    public static class IServiceCollectionExtensions
    {
        /// <summary>
        /// Adds an API explorer that is API version aware.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection">services</see> available in the application.</param>
        /// <returns>The original <paramref name="services"/> object.</returns>
        public static IServiceCollection AddODataApiExplorer( this IServiceCollection services )
        {
            Arg.NotNull( services, nameof( services ) );
            Contract.Ensures( Contract.Result<IServiceCollection>() != null );

            AddApiExplorerServices( services );

            return services;
        }

        /// <summary>
        /// Adds an API explorer that is API version aware.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection">services</see> available in the application.</param>
        /// <param name="setupAction">An <see cref="Action{T}">action</see> used to configure the provided options.</param>
        /// <returns>The original <paramref name="services"/> object.</returns>
        public static IServiceCollection AddODataApiExplorer( this IServiceCollection services, Action<ODataApiExplorerOptions> setupAction )
        {
            Arg.NotNull( services, nameof( services ) );
            Arg.NotNull( setupAction, nameof( setupAction ) );
            Contract.Ensures( Contract.Result<IServiceCollection>() != null );

            AddApiExplorerServices( services );
            services.Configure( setupAction );

            return services;
        }

        static void AddApiExplorerServices( IServiceCollection services )
        {
            services.AddVersionedApiExplorer();
            services.TryAdd( Singleton<IOptionsFactory<ODataApiExplorerOptions>, ApiExplorerOptionsFactory<ODataApiExplorerOptions>>() );
            services.Replace( Singleton( typeof( ApiExplorerOptions ), sp => sp.GetRequiredService<ODataApiExplorerOptions>() ) );
            services.TryAddEnumerable( Transient<IApiDescriptionProvider, ODataApiDescriptionProvider>() );
        }
    }
}