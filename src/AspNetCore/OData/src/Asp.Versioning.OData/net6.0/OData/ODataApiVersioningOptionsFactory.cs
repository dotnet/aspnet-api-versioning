// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OData;

using Microsoft.Extensions.Options;

/// <summary>
/// Represents a factory to create API versioning options specific to OData.
/// </summary>
[CLSCompliant( false )]
public class ODataApiVersioningOptionsFactory : OptionsFactory<ODataApiVersioningOptions>
{
    private readonly VersionedODataModelBuilder modelBuilder;

    /// <summary>
    /// Initializes a new instance of the <see cref="ODataApiVersioningOptionsFactory"/> class.
    /// </summary>
    /// <param name="modelBuilder">The associated <see cref="VersionedODataModelBuilder">model builder</see>.</param>
    /// <param name="setups">The <see cref="IEnumerable{T}">sequence</see> of
    /// <see cref="IConfigureOptions{TOptions}">configuration actions</see> to run.</param>
    /// <param name="postConfigures">The <see cref="IEnumerable{T}">sequence</see> of
    /// <see cref="IPostConfigureOptions{TOptions}">initialization actions</see> to run.</param>
    public ODataApiVersioningOptionsFactory(
        VersionedODataModelBuilder modelBuilder,
        IEnumerable<IConfigureOptions<ODataApiVersioningOptions>> setups,
        IEnumerable<IPostConfigureOptions<ODataApiVersioningOptions>> postConfigures )
        : base( setups, postConfigures ) =>
        this.modelBuilder = modelBuilder ?? throw new ArgumentNullException( nameof( modelBuilder ) );

    /// <inheritdoc />
    protected override ODataApiVersioningOptions CreateInstance( string name ) => new( modelBuilder );
}