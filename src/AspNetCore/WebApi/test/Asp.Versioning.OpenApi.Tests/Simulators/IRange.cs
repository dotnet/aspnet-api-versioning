// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OpenApi.Simulators;

/// <summary>
/// Represents a range of values.
/// </summary>
/// <typeparam name="T">The type of value in the range.</typeparam>
public interface IRange<T> where T : struct
{
    /// <summary>
    /// Gets or sets the lower bound of the range.
    /// </summary>
    T? Lower { get; set; }

    /// <summary>
    /// Gets or sets the upper bound of the range.
    /// </summary>
    T? Upper { get; set; }
}