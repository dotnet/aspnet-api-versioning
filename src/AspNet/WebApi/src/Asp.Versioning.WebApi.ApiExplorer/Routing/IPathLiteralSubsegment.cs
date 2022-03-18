// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Routing;

/// <summary>
/// Defines the behavior of a a literal segment.
/// </summary>
public interface IPathLiteralSubsegment : IPathSubsegment
{
    /// <summary>
    /// Gets the literal subsegment value.
    /// </summary>
    /// <value>The literal subsegment value.</value>
    string Literal { get; }
}