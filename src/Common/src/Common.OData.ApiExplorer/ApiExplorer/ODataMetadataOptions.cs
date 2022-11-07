// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.ApiExplorer;

/// <summary>
/// Represents the possible OData metadata options used during API exploration.
/// </summary>
[Flags]
public enum ODataMetadataOptions
{
    /// <summary>
    /// Indicates no OData metadata options.
    /// </summary>
    None = 0,

    /// <summary>
    /// Indicates the OData service document will be included.
    /// </summary>
    ServiceDocument = 1,

    /// <summary>
    /// Indicates the OData metadata document will be included.
    /// </summary>
    Metadata = 2,

    /// <summary>
    /// Indicates all OData metadata options.
    /// </summary>
    All = ServiceDocument | Metadata,
}