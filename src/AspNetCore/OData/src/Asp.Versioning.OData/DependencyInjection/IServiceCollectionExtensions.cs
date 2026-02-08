// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0130

namespace Microsoft.Extensions.DependencyInjection;

using Asp.Versioning.OData;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection.Extensions;
using static Microsoft.Extensions.DependencyInjection.ServiceDescriptor;

/// <summary>
/// Provides extension methods for <see cref="IServiceCollection"/>.
/// </summary>
public static class IServiceCollectionExtensions
{
    extension( IServiceCollection services )
    {
        internal T GetService<T>() => (T) services.LastOrDefault( d => d.ServiceType == typeof( T ) )?.ImplementationInstance!;

        internal ApplicationPartManager GetOrCreateApplicationPartManager()
        {
            var partManager = services.GetService<ApplicationPartManager>();

            if ( partManager == null )
            {
                partManager = new ApplicationPartManager();
                services.TryAddSingleton( partManager );
            }

            partManager.ApplicationParts.Add( new AssemblyPart( typeof( ODataApiVersioningOptions ).Assembly ) );
            return partManager;
        }

        [UnconditionalSuppressMessage( "ILLink", "IL2072", Justification = "Model configuration types are never trimmed" )]
        internal void AddModelConfigurationsAsServices( ApplicationPartManager partManager )
        {
            var feature = new ModelConfigurationFeature();
            var modelConfigurationType = typeof( IModelConfiguration );

            partManager.PopulateFeature( feature );

            foreach ( var modelConfiguration in feature.ModelConfigurations )
            {
                services.TryAddEnumerable( Transient( modelConfigurationType, modelConfiguration ) );
            }
        }

        /// <summary>
        /// Registers discovered <see cref="IModelConfiguration">model configurations</see> as services in the <see cref="IServiceCollection"/>.
        /// </summary>
        public void AddModelConfigurationsAsServices()
        {
            ArgumentNullException.ThrowIfNull( services );

            var partManager = services.GetOrCreateApplicationPartManager();

            if ( partManager.ConfigureDefaultFeatureProviders() )
            {
                services.AddModelConfigurationsAsServices( partManager );
            }
        }
    }

    extension( ApplicationPartManager partManager )
    {
        internal bool ConfigureDefaultFeatureProviders()
        {
            if ( partManager.FeatureProviders.OfType<ModelConfigurationFeatureProvider>().Any() )
            {
                return false;
            }

            partManager.FeatureProviders.Add( new ModelConfigurationFeatureProvider() );
            return true;
        }
    }
}