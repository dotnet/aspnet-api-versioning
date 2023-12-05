// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

/// <summary>
/// Provides extension methods for the <see cref="ISunsetPolicyBuilder"/> interface.
/// </summary>
public static class ISunsetPolicyBuilderExtensions
{
    /// <summary>
    /// Creates and returns a new link builder.
    /// </summary>
    /// <param name="builder">The extended <see cref="ISunsetPolicyBuilder">sunset policy builder</see>.</param>
    /// <param name="linkTarget">The link target URL.</param>
    /// <returns>A new <see cref="ILinkBuilder">link builder</see>.</returns>
    public static ILinkBuilder Link( this ISunsetPolicyBuilder builder, string linkTarget )
    {
        ArgumentNullException.ThrowIfNull( builder );
        return builder.Link( new Uri( linkTarget, UriKind.RelativeOrAbsolute ) );
    }

    /// <summary>
    /// Indicates when a sunset policy is applied.
    /// </summary>
    /// <typeparam name="TBuilder">The type of <see cref="ISunsetPolicyBuilder">sunset policy builder</see>.</typeparam>
    /// <param name="builder">The extended <see cref="ISunsetPolicyBuilder">sunset policy builder</see>.</param>
    /// <param name="year">The year when the sunset policy is applied.</param>
    /// <param name="month">The month when the sunset policy is applied.</param>
    /// <param name="day">The day when the sunset policy is applied.</param>
    /// <returns>The current <see cref="ISunsetPolicyBuilder">sunset policy builder</see>.</returns>
    public static TBuilder Effective<TBuilder>( this TBuilder builder, int year, int month, int day )
        where TBuilder : notnull, ISunsetPolicyBuilder
    {
        ArgumentNullException.ThrowIfNull( builder );
        builder.Effective( new DateTimeOffset( new DateTime( year, month, day ) ) );
        return builder;
    }
}