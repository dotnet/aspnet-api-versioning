// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using System.Globalization;

/// <summary>
/// Represents the default API versioning policy builder.
/// </summary>
public class ApiVersioningPolicyBuilder : IApiVersioningPolicyBuilder
{
    private Dictionary<PolicyKey, ISunsetPolicyBuilder>? sunsetPolicies;

    /// <inheritdoc />
    public virtual IReadOnlyList<T> OfType<T>() where T : notnull
    {
        if ( typeof( T ) == typeof( ISunsetPolicyBuilder ) && sunsetPolicies != null )
        {
            return ( sunsetPolicies.Values.ToArray() as IReadOnlyList<T> )!;
        }

        return Array.Empty<T>();
    }

    /// <inheritdoc />
    public virtual ISunsetPolicyBuilder Sunset( string? name, ApiVersion? apiVersion )
    {
        if ( string.IsNullOrEmpty( name ) && apiVersion == null )
        {
            var message = string.Format( CultureInfo.CurrentCulture, Format.InvalidPolicyKey, nameof( name ), nameof( apiVersion ) );
            throw new System.ArgumentException( message );
        }

        var key = new PolicyKey( name, apiVersion );

        sunsetPolicies ??= [];

        if ( !sunsetPolicies.TryGetValue( key, out var builder ) )
        {
            sunsetPolicies.Add( key, builder = new SunsetPolicyBuilder( name, apiVersion ) );
        }

        return builder;
    }
}