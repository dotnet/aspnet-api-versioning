// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Microsoft.Extensions.DependencyInjection;

using Asp.Versioning.OData;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Runtime.CompilerServices;
using static Microsoft.Extensions.DependencyInjection.ServiceDescriptor;

/// <summary>
/// Provides extension methods for <see cref="IServiceCollection"/>.
/// </summary>
public static class IServiceCollectionExtensions
{
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static T GetService<T>( this IServiceCollection services ) =>
        (T) services.LastOrDefault( d => d.ServiceType == typeof( T ) )?.ImplementationInstance!;

    internal static ApplicationPartManager GetOrCreateApplicationPartManager( this IServiceCollection services )
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

    internal static void AddModelConfigurationsAsServices( this IServiceCollection services, ApplicationPartManager partManager )
    {
        var feature = new ModelConfigurationFeature();
        var modelConfigurationType = typeof( IModelConfiguration );

        partManager.PopulateFeature( feature );

        foreach ( var modelConfiguration in feature.ModelConfigurations )
        {
            services.TryAddEnumerable( Transient( modelConfigurationType, modelConfiguration ) );
        }
    }

    internal static bool ConfigureDefaultFeatureProviders( this ApplicationPartManager partManager )
    {
        if ( partManager.FeatureProviders.OfType<ModelConfigurationFeatureProvider>().Any() )
        {
            return false;
        }

        partManager.FeatureProviders.Add( new ModelConfigurationFeatureProvider() );
        return true;
    }

    /// <summary>
    /// Registers discovered <see cref="IModelConfiguration">model configurations</see> as services in the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The extended <see cref="IServiceCollection"/>.</param>
    public static void AddModelConfigurationsAsServices( this IServiceCollection services )
    {
        ArgumentNullException.ThrowIfNull( services );

        var partManager = services.GetOrCreateApplicationPartManager();

        if ( ConfigureDefaultFeatureProviders( partManager ) )
        {
            services.AddModelConfigurationsAsServices( partManager );
        }
    }
}