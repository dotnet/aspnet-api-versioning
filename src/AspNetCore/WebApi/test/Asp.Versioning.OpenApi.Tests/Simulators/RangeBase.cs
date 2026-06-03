// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OpenApi.Simulators;

/// <summary>
/// Represents the base implementation of a range of values.
/// </summary>
/// <typeparam name="T">The type of value in the range.</typeparam>
public abstract class RangeBase<T> : IRange<T> where T : struct
{
    /// <inheritdoc />
    public T? Lower { get; set; }

    /// <inheritdoc />
    public T? Upper { get; set; }
}