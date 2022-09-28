// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OData;

using Asp.Versioning.OData.Batch;
using Microsoft.AspNetCore.OData.Batch;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

/// <summary>
/// Represents the possible API versioning options for OData services.
/// </summary>
[CLSCompliant( false )]
public partial class ODataApiVersioningOptions
{
    private Dictionary<string, Action<IServiceCollection>>? configurations;

    /// <summary>
    /// Initializes a new instance of the <see cref="ODataApiVersioningOptions"/> class.
    /// </summary>
    /// <param name="modelBuilder">The associated <see cref="VersionedODataModelBuilder">model builder</see>.</param>
    public ODataApiVersioningOptions( VersionedODataModelBuilder modelBuilder ) =>
        ModelBuilder = modelBuilder ?? throw new ArgumentNullException( nameof( modelBuilder ) );

    /// <summary>
    /// Gets the builder used to create versioned Entity Data Models (EDMs).
    /// </summary>
    /// <value>The associated <see cref="VersionedODataModelBuilder">model builder</see>.</value>
    public VersionedODataModelBuilder ModelBuilder { get; }

    /// <summary>
    /// Gets a value indicating whether the options have any configurations.
    /// </summary>
    /// <value>True if the options has, at least, one configuration; otherwise, false.</value>
    public bool HasConfigurations => configurations is not null && configurations.Count > 0;

    /// <summary>
    /// Gets the collection of model configurations.
    /// </summary>
    /// <value>The <see cref="IReadOnlyDictionary{TKey, TValue}">read-only collection</see> of OData configurations.</value>
    public IReadOnlyDictionary<string, Action<IServiceCollection>> Configurations =>
        configurations ??= new( StringComparer.OrdinalIgnoreCase );

    /// <summary>
    /// Adds an OData configuration for the provided prefix.
    /// </summary>
    /// <returns>The original <see cref="ODataApiVersioningOptions">options</see>.</returns>
    public ODataApiVersioningOptions AddRouteComponents() => AddRouteComponents( string.Empty, static _ => { } );

    /// <summary>
    /// Adds an OData configuration for the provided prefix.
    /// </summary>
    /// <param name="prefix">The associated OData prefix.</param>
    /// <returns>The original <see cref="ODataApiVersioningOptions">options</see>.</returns>
    public virtual ODataApiVersioningOptions AddRouteComponents( string prefix ) =>
        AddRouteComponents( prefix, static _ => { } );

    /// <summary>
    /// Adds an OData configuration for the provided prefix.
    /// </summary>
    /// <param name="configureAction">The configuration <see cref="Action{T}">action</see>.</param>
    /// <returns>The original <see cref="ODataApiVersioningOptions">options</see>.</returns>
    public virtual ODataApiVersioningOptions AddRouteComponents( Action<IServiceCollection> configureAction ) =>
        AddRouteComponents( string.Empty, configureAction );

    /// <summary>
    /// Adds an OData configuration for the provided prefix.
    /// </summary>
    /// <param name="prefix">The associated OData prefix.</param>
    /// <param name="batchHandler">The <see cref="ODataBatchHandler">$batch handler</see>.</param>
    /// <returns>The original <see cref="ODataApiVersioningOptions">options</see>.</returns>
    [CLSCompliant( false )]
    public ODataApiVersioningOptions AddRouteComponents( string prefix, ODataBatchHandler batchHandler ) =>
        AddRouteComponents( prefix, builder => builder.AddSingleton( batchHandler ) );

    /// <summary>
    /// Adds an OData configuration for the provided prefix.
    /// </summary>
    /// <param name="batchHandler">The <see cref="ODataBatchHandler">$batch handler</see>.</param>
    /// <returns>The original <see cref="ODataApiVersioningOptions">options</see>.</returns>
    [CLSCompliant( false )]
    public ODataApiVersioningOptions AddRouteComponents( ODataBatchHandler batchHandler ) =>
        AddRouteComponents( string.Empty, builder => builder.AddSingleton( batchHandler ) );

    /// <summary>
    /// Adds an OData configuration for the provided prefix.
    /// </summary>
    /// <param name="prefix">The associated OData prefix.</param>
    /// <param name="configureAction">The configuration <see cref="Action{T}">action</see>.</param>
    /// <returns>The original <see cref="ODataApiVersioningOptions">options</see>.</returns>
    public virtual ODataApiVersioningOptions AddRouteComponents(
        string prefix,
        Action<IServiceCollection> configureAction )
    {
        configurations ??= new( StringComparer.OrdinalIgnoreCase );
        configurations.Add( prefix, configureAction + AddDefaultServices );
        return this;
    }

    private static void AddDefaultServices( IServiceCollection services ) =>
        services.TryAddSingleton<ODataBatchHandler, VersionedODataBatchHandler>();
}