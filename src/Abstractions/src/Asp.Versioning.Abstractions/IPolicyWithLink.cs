// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

/// <summary>
/// Defines a policy which can (optionally) expose a link to more information.
/// </summary>
public interface IPolicyWithLink
{
    /// <summary>
    /// Creates and returns a new link builder.
    /// </summary>
    /// <param name="linkTarget">The link target URL.</param>
    /// <returns>A new <see cref="ILinkBuilder">link builder</see>.</returns>
    ILinkBuilder Link( Uri linkTarget );
}