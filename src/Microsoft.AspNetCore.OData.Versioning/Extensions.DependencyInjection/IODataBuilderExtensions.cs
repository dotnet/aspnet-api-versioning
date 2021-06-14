namespace Microsoft.Extensions.DependencyInjection
{
    using Microsoft.AspNet.OData.Builder;
    using Microsoft.AspNet.OData.Interfaces;
    using Microsoft.AspNet.OData.Routing;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.ApplicationModels;
    using Microsoft.AspNetCore.Mvc.ApplicationParts;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.AspNetCore.Mvc.Routing;
    using Microsoft.AspNetCore.Mvc.Versioning;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using System;
    using System.Linq;
    using static Microsoft.AspNetCore.Mvc.Versioning.ApiVersionParameterLocation;
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
            var partManager = services.GetService<ApplicationPartManager>();

            if ( partManager == null )
            {
                partManager = new ApplicationPartManager();
                services.TryAddSingleton( partManager );
            }

            partManager.ApplicationParts.Add( new AssemblyPart( typeof( IODataBuilderExtensions ).Assembly ) );

            ConfigureDefaultFeatureProviders( partManager );
            services.Replace( Singleton<IActionSelector, ODataApiVersionActionSelector>() );
            services.TryAdd( Transient<VersionedODataModelBuilder, VersionedODataModelBuilder>() );
            services.TryAdd( Singleton<IODataRouteCollectionProvider, ODataRouteCollectionProvider>() );
            services.AddTransient<IApplicationModelProvider, ODataApplicationModelProvider>();
            services.AddTransient<IActionDescriptorProvider, ODataActionDescriptorProvider>();
            services.AddSingleton<ODataActionDescriptorChangeProvider>();
            services.AddSingleton<IActionDescriptorChangeProvider>( sp => sp.GetRequiredService<ODataActionDescriptorChangeProvider>() );
            services.TryAddEnumerable( Transient<IApiControllerSpecification, ODataControllerSpecification>() );
            services.AddTransient<IStartupFilter, RaiseVersionedODataRoutesMapped>();
            services.AddModelConfigurationsAsServices( partManager );
            services.TryReplace( WithLinkGeneratorDecorator( services ) );
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

        static IServiceCollection TryReplace( this IServiceCollection services, ServiceDescriptor? descriptor )
        {
            if ( descriptor != null )
            {
                services.Replace( descriptor );
            }

            return services;
        }

        static void ConfigureDefaultFeatureProviders( ApplicationPartManager partManager )
        {
            if ( !partManager.FeatureProviders.OfType<ModelConfigurationFeatureProvider>().Any() )
            {
                partManager.FeatureProviders.Add( new ModelConfigurationFeatureProvider() );
            }
        }

        static ServiceDescriptor? WithLinkGeneratorDecorator( IServiceCollection services )
        {
            // HACK: even though the core api versioning services decorate the default LinkGenerator, we need to get in front of the odata
            // implementation in order to add the necessary route values when versioning by url segment.
            //
            // REF: https://github.com/OData/WebApi/blob/master/src/Microsoft.AspNetCore.OData/Extensions/ODataServiceCollectionExtensions.cs#L99
            // REF: https://github.com/OData/WebApi/blob/master/src/Microsoft.AspNetCore.OData/Extensions/Endpoint/ODataEndpointLinkGenerator.cs
            var descriptor = services.FirstOrDefault( sd => sd.ServiceType == typeof( LinkGenerator ) );

            if ( descriptor == null )
            {
                return default;
            }

            var lifetime = descriptor.Lifetime;
            var factory = descriptor.ImplementationFactory;

            if ( factory == null )
            {
                throw new InvalidOperationException( LocalSR.MissingLinkGenerator );
            }

            LinkGenerator NewFactory( IServiceProvider serviceProvider )
            {
                var instance = (LinkGenerator) factory( serviceProvider );
                var source = serviceProvider.GetRequiredService<IApiVersionParameterSource>();
                var context = new UrlSegmentDescriptionContext();

                source.AddParameters( context );

                if ( context.HasPathApiVersion )
                {
                    instance = new ApiVersionLinkGenerator( instance );
                }

                return instance;
            }

            return Describe( typeof( LinkGenerator ), NewFactory, lifetime );
        }

        sealed class UrlSegmentDescriptionContext : IApiVersionParameterDescriptionContext
        {
            internal bool HasPathApiVersion { get; private set; }

            public void AddParameter( string name, ApiVersionParameterLocation location ) => HasPathApiVersion |= location == Path;
        }
    }
}