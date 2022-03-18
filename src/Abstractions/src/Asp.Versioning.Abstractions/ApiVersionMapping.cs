// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

/// <summary>
/// Represents the possible types of API version mappings.
/// </summary>
public enum ApiVersionMapping
{
    /// <summary>
    /// Indicates no mapping.
    /// </summary>
    None,

    /// <summary>
    /// Indicates an explicit mapping.
    /// </summary>
    Explicit,

    /// <summary>
    /// Indicates an implicit mapping.
    /// </summary>
    Implicit,
}