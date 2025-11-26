// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

/// <summary>
/// Defines the behavior of a policy builder which applies to a single API version.
/// </summary>
/// <typeparam name="TPolicy">The type of policy which is built by this builder.</typeparam>
public interface IPolicyBuilder<TPolicy>
{
    /// <summary>
    /// Gets the policy name.
    /// </summary>
    /// <value>The policy name, if any.</value>
    /// <remarks>The name is typically of an API.</remarks>
    string? Name { get; }

    /// <summary>
    /// Gets the API version the policy is for.
    /// </summary>
    /// <value>The specific policy <see cref="ApiVersion">API version</see>, if any.</value>
    ApiVersion? ApiVersion { get; }

    /// <summary>
    /// Configures the builder per the specified <paramref name="policy"/>.
    /// </summary>
    /// <param name="policy">The applied <typeparamref name="TPolicy">policy</typeparamref>.</param>
    void Per( TPolicy policy );

    /// <summary>
    /// Builds and returns a policy.
    /// </summary>
    /// <returns>A new <typeparamref name="TPolicy">policy</typeparamref>.</returns>
    TPolicy Build();
}