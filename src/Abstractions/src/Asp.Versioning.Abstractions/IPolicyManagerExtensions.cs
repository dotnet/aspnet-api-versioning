// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

/// <summary>
/// Provides extension methods for the <see cref="IPolicyManager{T}"/> interface.
/// </summary>
public static class IPolicyManagerExtensions
{
    /// <typeparam name="T">The type of policy.</typeparam>
    /// <param name="policyManager">The extended <see cref="IPolicyManager{T}">policy manager</see>.</param>
    extension<T>( IPolicyManager<T> policyManager )
    {
        /// <summary>
        /// Returns the policy for the specified API and version.
        /// </summary>
        /// <param name="apiVersion">The API version to get the policy for.</param>
        /// <param name="policy">The applicable <typeparamref name="T">policy</typeparamref>, if any.</param>
        /// <returns>True if the <paramref name="policy">policy</paramref> was retrieved; otherwise, false.</returns>
        public bool TryGetPolicy( ApiVersion apiVersion, [MaybeNullWhen( false )] out T policy )
        {
            ArgumentNullException.ThrowIfNull( policyManager );
            return policyManager.TryGetPolicy( default, apiVersion, out policy );
        }

        /// <summary>
        /// Returns the policy for the specified API and version.
        /// </summary>
        /// <param name="name">The name of the API.</param>
        /// <param name="policy">The applicable <typeparamref name="T">policy</typeparamref>, if any.</param>
        /// <returns>True if the <paramref name="policy">policy</paramref> was retrieved; otherwise, false.</returns>
        public bool TryGetPolicy( string name, [MaybeNullWhen( false )] out T policy )
        {
            ArgumentNullException.ThrowIfNull( policyManager );
            return policyManager.TryGetPolicy( name, default, out policy );
        }

        /// <summary>
        /// Attempts to resolve a policy for the specified name and API version combination.
        /// </summary>
        /// <param name="name">The name of the API.</param>
        /// <param name="apiVersion">The API version to get the policy for.</param>
        /// <returns>The applicable <typeparamref name="T">policy</typeparamref>, if any.</returns>
        /// <remarks>The resolution order is as follows:
        /// <list type="bullet">
        ///  <item><paramref name="name"/> and <paramref name="apiVersion"/></item>
        ///  <item><paramref name="name"/> only</item>
        ///  <item><paramref name="apiVersion"/> only</item>
        /// </list>
        /// </remarks>
        public T? ResolvePolicyOrDefault( string? name, ApiVersion? apiVersion )
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
        /// <param name="name">The name of the API.</param>
        /// <param name="apiVersion">The API version to get the policy for.</param>
        /// <param name="policy">The applicable <typeparamref name="T">policy</typeparamref>, if any.</param>
        /// <returns>True if the <paramref name="policy">policy</paramref> was retrieved; otherwise, false.</returns>
        /// <remarks>The resolution order is as follows:
        /// <list type="bullet">
        ///  <item><paramref name="name"/> and <paramref name="apiVersion"/></item>
        ///  <item><paramref name="name"/> only</item>
        ///  <item><paramref name="apiVersion"/> only</item>
        /// </list>
        /// </remarks>
        public bool TryResolvePolicy( string? name, ApiVersion? apiVersion, [MaybeNullWhen( false )] out T policy )
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
}