// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Microsoft.OData.Edm;

using Asp.Versioning;
using Asp.Versioning.OData;

/// <summary>
/// Provides extension methods for <see cref="IEdmModel"/>.
/// </summary>
public static class IEdmModelExtensions
{
    /// <summary>
    /// Gets the API version associated with the Entity Data Model (EDM).
    /// </summary>
    /// <param name="model">The extended <see cref="IEdmModel">EDM</see>.</param>
    /// <returns>The associated <see cref="ApiVersion">API version</see> or <c>null</c>.</returns>
    public static ApiVersion? GetApiVersion( this IEdmModel model ) =>
        model.GetAnnotationValue<ApiVersionAnnotation>( model )?.ApiVersion;

    /// <summary>
    /// Gets a value indicating whether the Entity Data Model (EDM) is for defined ad hoc usage.
    /// </summary>
    /// <param name="model">The extended <see cref="IEdmModel">EDM</see>.</param>
    /// <returns>True if the EDM is defined for ad hoc usage; otherwise, false.</returns>
    public static bool IsAdHoc( this IEdmModel model ) =>
        model.GetAnnotationValue<AdHocAnnotation>( model ) is not null;
}