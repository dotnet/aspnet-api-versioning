// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using System.Globalization;

/// <summary>
/// Represents the default policy builder.
/// </summary>
/// <typeparam name="TPolicy">The type of policy.</typeparam>
public abstract class PolicyBuilder<TPolicy> : IPolicyBuilder<TPolicy>
{
    /// <summary>
    /// Gets a pre-built policy.
    /// </summary>
    /// <value>The pre-built policy, if it exists.</value>
    protected TPolicy? Policy { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PolicyBuilder{T}"/> class.
    /// </summary>
    /// <param name="name">The name of the API the policy is for.</param>
    /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> the policy is for.</param>
    public PolicyBuilder( string? name, ApiVersion? apiVersion )
    {
        if ( string.IsNullOrEmpty( name ) && apiVersion == null )
        {
            var message = string.Format( CultureInfo.CurrentCulture, Format.InvalidPolicyKey, nameof( name ), nameof( apiVersion ) );
            throw new System.ArgumentException( message );
        }

        Name = name;
        ApiVersion = apiVersion;
    }

    /// <inheritdoc />
    public string? Name { get; }

    /// <inheritdoc />
    public ApiVersion? ApiVersion { get; }

    /// <inheritdoc />
    public virtual void Per( TPolicy policy ) =>
        Policy = policy ?? throw new System.ArgumentNullException( nameof( policy ) );

    /// <inheritdoc />
    public abstract TPolicy Build();
}