// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

/// <summary>
/// Defines the behavior of a sunset policy builder.
/// </summary>
public interface ISunsetPolicyBuilder : IPolicyBuilder<SunsetPolicy>
{
    /// <summary>
    /// Creates and returns a new link builder.
    /// </summary>
    /// <param name="linkTarget">The link target URL.</param>
    /// <returns>A new <see cref="ILinkBuilder">link builder</see>.</returns>
    ILinkBuilder Link( Uri linkTarget );

    /// <summary>
    /// Indicates when a sunset policy is applied.
    /// </summary>
    /// <param name="sunsetDate">The <see cref="DateTimeOffset">date and time</see> when a
    /// sunset policy is applied.</param>
    /// <returns>The current <see cref="ISunsetPolicyBuilder">sunset policy builder</see>.</returns>
    ISunsetPolicyBuilder Effective( DateTimeOffset sunsetDate );
}