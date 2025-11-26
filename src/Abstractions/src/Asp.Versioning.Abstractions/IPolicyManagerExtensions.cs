// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

/// <summary>
/// Provides extension methods for the <see cref="IPolicyManager{T}"/> interface.
/// </summary>
public static class IPolicyManagerExtensions
{
    /// <summary>
    /// Returns the policy for the specified API and version.
    /// </summary>
    /// <param name="policyManager">The extended <see cref="IPolicyManager{T}">policy manager</see>.</param>
    /// <param name="apiVersion">The API version to get the policy for.</param>
    /// <param name="policy">The applicable <typeparamref name="TPolicy">policy</typeparamref>, if any.</param>
    /// <typeparam name="TPolicy">The type of policy.</typeparam>
    /// <returns>True if the <paramref name="policy">policy</paramref> was retrieved; otherwise, false.</returns>
    public static bool TryGetPolicy<TPolicy>(
        this IPolicyManager<TPolicy> policyManager,
        ApiVersion apiVersion,
        [MaybeNullWhen( false )] out TPolicy policy )
    {
        ArgumentNullException.ThrowIfNull( policyManager );
        return policyManager.TryGetPolicy( default, apiVersion, out policy );
    }

    /// <summary>
    /// Returns the policy for the specified API and version.
    /// </summary>
    /// <param name="policyManager">The extended <see cref="IPolicyManager{T}">policy manager</see>.</param>
    /// <param name="name">The name of the API.</param>
    /// <param name="policy">The applicable <typeparamref name="TPolicy">policy</typeparamref>, if any.</param>
    /// <typeparam name="TPolicy">The type of policy.</typeparam>
    /// <returns>True if the <paramref name="policy">policy</paramref> was retrieved; otherwise, false.</returns>
    public static bool TryGetPolicy<TPolicy>(
        this IPolicyManager<TPolicy> policyManager,
        string name,
        [MaybeNullWhen( false )] out TPolicy policy )
    {
        ArgumentNullException.ThrowIfNull( policyManager );
        return policyManager.TryGetPolicy( name, default, out policy );
    }

    /// <summary>
    /// Attempts to resolve a policy for the specified name and API version combination.
    /// </summary>
    /// <param name="policyManager">The extended <see cref="IPolicyManager{T}">policy manager</see>.</param>
    /// <param name="name">The name of the API.</param>
    /// <param name="apiVersion">The API version to get the policy for.</param>
    /// <typeparam name="TPolicy">The type of policy.</typeparam>
    /// <returns>The applicable <typeparamref name="TPolicy">policy</typeparamref>, if any.</returns>
    /// <remarks>The resolution order is as follows:
    /// <list type="bullet">
    ///  <item><paramref name="name"/> and <paramref name="apiVersion"/></item>
    ///  <item><paramref name="name"/> only</item>
    ///  <item><paramref name="apiVersion"/> only</item>
    /// </list>
    /// </remarks>
    public static TPolicy? ResolvePolicyOrDefault<TPolicy>(
        this IPolicyManager<TPolicy> policyManager,
        string? name,
        ApiVersion? apiVersion )
    {
        ArgumentNullException.ThrowIfNull( policyManager );

        if ( policyManager.TryResolvePolicy( name, apiVersion, out var policy ) )
        {
            return policy;
        }

        return default;
    }

    /// <summary>
    /// Attempts to resolve a policy for the specified name and API version combination.
    /// </summary>
    /// <param name="policyManager">The extended <see cref="IPolicyManager{T}">policy manager</see>.</param>
    /// <param name="name">The name of the API.</param>
    /// <param name="apiVersion">The API version to get the policy for.</param>
    /// <param name="policy">The applicable <typeparamref name="TPolicy">policy</typeparamref>, if any.</param>
    /// <typeparam name="TPolicy">The type of policy.</typeparam>
    /// <returns>True if the <paramref name="policy">policy</paramref> was retrieved; otherwise, false.</returns>
    /// <remarks>The resolution order is as follows:
    /// <list type="bullet">
    ///  <item><paramref name="name"/> and <paramref name="apiVersion"/></item>
    ///  <item><paramref name="name"/> only</item>
    ///  <item><paramref name="apiVersion"/> only</item>
    /// </list>
    /// </remarks>
    public static bool TryResolvePolicy<TPolicy>(
        this IPolicyManager<TPolicy> policyManager,
        string? name,
        ApiVersion? apiVersion,
        [MaybeNullWhen( false )] out TPolicy policy )
    {
        ArgumentNullException.ThrowIfNull( policyManager );

        if ( !string.IsNullOrEmpty( name ) )
        {
            if ( apiVersion != null && policyManager.TryGetPolicy( name, apiVersion, out policy ) )
            {
                return true;
            }
            else if ( policyManager.TryGetPolicy( name!, out policy ) )
            {
                return true;
            }
        }

        if ( apiVersion != null && policyManager.TryGetPolicy( apiVersion, out policy ) )
        {
            return true;
        }

        policy = default!;
        return false;
    }
}