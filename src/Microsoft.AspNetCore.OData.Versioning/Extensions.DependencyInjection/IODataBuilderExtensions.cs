namespace Microsoft.Extensions.DependencyInjection
{
    using Microsoft.AspNet.OData.Builder;
    using Microsoft.AspNet.OData.Interfaces;
    using Microsoft.AspNet.OData.Routing;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.ApplicationModels;
    using Microsoft.AspNetCore.Mvc.ApplicationParts;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.AspNetCore.Mvc.Versioning;
    using Microsoft.Extensions.DependencyInjection.Extensions;
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
        public static IODataBuilder EnableApiVersioning( this IODataBuilder builder )
        {
            if ( builder == null )
            {
                throw new ArgumentNullException( nameof( builder ) );
            }

            AddODataServices( builder.Services );
            return builder;
        }

        /// <summary>
        /// Enables service API versioning for the specified OData configuration.
        /// </summary>
        /// <param name="builder">The <see cref="IODataBuilder">OData builder</see> available in the application.</param>
        /// <param name="setupAction">An <see cref="Action{T}">action</see> used to configure the provided options.</param>
        /// <returns>The original <paramref name="builder"/> object.</returns>
        public static IODataBuilder EnableApiVersioning( this IODataBuilder builder, Action<ODataApiVersioningOptions> setupAction )
        {
            if ( builder == null )
            {
                throw new ArgumentNullException( nameof( builder ) );
            }

            var services = builder.Services;

            AddODataServices( services );
            services.Configure( setupAction );

            return builder;
        }

        static void AddODataServices( IServiceCollection services )
        {
            // note: if we end up creating a new ApplicationPartManager here we won't fail, but the setup
            // will not register any model configurations automatically. this is almost certainly because
            // services.AddMvcCore() hasn't be called yet, which is unexpected
            var partManager = services.GetService<ApplicationPartManager>() ?? new ApplicationPartManager();

            ConfigureDefaultFeatureProviders( partManager );
            services.Replace( Singleton<IActionSelector, ODataApiVersionActionSelector>() );
            services.TryAdd( Transient<VersionedODataModelBuilder, VersionedODataModelBuilder>() );
            services.TryAdd( Singleton<IODataRouteCollectionProvider, ODataRouteCollectionProvider>() );
            services.AddTransient<IApplicationModelProvider, ODataApplicationModelProvider>();
            services.AddTransient<IActionDescriptorProvider, ODataActionDescriptorProvider>();
            services.AddSingleton<IActionDescriptorChangeProvider>( ODataActionDescriptorChangeProvider.Instance );
            services.TryAddEnumerable( Transient<IApiControllerSpecification, ODataControllerSpecification>() );
            services.AddModelConfigurationsAsServices( partManager );
        }

        static T GetService<T>( this IServiceCollection services ) => (T) services.LastOrDefault( d => d.ServiceType == typeof( T ) )?.ImplementationInstance!;

        static void AddModelConfigurationsAsServices( this IServiceCollection services, ApplicationPartManager partManager )
        {
            var feature = new ModelConfigurationFeature();
            var modelConfigurationType = typeof( IModelConfiguration );

            partManager.PopulateFeature( feature );

            foreach ( var modelConfiguration in feature.ModelConfigurations.Select( t => t.AsType() ) )
            {
                services.TryAddEnumerable( Transient( modelConfigurationType, modelConfiguration ) );
            }
        }

        static void ConfigureDefaultFeatureProviders( ApplicationPartManager partManager )
        {
            if ( !partManager.FeatureProviders.OfType<ModelConfigurationFeatureProvider>().Any() )
            {
                partManager.FeatureProviders.Add( new ModelConfigurationFeatureProvider() );
            }
        }
    }
}