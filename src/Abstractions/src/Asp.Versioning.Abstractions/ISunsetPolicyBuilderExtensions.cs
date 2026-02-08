// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

/// <summary>
/// Provides extension methods for the <see cref="ISunsetPolicyBuilder"/> interface.
/// </summary>
public static class ISunsetPolicyBuilderExtensions
{
    /// <param name="builder">The extended <see cref="ISunsetPolicyBuilder">sunset policy builder</see>.</param>
    extension( ISunsetPolicyBuilder builder )
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

    /// <typeparam name="TBuilder">The type of <see cref="ISunsetPolicyBuilder">sunset policy builder</see>.</typeparam>
    /// <returns>The current <paramref name="builder" />.</returns>
    extension<TBuilder>( TBuilder builder ) where TBuilder : notnull, ISunsetPolicyBuilder
    {
        /// <summary>
        /// Indicates when a sunset policy is applied.
        /// </summary>
        /// <param name="year">The year when the sunset policy is applied.</param>
        /// <param name="month">The month when the sunset policy is applied.</param>
        /// <param name="day">The day when the sunset policy is applied.</param>
        public TBuilder Effective( int year, int month, int day )
        {
            ArgumentNullException.ThrowIfNull( builder );
            builder.Effective( new DateTimeOffset( new DateTime( year, month, day ) ) );
            return builder;
        }
    }
}