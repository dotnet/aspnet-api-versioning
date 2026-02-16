// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

/// <summary>
/// Provides extension methods for the <see cref="IPolicyBuilder{T}"/> interface.
/// </summary>
public static class IPolicyBuilderExtensions
{
    /// <param name="builder">The extended <see cref="IPolicyBuilder{T}">policy builder</see>.</param>
    extension( IPolicyWithLink builder )
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

    /// <typeparam name="TBuilder">The type of <see cref="IPolicyBuilder{T}">policy builder</see>.</typeparam>
    /// <param name="builder">The extended <see cref="IPolicyBuilder{T}">policy builder</see>.</param>
    extension<TBuilder>( TBuilder builder ) where TBuilder : notnull, IPolicyWithEffectiveDate
    {
        /// <summary>
        /// Indicates when a policy is applied.
        /// </summary>
        /// <param name="effectiveDate">The time when the policy is applied.</param>
        /// <returns>The current policy builder.</returns>
        public TBuilder Effective( DateTimeOffset effectiveDate )
        {
            ArgumentNullException.ThrowIfNull( builder );
            builder.SetEffectiveDate( effectiveDate );
            return builder;
        }

        /// <summary>
        /// Indicates when a policy is applied.
        /// </summary>
        /// <param name="year">The year when the policy is applied.</param>
        /// <param name="month">The month when the policy is applied.</param>
        /// <param name="day">The day when the policy is applied.</param>
        /// <returns>The current policy builder.</returns>
        public TBuilder Effective( int year, int month, int day )
        {
            ArgumentNullException.ThrowIfNull( builder );
            return builder.Effective( new DateTimeOffset( new DateTime( year, month, day ) ) );
        }
    }
}