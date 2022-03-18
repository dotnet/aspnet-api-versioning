// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Microsoft.OData;

using Asp.Versioning.OData;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using System.Runtime.CompilerServices;
using System.Web.Http;
using static Asp.Versioning.Routing.VersionedODataRoutingConventions;
using static Microsoft.OData.ServiceLifetime;

/// <summary>
/// Provides extension methods for the <see cref="IContainerBuilder"/> interface.
/// </summary>
public static class IContainerBuilderExtensions
{
    /// <summary>
    /// Adds service API versioning to the specified container builder.
    /// </summary>
    /// <param name="builder">The extended <see cref="IContainerBuilder">container builder</see>.</param>
    /// <param name="routeName">The name of the route to add API versioning to.</param>
    /// <param name="models">The <see cref="IEnumerable{T}">sequence</see> of <see cref="IEdmModel">EDM models</see> to use for parsing OData paths.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    public static IContainerBuilder AddApiVersioning( this IContainerBuilder builder, string routeName, IEnumerable<IEdmModel> models ) =>
        builder.AddService( Transient, sp => sp.GetRequiredService<IEdmModelSelector>().SelectModel( sp ) )
               .AddService( Singleton, sp => NewEdmModelSelector( sp, models ) )
               .AddService( Singleton, sp => NewRoutingConventions( sp, routeName ) );

    /// <summary>
    /// Adds service API versioning to the specified container builder.
    /// </summary>
    /// <param name="builder">The extended <see cref="IContainerBuilder">container builder</see>.</param>
    /// <param name="models">The <see cref="IEnumerable{T}">sequence</see> of <see cref="IEdmModel">EDM models</see> to use for parsing OData paths.</param>
    /// <param name="routingConventions">The OData routing conventions to use for controller and action selection.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    public static IContainerBuilder AddApiVersioning(
        this IContainerBuilder builder,
        IEnumerable<IEdmModel> models,
        IEnumerable<IODataRoutingConvention> routingConventions ) =>
        builder.AddService( Transient, sp => sp.GetRequiredService<IEdmModelSelector>().SelectModel( sp ) )
               .AddService( Singleton, sp => NewEdmModelSelector( sp, models ) )
               .AddService( Singleton, sp => AddOrUpdate( routingConventions.ToList() ).AsEnumerable() );

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static IEnumerable<IODataRoutingConvention> NewRoutingConventions( IServiceProvider serviceProvider, string routeName ) =>
        CreateDefaultWithAttributeRouting( routeName, serviceProvider.GetRequiredService<HttpConfiguration>() );

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static IEdmModelSelector NewEdmModelSelector( IServiceProvider serviceProvider, IEnumerable<IEdmModel> models )
    {
        var options = serviceProvider.GetRequiredService<HttpConfiguration>().GetApiVersioningOptions();
        return new EdmModelSelector( models, options.ApiVersionSelector );
    }
}