// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

/// <summary>
/// Defines the behavior of a link builder.
/// </summary>
public interface ILinkBuilder
{
    /// <summary>
    /// Creates and returns a new link builder.
    /// </summary>
    /// <param name="linkTarget">The link target URL.</param>
    /// <returns>A new <see cref="ILinkBuilder">link builder</see>.</returns>
    ILinkBuilder Link( Uri linkTarget );

    /// <summary>
    /// Applies a type to the link.
    /// </summary>
    /// <param name="value">A hint indicating what the media type of the result of dereferencing the link should be.</param>
    /// <returns>The current <see cref="ILinkBuilder">link builder</see>.</returns>
    ILinkBuilder Type( string value );

    /// <summary>
    /// Applies a title to the link.
    /// </summary>
    /// <param name="value">The value used to label the destination of the link such that it can be used as a human-readable
    /// identifier (e.g. "menu entry") in the language indicated by the Content-Language header field, if present.</param>
    /// <returns>The current <see cref="ILinkBuilder">link builder</see>.</returns>
    ILinkBuilder Title( string value );

    /// <summary>
    /// Applies a language to the link.
    /// </summary>
    /// <param name="value">A hint indicating what the language of the result of dereferencing the link should be.</param>
    /// <returns>The current <see cref="ILinkBuilder">link builder</see>.</returns>
    ILinkBuilder Language( string value );

    /// <summary>
    /// Creates and returns a new link.
    /// </summary>
    /// <returns>A new <see cref="LinkHeaderValue">link header value</see>.</returns>
    LinkHeaderValue Build();
}