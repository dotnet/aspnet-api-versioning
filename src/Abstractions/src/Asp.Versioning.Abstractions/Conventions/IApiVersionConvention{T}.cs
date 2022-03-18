// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Conventions;

/// <summary>
/// Defines the behavior of an API version convention.
/// </summary>
/// <typeparam name="T">The <see cref="Type">type</see> of item to apply the convention to.</typeparam>
public interface IApiVersionConvention<in T> where T : notnull
{
    /// <summary>
    /// Applies the API version convention.
    /// </summary>
    /// <param name="item">The descriptor to apply the convention to.</param>
    void ApplyTo( T item );
}