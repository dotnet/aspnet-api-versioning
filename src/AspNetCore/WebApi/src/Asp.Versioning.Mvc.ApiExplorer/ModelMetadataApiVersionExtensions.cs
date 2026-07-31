// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.ApiExplorer;

using Microsoft.AspNetCore.Mvc.ModelBinding;

/// <summary>
/// Provides API version related extension methods for <see cref="ModelMetadata">model metadata</see>.
/// </summary>
/// <remarks>
/// The API version a model was described for is recorded in <see cref="ModelMetadata.AdditionalValues"/> keyed by
/// <see cref="ApiVersion"/>. A well-known key is used rather than a shared type because the packages that produce
/// versioned metadata do not all reference each other by design.
/// </remarks>
[CLSCompliant( false )]
public static class ModelMetadataApiVersionExtensions
{
    /// <param name="metadata">The extended <see cref="ModelMetadata">model metadata</see>.</param>
    extension( ModelMetadata metadata )
    {
        /// <summary>
        /// Gets the API version the model metadata was described for, if any.
        /// </summary>
        /// <value>The described <see cref="ApiVersion">API version</see>, or <c>null</c> if the metadata is not
        /// specific to an API version.</value>
        /// <remarks>Metadata that reports an API version is describing a subset of its model type. Metadata that
        /// does not is describing the type as declared, which is not the same as describing a subset with no
        /// members.</remarks>
        public ApiVersion? DescribedApiVersion
        {
            get
            {
                ArgumentNullException.ThrowIfNull( metadata );

                return metadata.AdditionalValues.TryGetValue( typeof( ApiVersion ), out var value )
                    ? value as ApiVersion
                    : default;
            }
        }
    }
}