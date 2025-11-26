// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

/// <summary>
/// Defines the behavior of a deprecation policy builder.
/// </summary>
public interface IDeprecationPolicyBuilder : IPolicyBuilder<DeprecationPolicy>
{
    /// <summary>
    /// Creates and returns a new link builder.
    /// </summary>
    /// <param name="linkTarget">The link target URL.</param>
    /// <returns>A new <see cref="ILinkBuilder">link builder</see>.</returns>
    ILinkBuilder Link( Uri linkTarget );

    /// <summary>
    /// Indicates when a deprecation policy is applied.
    /// </summary>
    /// <param name="deprecationDate">The <see cref="DateTimeOffset">date and time</see> when a
    /// deprecation policy is applied.</param>
    /// <returns>The current <see cref="IDeprecationPolicyBuilder">deprecation policy builder</see>.</returns>
    IDeprecationPolicyBuilder Effective( DateTimeOffset deprecationDate );
}