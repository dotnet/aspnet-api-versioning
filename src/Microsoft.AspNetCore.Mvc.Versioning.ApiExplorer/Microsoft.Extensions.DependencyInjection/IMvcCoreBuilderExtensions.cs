namespace Microsoft.Extensions.DependencyInjection
{
    using Extensions;
    using Microsoft.AspNetCore.Mvc.ApiExplorer;
    using Microsoft.AspNetCore.Mvc.Versioning;
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
        /// <param name="builder">The <see cref="IMvcCoreBuilder">core MVC builder</see> available in the application</param>
        /// <returns>The original <see cref="IMvcCoreBuilder"/> instance.</returns>
        public static IMvcCoreBuilder AddVersionedApiExplorer( this IMvcCoreBuilder builder ) => builder.AddVersionedApiExplorer( _ => { } );

        /// <summary>
        /// Adds an API explorer that is API version aware.
        /// </summary>
        /// <param name="builder">The <see cref="IMvcCoreBuilder">core MVC builder</see> available in the application</param>
        /// <param name="setupAction">An <see cref="Action{T}">action</see> used to configure the provided options.</param>
        /// <returns>The original <see cref="IMvcCoreBuilder"/> instance.</returns>
        public static IMvcCoreBuilder AddVersionedApiExplorer( this IMvcCoreBuilder builder, Action<ApiExplorerOptions> setupAction )
        {
            Arg.NotNull( builder, nameof( builder ) );
            Arg.NotNull( setupAction, nameof( setupAction ) );

            builder.Services.Add( Singleton( serviceProvider => NewOptions( serviceProvider, setupAction ) ) );
            builder.Services.TryAddSingleton<IApiVersionDescriptionProvider, DefaultApiVersionDescriptionProvider>();
            builder.Services.TryAddSingleton<IApiDescriptionGroupCollectionProvider, ApiDescriptionGroupCollectionProvider>();
            builder.Services.TryAddEnumerable( Transient<IApiDescriptionProvider, VersionedApiDescriptionProvider>() );

            return builder;
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