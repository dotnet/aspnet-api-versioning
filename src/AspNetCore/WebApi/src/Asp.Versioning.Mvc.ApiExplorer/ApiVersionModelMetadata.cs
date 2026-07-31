// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.ApiExplorer;

using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

/// <summary>
/// Represents the model metadata for an <see cref="ApiVersion">API version</see>.
/// </summary>
[CLSCompliant( false )]
public sealed class ApiVersionModelMetadata : DelegatingModelMetadata
{
    private readonly string description;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiVersionModelMetadata"/> class.
    /// </summary>
    /// <param name="modelMetadataProvider">The <see cref="IModelMetadataProvider">model metadata provider</see>
    /// used to create the new instance.</param>
    /// <param name="description">The description associated with the model metadata.</param>
    public ApiVersionModelMetadata( IModelMetadataProvider modelMetadataProvider, string description )
        : base(
            NewInner( modelMetadataProvider ),
            ModelMetadataIdentity.ForType( typeof( string ) ) ) => this.description = description;

    /// <inheritdoc />
    public override string DataTypeName => nameof( ApiVersion );

    /// <inheritdoc />
    public override string Description => description;

    /// <inheritdoc />
    public override string? DisplayName => SR.ApiVersionDisplayName;

    private static ModelMetadata NewInner( IModelMetadataProvider modelMetadataProvider )
    {
        ArgumentNullException.ThrowIfNull( modelMetadataProvider );
        return modelMetadataProvider.GetMetadataForType( typeof( string ) );
    }
}