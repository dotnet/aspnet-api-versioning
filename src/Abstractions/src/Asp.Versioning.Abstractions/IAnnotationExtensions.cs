// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

/// <summary>
/// Provides extension methods for the <see cref="IAnnotation{TSource, TAnnotation}"/> interface.
/// </summary>
public static class IAnnotationExtensions
{
    /// <typeparam name="TSource">The type of annotated source.</typeparam>
    /// <param name="annotations">The extended <see cref="IAnnotation{TSource, TAnnotation}">annotations</see>.</param>
    extension<TSource>( IAnnotation<TSource, ApiVersionRange> annotations )
    {
        /// <summary>
        /// Determines whether the provided source is visible to the specified API version.
        /// </summary>
        /// <param name="source">The source to evaluate.</param>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> to compare against.</param>
        /// <returns>True if the source is visible in the specified <paramref name="apiVersion">API version</paramref>;
        /// otherwise, false.</returns>
        /// <remarks>This is the filtering action applied from an annotation. A source that is not annotated is
        /// visible to every API version. A source that is evaluated repeatedly should resolve its annotation once
        /// and evaluate the range directly.</remarks>
        public bool IsVisible( TSource source, ApiVersion apiVersion )
        {
            ArgumentNullException.ThrowIfNull( annotations );
            return !annotations.TryGet( source, out var apiVersions ) || apiVersions.Contains( apiVersion );
        }
    }
}