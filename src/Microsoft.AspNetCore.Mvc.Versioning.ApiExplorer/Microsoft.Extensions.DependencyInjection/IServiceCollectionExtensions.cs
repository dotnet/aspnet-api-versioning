namespace Microsoft.Extensions.DependencyInjection
{
    using Microsoft.AspNetCore.Mvc.ApiExplorer;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Options;
    using System;
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
        public static IServiceCollection AddVersionedApiExplorer( this IServiceCollection services )
        {
            AddApiExplorerServices( services );
            return services;
        }

        /// <summary>
        /// Adds an API explorer that is API version aware.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection">services</see> available in the application.</param>
        /// <param name="setupAction">An <see cref="Action{T}">action</see> used to configure the provided options.</param>
        /// <returns>The original <paramref name="services"/> object.</returns>
        public static IServiceCollection AddVersionedApiExplorer( this IServiceCollection services, Action<ApiExplorerOptions> setupAction )
        {
            AddApiExplorerServices( services );
            services.Configure( setupAction );
            return services;
        }

        static void AddApiExplorerServices( IServiceCollection services )
        {
            if ( services == null )
            {
                throw new ArgumentNullException( nameof( services ) );
            }

            services.AddMvcCore().AddApiExplorer();
            services.TryAdd( Singleton<IOptionsFactory<ApiExplorerOptions>, ApiExplorerOptionsFactory<ApiExplorerOptions>>() );
            services.TryAddSingleton<IApiVersionDescriptionProvider, DefaultApiVersionDescriptionProvider>();
            services.TryAddEnumerable( Transient<IApiDescriptionProvider, VersionedApiDescriptionProvider>() );
        }
    }
}