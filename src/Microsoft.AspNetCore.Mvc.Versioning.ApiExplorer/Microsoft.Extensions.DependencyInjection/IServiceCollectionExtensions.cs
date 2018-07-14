namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Diagnostics.Contracts;
    using Microsoft.AspNetCore.Mvc.ApiExplorer;
    using Microsoft.AspNetCore.Mvc.Versioning;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Options;
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
        public static IServiceCollection AddVersionedApiExplorer( this IServiceCollection services ) => services.AddVersionedApiExplorer( _ => { } );

        /// <summary>
        /// Adds an API explorer that is API version aware.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection">services</see> available in the application.</param>
        /// <param name="setupAction">An <see cref="Action{T}">action</see> used to configure the provided options.</param>
        /// <returns>The original <paramref name="services"/> object.</returns>
        public static IServiceCollection AddVersionedApiExplorer( this IServiceCollection services, Action<ApiExplorerOptions> setupAction )
        {
            Arg.NotNull( services, nameof( services ) );
            Arg.NotNull( setupAction, nameof( setupAction ) );

            services.AddMvcCore().AddApiExplorer();
            services.Add( Singleton( serviceProvider => NewOptions( serviceProvider, setupAction ) ) );
            services.TryAddSingleton<IApiVersionDescriptionProvider, DefaultApiVersionDescriptionProvider>();
            services.TryAddSingleton<IApiDescriptionGroupCollectionProvider, ApiDescriptionGroupCollectionProvider>();
            services.TryAddEnumerable( Transient<IApiDescriptionProvider, VersionedApiDescriptionProvider>() );

            return services;
        }

        static IOptions<ApiExplorerOptions> NewOptions( IServiceProvider serviceProvider, Action<ApiExplorerOptions> setupAction )
        {
            Contract.Requires( serviceProvider != null );
            Contract.Requires( setupAction != null );
            Contract.Ensures( Contract.Result<IOptions<ApiExplorerOptions>>() != null );

            var versioningOptions = serviceProvider.GetService<IOptions<ApiVersioningOptions>>()?.Value;
            var options = new ApiExplorerOptions();

            if ( versioningOptions != null )
            {
                options.DefaultApiVersion = versioningOptions.DefaultApiVersion;
                options.ApiVersionParameterSource = versioningOptions.ApiVersionReader;
                options.AssumeDefaultVersionWhenUnspecified = versioningOptions.AssumeDefaultVersionWhenUnspecified;
            }

            setupAction( options );

            return new OptionsWrapper<ApiExplorerOptions>( options );
        }
    }
}