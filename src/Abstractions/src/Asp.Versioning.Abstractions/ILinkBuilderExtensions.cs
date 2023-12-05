// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

/// <summary>
/// Provides extension methods for the <see cref="ILinkBuilder"/> interface.
/// </summary>
public static class ILinkBuilderExtensions
{
    /// <summary>
    /// Creates and returns a new link builder.
    /// </summary>
    /// <param name="builder">The extended <see cref="ILinkBuilder">link builder</see>.</param>
    /// <param name="linkTarget">The link target URL.</param>
    /// <returns>A new <see cref="ILinkBuilder">link builder</see>.</returns>
    public static ILinkBuilder Link( this ILinkBuilder builder, string linkTarget )
    {
        ArgumentNullException.ThrowIfNull( builder );
        return builder.Link( new Uri( linkTarget, UriKind.RelativeOrAbsolute ) );
    }
}