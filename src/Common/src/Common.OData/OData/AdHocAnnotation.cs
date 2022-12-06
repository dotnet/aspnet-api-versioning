// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OData;

/// <summary>
/// Represents an annotation for ad hoc usage.
/// </summary>
public sealed class AdHocAnnotation
{
    /// <summary>
    /// Gets a singleton instance of the annotation.
    /// </summary>
    /// <value>A singleton <see cref="AdHocAnnotation">annotation</see> instance.</value>
    public static AdHocAnnotation Instance { get; } = new();
}