namespace Microsoft.Extensions.DependencyInjection
{
    using AspNetCore.Mvc.Infrastructure;
    using AspNetCore.Mvc.Versioning;
    using Extensions;
    using Microsoft.AspNet.OData.Builder;
    using Microsoft.AspNet.OData.Interfaces;
    using Microsoft.AspNetCore.Mvc.ApplicationModels;
    using System;
    using static ServiceDescriptor;

    /// <summary>
    /// Provides extension methods for the <see cref="IODataBuilder"/> interface.
    /// </summary>
    [CLSCompliant( false )]
    public static class IODataBuilderExtensions
    {
        /// <summary>
        /// Enables service API versioning for the specified OData configuration.
        /// </summary>
        /// <param name="builder">The <see cref="IODataBuilder">OData builder</see> available in the application.</param>
        /// <returns>The original <paramref name="builder"/> object.</returns>
        public static IODataBuilder EnableApiVersioning( this IODataBuilder builder )
        {
            Arg.NotNull( builder, nameof( builder ) );

            var services = builder.Services;

            services.RemoveAll<IActionSelector>();
            services.Replace( Singleton<IActionSelector, ODataApiVersionActionSelector>() );
            services.TryAdd( Singleton<IODataApiVersionProvider, ODataApiVersionProvider>() );
            services.TryAddEnumerable( Transient<IApplicationModelProvider, MetadataControllerConfiguration>() );
            services.TryAdd( Transient<VersionedODataModelBuilder, VersionedODataModelBuilder>() );

            return builder;
        }
    }
}