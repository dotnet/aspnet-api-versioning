// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

/// <summary>
/// Defines the behavior of an API versioning policy builder.
/// </summary>
public interface IApiVersioningPolicyBuilder
{
    /// <summary>
    /// Gets a read-only list of policies for the specified type.
    /// </summary>
    /// <typeparam name="T">The type of policy to get.</typeparam>
    /// <returns>A <see cref="IReadOnlyList{T}">read-only list</see> of policies.</returns>
    IReadOnlyList<T> OfType<T>() where T : notnull;

    /// <summary>
    /// Creates and returns a new sunset policy builder.
    /// </summary>
    /// <param name="name">The optional name of the API the policy is for.</param>
    /// <param name="apiVersion">The optional <see cref="ApiVersion">API version</see> the policy is for.</param>
    /// <returns>A new <see cref="ISunsetPolicyBuilder">sunset policy builder</see>.</returns>
    /// <exception cref="ArgumentException">The <paramref name="name"/> and <paramref name="apiVersion"/>
    /// parameters are both <c>null</c>.</exception>
    ISunsetPolicyBuilder Sunset( string? name, ApiVersion? apiVersion );
}