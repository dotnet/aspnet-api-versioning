// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

/// <inheritdoc/>
public abstract class PolicyManager<TPolicy, TPolicyBuilder> : IPolicyManager<TPolicy>
    where TPolicyBuilder : IPolicyBuilder<TPolicy>
{
    private Dictionary<PolicyKey, TPolicy>? policies;

    /// <summary>
    /// Gets the current api versioning options.
    /// </summary>
    /// <value>The api versioning options.</value>
    protected abstract ApiVersioningOptions Options { get; }

    /// <inheritdoc />
    public virtual bool TryGetPolicy(
        string? name,
        ApiVersion? apiVersion,
        [MaybeNullWhen( false )] out TPolicy policy )
    {
        if ( string.IsNullOrEmpty( name ) && apiVersion == null )
        {
            policy = default!;
            return false;
        }

        policies ??= BuildPolicies( Options );

        var key = new PolicyKey( name, apiVersion );

        return policies.TryGetValue( key, out policy );
    }

    private static Dictionary<PolicyKey, TPolicy> BuildPolicies( ApiVersioningOptions options )
    {
        var builders = options.Policies.OfType<TPolicyBuilder>();
        var mapping = new Dictionary<PolicyKey, TPolicy>( capacity: builders.Count );

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