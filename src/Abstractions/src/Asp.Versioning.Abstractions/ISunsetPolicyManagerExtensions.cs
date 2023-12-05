// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

/// <summary>
/// Provides extension methods for the <see cref="ISunsetPolicyManager"/> interface.
/// </summary>
public static class ISunsetPolicyManagerExtensions
{
    /// <summary>
    /// Returns the sunset policy for the specified API and version.
    /// </summary>
    /// <param name="policyManager">The extended <see cref="ISunsetPolicyManager">sunset policy manager</see>.</param>
    /// <param name="apiVersion">The API version to get the policy for.</param>
    /// <param name="sunsetPolicy">The applicable <see cref="SunsetPolicy">sunset policy</see>, if any.</param>
    /// <returns>True if the <paramref name="sunsetPolicy">sunset policy</paramref> was retrieved; otherwise, false.</returns>
    public static bool TryGetPolicy(
        this ISunsetPolicyManager policyManager,
        ApiVersion apiVersion,
        [MaybeNullWhen( false )] out SunsetPolicy sunsetPolicy )
    {
        ArgumentNullException.ThrowIfNull( policyManager );
        return policyManager.TryGetPolicy( default, apiVersion, out sunsetPolicy );
    }

    /// <summary>
    /// Returns the sunset policy for the specified API and version.
    /// </summary>
    /// <param name="policyManager">The extended <see cref="ISunsetPolicyManager">sunset policy manager</see>.</param>
    /// <param name="name">The name of the API.</param>
    /// <param name="sunsetPolicy">The applicable <see cref="SunsetPolicy">sunset policy</see>, if any.</param>
    /// <returns>True if the <paramref name="sunsetPolicy">sunset policy</paramref> was retrieved; otherwise, false.</returns>
    public static bool TryGetPolicy(
        this ISunsetPolicyManager policyManager,
        string name,
        [MaybeNullWhen( false )] out SunsetPolicy sunsetPolicy )
    {
        ArgumentNullException.ThrowIfNull( policyManager );
        return policyManager.TryGetPolicy( name, default, out sunsetPolicy );
    }

    /// <summary>
    /// Attempts to resolve a sunset policy for the specified name and API version combination.
    /// </summary>
    /// <param name="policyManager">The extended <see cref="ISunsetPolicyManager">sunset policy manager</see>.</param>
    /// <param name="name">The name of the API.</param>
    /// <param name="apiVersion">The API version to get the policy for.</param>
    /// <returns>The applicable <see cref="SunsetPolicy">sunset policy</see>, if any.</returns>
    /// <remarks>The resolution or is as follows:
    /// <list type="bullet">
    ///  <item><paramref name="name"/> and <paramref name="apiVersion"/></item>
    ///  <item><paramref name="name"/> only</item>
    ///  <item><paramref name="apiVersion"/> only</item>
    /// </list>
    /// </remarks>
    public static SunsetPolicy? ResolvePolicyOrDefault(
        this ISunsetPolicyManager policyManager,
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
    /// Attempts to resolve a sunset policy for the specified name and API version combination.
    /// </summary>
    /// <param name="policyManager">The extended <see cref="ISunsetPolicyManager">sunset policy manager</see>.</param>
    /// <param name="name">The name of the API.</param>
    /// <param name="apiVersion">The API version to get the policy for.</param>
    /// /// <param name="sunsetPolicy">The applicable <see cref="SunsetPolicy">sunset policy</see>, if any.</param>
    /// <returns>True if the <paramref name="sunsetPolicy">sunset policy</paramref> was retrieved; otherwise, false.</returns>
    /// <remarks>The resolution or is as follows:
    /// <list type="bullet">
    ///  <item><paramref name="name"/> and <paramref name="apiVersion"/></item>
    ///  <item><paramref name="name"/> only</item>
    ///  <item><paramref name="apiVersion"/> only</item>
    /// </list>
    /// </remarks>
    public static bool TryResolvePolicy(
        this ISunsetPolicyManager policyManager,
        string? name,
        ApiVersion? apiVersion,
        [MaybeNullWhen( false )] out SunsetPolicy sunsetPolicy )
    {
        ArgumentNullException.ThrowIfNull( policyManager );

        if ( !string.IsNullOrEmpty( name ) )
        {
            if ( apiVersion != null && policyManager.TryGetPolicy( name, apiVersion, out sunsetPolicy ) )
            {
                return true;
            }
            else if ( policyManager.TryGetPolicy( name!, out sunsetPolicy ) )
            {
                return true;
            }
        }
        else if ( apiVersion != null && policyManager.TryGetPolicy( apiVersion, out sunsetPolicy ) )
        {
            return true;
        }

        sunsetPolicy = default!;
        return false;
    }
}