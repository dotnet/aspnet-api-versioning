// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Routing;

/// <summary>
/// Defines the behavior of a parameter subsegment.
/// </summary>
public interface IPathParameterSubsegment : IPathSubsegment
{
    /// <summary>
    /// Gets a value indicating whether the segment represents a "catch all".
    /// </summary>
    /// <value>True if the segment represents a "catch all" (*); otherwise, false.</value>
    bool IsCatchAll { get; }

    /// <summary>
    /// Gets the corresponding parameter name.
    /// </summary>
    /// <value>The corresponding segment parameter name.</value>
    string ParameterName { get; }
}