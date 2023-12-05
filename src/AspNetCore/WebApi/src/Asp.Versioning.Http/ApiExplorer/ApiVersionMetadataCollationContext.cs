// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.ApiExplorer;

/// <summary>
/// Represents the context used during API version metadata collation.
/// </summary>
public class ApiVersionMetadataCollationContext
{
    /// <summary>
    /// Gets the read-only list of collation results.
    /// </summary>
    /// <value>The <see cref="IReadOnlyList{T}">read-only list</see> of collation results.</value>
    public ApiVersionMetadataCollationCollection Results { get; } = [];
}