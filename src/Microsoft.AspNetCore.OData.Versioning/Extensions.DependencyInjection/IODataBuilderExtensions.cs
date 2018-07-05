namespace Microsoft.Extensions.DependencyInjection
{
    using AspNetCore.Mvc.Infrastructure;
    using AspNetCore.Mvc.Versioning;
    using Extensions;
    using Microsoft.AspNet.OData.Builder;
    using Microsoft.AspNet.OData.Interfaces;
    using Microsoft.Extensions.Options;
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
        public static IODataBuilder EnableApiVersioning( this IODataBuilder builder ) => builder.EnableApiVersioning( _ => { } );

        /// <summary>
        /// Enables service API versioning for the specified OData configuration.
        /// </summary>
        /// <param name="builder">The <see cref="IODataBuilder">OData builder</see> available in the application.</param>
        /// <param name="setupAction">An <see cref="Action{T}">action</see> used to configure the provided options.</param>
        /// <returns>The original <paramref name="builder"/> object.</returns>
        public static IODataBuilder EnableApiVersioning( this IODataBuilder builder, Action<ODataApiVersioningOptions> setupAction )
        {
            Arg.NotNull( builder, nameof( builder ) );
            Arg.NotNull( setupAction, nameof( setupAction ) );

            var options = new ODataApiVersioningOptions();
            var services = builder.Services;

            setupAction( options );
            services.Add( Singleton<IOptions<ODataApiVersioningOptions>>( new OptionsWrapper<ODataApiVersioningOptions>( options ) ) );
            services.RemoveAll<IActionSelector>();
            services.Replace( Singleton<IActionSelector, ODataApiVersionActionSelector>() );
            services.TryAdd( Transient<VersionedODataModelBuilder, VersionedODataModelBuilder>() );
            services.AddMvcCore( mvcOptions => mvcOptions.Conventions.Add( new MetadataControllerConvention( options ) ) );

            return builder;
        }
    }
}