// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

/// <summary>
/// Provides extension methods for <see cref="MediaTypeApiVersionReaderBuilder"/>.
/// </summary>
public static class MediaTypeApiVersionReaderBuilderExtensions
{
    /// <typeparam name="T">The type of builder.</typeparam>
    /// <exception cref="ArgumentNullException"><paramref name="builder"/> is <c>null</c>.</exception>
    extension<T>( T builder ) where T : MediaTypeApiVersionReaderBuilder
    {
        /// <summary>
        /// Selects the first available API version, if there is one.
        /// </summary>
        /// <returns>The current <typeparamref name="T">builder</typeparamref>.</returns>
        /// <remarks>This will likely select the lowest API version.</remarks>
        public T SelectFirstOrDefault()
        {
            ArgumentNullException.ThrowIfNull( builder );
            builder.Select( static ( request, versions ) => versions.Count == 0 ? versions : [versions[0]] );
            return builder;
        }

        /// <summary>
        /// Selects the last available API version, if there is one.
        /// </summary>
        /// <returns>The current <typeparamref name="T">builder</typeparamref>.</returns>
        /// <remarks>This will likely select the highest API version.</remarks>
        public T SelectLastOrDefault()
        {
            ArgumentNullException.ThrowIfNull( builder );
            builder.Select( static ( request, versions ) => versions.Count == 0 ? versions : [versions[versions.Count - 1]] );
            return builder;
        }
    }
}