// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

/// <summary>
/// Represents the default API version sunset policy manager.
/// </summary>
public partial class SunsetPolicyManager : ISunsetPolicyManager
{
    private Dictionary<PolicyKey, SunsetPolicy>? policies;

    /// <inheritdoc />
    public virtual bool TryGetPolicy(
        string? name,
        ApiVersion? apiVersion,
#if !NETFRAMEWORK && !NETCOREAPP3_1
        [MaybeNullWhen( false )]
#endif
        out SunsetPolicy sunsetPolicy )
    {
        if ( string.IsNullOrEmpty( name ) && apiVersion == null )
        {
            sunsetPolicy = default!;
            return false;
        }

#if NETFRAMEWORK
        policies ??= BuildPolicies( options );
#else
        policies ??= BuildPolicies( options.Value );
#endif
        var key = new PolicyKey( name, apiVersion );

        // NETCOREAPP3_1 only; remove when target is dropped
#pragma warning disable IDE0079
#pragma warning disable CS8601 // Possible null reference assignment
        return policies.TryGetValue( key, out sunsetPolicy );
#pragma warning restore CS8601
#pragma warning restore IDE0079
    }

    private static Dictionary<PolicyKey, SunsetPolicy> BuildPolicies( ApiVersioningOptions options )
    {
        var builders = options.Policies.OfType<ISunsetPolicyBuilder>();
        var mapping = new Dictionary<PolicyKey, SunsetPolicy>( capacity: builders.Count );

        for ( var i = 0; i < builders.Count; i++ )
        {
            var builder = builders[i];
            var policy = builder.Build();
            var key = new PolicyKey( builder.Name, builder.ApiVersion );

            mapping[key] = policy;
        }

        return mapping;
    }
}