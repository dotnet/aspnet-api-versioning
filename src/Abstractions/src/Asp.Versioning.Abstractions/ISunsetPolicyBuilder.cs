// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

/// <summary>
/// Defines the behavior of a sunset policy builder.
/// </summary>
public interface ISunsetPolicyBuilder
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
    /// Applies a sunset policy per the specified policy.
    /// </summary>
    /// <param name="policy">The applied <see cref="SunsetPolicy">sunset policy</see>.</param>
    void Per( SunsetPolicy policy );

    /// <summary>
    /// Indicates when a sunset policy is applied.
    /// </summary>
    /// <param name="sunsetDate">The <see cref="DateTimeOffset">date and time</see> when a
    /// sunset policy is applied.</param>
    /// <returns>The current <see cref="ISunsetPolicyBuilder">sunset policy builder</see>.</returns>
    ISunsetPolicyBuilder Effective( DateTimeOffset sunsetDate );

    /// <summary>
    /// Creates and returns a new link builder.
    /// </summary>
    /// <param name="linkTarget">The link target URL.</param>
    /// <returns>A new <see cref="ILinkBuilder">link builder</see>.</returns>
    ILinkBuilder Link( Uri linkTarget );

    /// <summary>
    /// Builds and returns a sunset policy.
    /// </summary>
    /// <returns>A new <see cref="SunsetPolicy">sunset policy</see>.</returns>
    SunsetPolicy Build();
}