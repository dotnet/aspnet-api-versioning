namespace Microsoft.Extensions.DependencyInjection
{
    using Extensions;
    using Microsoft.AspNetCore.Mvc.ApiExplorer;
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
        /// <param name="builder">The <see cref="IMvcCoreBuilder">core MVC builder</see> available in the application</param>
        /// <returns>The original <see cref="IMvcCoreBuilder"/> instance.</returns>
        public static IMvcCoreBuilder AddVersionedApiExplorer( this IMvcCoreBuilder builder )
        {
            Arg.NotNull( builder, nameof( builder ) );

            builder.Services.TryAddSingleton<IApiVersionGroupNameFormatter, DefaultApiVersionGroupNameFormatter>();
            builder.Services.TryAddSingleton<IApiVersionDescriptionProvider, DefaultApiVersionDescriptionProvider>();
            builder.Services.TryAddSingleton<IApiDescriptionGroupCollectionProvider, ApiDescriptionGroupCollectionProvider>();
            builder.Services.TryAddEnumerable( Transient<IApiDescriptionProvider, VersionedApiDescriptionProvider>() );

            return builder;
        }
    }
}