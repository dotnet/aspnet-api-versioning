// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

/// <summary>
/// Provides extension methods for the <see cref="ILinkBuilder"/> interface.
/// </summary>
public static class ILinkBuilderExtensions
{
    /// <param name="builder">The extended <see cref="ILinkBuilder">link builder</see>.</param>
    extension( ILinkBuilder builder )
    {
        /// <summary>
        /// Creates and returns a new link builder.
        /// </summary>
        /// <param name="linkTarget">The link target URL.</param>
        /// <returns>A new <see cref="ILinkBuilder">link builder</see>.</returns>
        public ILinkBuilder Link( string linkTarget )
        {
            ArgumentNullException.ThrowIfNull( builder );
            return builder.Link( new Uri( linkTarget, UriKind.RelativeOrAbsolute ) );
        }
    }
}