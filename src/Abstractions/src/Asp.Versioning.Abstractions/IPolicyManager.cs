// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

/// <summary>
/// Defines the behavior of an API version policy manager.
/// </summary>
/// <typeparam name="TPolicy">The type of the policy.</typeparam>
public interface IPolicyManager<TPolicy>
{
    /// <summary>
    /// Returns the policy for the specified API and version.
    /// </summary>
    /// <param name="name">The name of the API.</param>
    /// <param name="apiVersion">The API version to get the policy for.</param>
    /// <param name="policy">The applicable <typeparamref name="TPolicy">policy</typeparamref>, if any.</param>
    /// <returns>True if the <paramref name="policy">policy</paramref> was retrieved; otherwise, false.</returns>
    /// <remarks>If <paramref name="name"/> is <c>null</c>, it is assumed the caller intends to match any
    /// policy for the specified <paramref name="apiVersion">API version</paramref>. If
    /// <paramref name="apiVersion">API version</paramref> is <c>null</c>, it is assumed the caller intends to match
    /// any policy for the specified <paramref name="name"/>.</remarks>
    bool TryGetPolicy( string? name, ApiVersion? apiVersion, [MaybeNullWhen( false )] out TPolicy policy );
}