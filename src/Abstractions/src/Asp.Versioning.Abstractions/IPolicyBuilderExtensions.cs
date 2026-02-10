// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

/// <summary>
/// Provides extension methods for the <see cref="IPolicyBuilder{T}"/> interface.
/// </summary>
public static class IPolicyBuilderExtensions
{
    /// <summary>
    /// Creates and returns a new link builder.
    /// </summary>
    /// <param name="builder">The extended <see cref="IPolicyBuilder{T}">policy builder</see>.</param>
    /// <param name="linkTarget">The link target URL.</param>
    /// <returns>A new <see cref="ILinkBuilder">link builder</see>.</returns>
    public static ILinkBuilder Link( this IPolicyWithLink builder, string linkTarget )
    {
        ArgumentNullException.ThrowIfNull( builder );
        return builder.Link( new Uri( linkTarget, UriKind.RelativeOrAbsolute ) );
    }

    /// <summary>
    /// Indicates when a policy is applied.
    /// </summary>
    /// <typeparam name="TBuilder">The type of <see cref="IPolicyBuilder{T}">policy builder</see>.</typeparam>
    /// <param name="builder">The extended <see cref="IPolicyBuilder{T}">policy builder</see>.</param>
    /// <param name="effectiveDate">The time when the policy is applied.</param>
    /// <returns>The current <see cref="IPolicyBuilder{T}">policy builder</see>.</returns>
    public static TBuilder Effective<TBuilder>( this TBuilder builder, DateTimeOffset effectiveDate )
        where TBuilder : notnull, IPolicyWithEffectiveDate
    {
        ArgumentNullException.ThrowIfNull( builder );
        builder.SetEffectiveDate( effectiveDate );
        return builder;
    }

    /// <summary>
    /// Indicates when a policy is applied.
    /// </summary>
    /// <typeparam name="TBuilder">The type of <see cref="IPolicyBuilder{T}">policy builder</see>.</typeparam>
    /// <param name="builder">The extended <see cref="IPolicyBuilder{T}">policy builder</see>.</param>
    /// <param name="year">The year when the policy is applied.</param>
    /// <param name="month">The month when the policy is applied.</param>
    /// <param name="day">The day when the policy is applied.</param>
    /// <returns>The current <see cref="IPolicyBuilder{T}">policy builder</see>.</returns>
    public static TBuilder Effective<TBuilder>( this TBuilder builder, int year, int month, int day )
        where TBuilder : notnull, IPolicyWithEffectiveDate
    {
        ArgumentNullException.ThrowIfNull( builder );
        return builder.Effective( new DateTimeOffset( new DateTime( year, month, day ) ) );
    }
}