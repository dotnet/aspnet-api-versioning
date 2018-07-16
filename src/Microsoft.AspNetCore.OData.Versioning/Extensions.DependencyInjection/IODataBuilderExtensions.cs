namespace Microsoft.Extensions.DependencyInjection
{
    using Microsoft.AspNet.OData.Builder;
    using Microsoft.AspNet.OData.Interfaces;
    using Microsoft.AspNet.OData.Routing;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.ApplicationParts;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.AspNetCore.Mvc.Versioning;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Options;
    using System;
    using System.Linq;
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
            var mvcCore = services.AddMvcCore();

            setupAction( options );
            ConfigureDefaultFeatureProviders( mvcCore.PartManager );
            services.Add( Singleton<IOptions<ODataApiVersioningOptions>>( new OptionsWrapper<ODataApiVersioningOptions>( options ) ) );
            services.RemoveAll<IActionSelector>();
            services.Replace( Singleton<IActionSelector, ODataApiVersionActionSelector>() );
            services.TryAdd( Transient<VersionedODataModelBuilder, VersionedODataModelBuilder>() );
            services.TryAdd( Singleton<IODataRouteCollectionProvider, ODataRouteCollectionProvider>() );
            services.TryAddEnumerable( Transient<IActionDescriptorProvider, ODataSupportedHttpMethodProvider>() );
            services.AddMvcCore( mvcOptions => mvcOptions.Conventions.Add( new MetadataControllerConvention( options ) ) );
            AddModelConfigurationsAsServices( mvcCore, services );

            return builder;
        }

        static void ConfigureDefaultFeatureProviders( ApplicationPartManager partManager )
        {
            if ( !partManager.FeatureProviders.OfType<ModelConfigurationFeatureProvider>().Any() )
            {
                partManager.FeatureProviders.Add( new ModelConfigurationFeatureProvider() );
            }
        }

        static void AddModelConfigurationsAsServices( IMvcCoreBuilder builder, IServiceCollection services )
        {
            var feature = new ModelConfigurationFeature();
            var modelConfigurationType = typeof( IModelConfiguration );

            builder.PartManager.PopulateFeature( feature );

            foreach ( var modelConfiguration in feature.ModelConfigurations.Select( t => t.AsType() ) )
            {
                services.TryAddEnumerable( Transient( modelConfigurationType, modelConfiguration ) );
            }
        }
    }
}