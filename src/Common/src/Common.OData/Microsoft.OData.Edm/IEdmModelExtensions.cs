// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Microsoft.OData.Edm;

using Asp.Versioning;
using Asp.Versioning.OData;

/// <summary>
/// Provides extension methods for <see cref="IEdmModel"/>.
/// </summary>
public static class IEdmModelExtensions
{
    /// <param name="model">The extended <see cref="IEdmModel">EDM</see>.</param>
    extension( IEdmModel model )
    {
        /// <summary>
        /// Gets the API version associated with the Entity Data Model (EDM).
        /// </summary>
        /// <returns>The associated <see cref="ApiVersion">API version</see> or <c>null</c>.</returns>
        public ApiVersion? ApiVersion => model.GetAnnotationValue<ApiVersionAnnotation>( model )?.ApiVersion;

        /// <summary>
        /// Gets a value indicating whether the Entity Data Model (EDM) is for defined ad hoc usage.
        /// </summary>
        /// <returns>True if the EDM is defined for ad hoc usage; otherwise, false.</returns>
        public bool IsAdHoc => model.GetAnnotationValue<AdHocAnnotation>( model ) is not null;
    }
}