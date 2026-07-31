// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

/// <summary>
/// Defines the behavior of an annotation.
/// </summary>
/// <typeparam name="TSource">The type of annotated source.</typeparam>
/// <typeparam name="TAnnotation">The type of annotation.</typeparam>
/// <remarks>An annotation describes a source; it does not act on it. An action taken from an annotation, such as
/// deciding whether a data member is visible, is layered on top of the annotations reported here. A source can be
/// anything that is annotated, which includes, but is not limited to, a data member.</remarks>
public interface IAnnotation<in TSource, TAnnotation>
{
    /// <summary>
    /// Attempts to retrieve the annotation for the specified source.
    /// </summary>
    /// <param name="source">The source to retrieve the annotation for.</param>
    /// <param name="annotation">The retrieved <typeparamref name="TAnnotation">annotation</typeparamref>, if any.</param>
    /// <returns>True if the <paramref name="source"/> is annotated; otherwise, false.</returns>
    bool TryGet( TSource source, [MaybeNullWhen( false )] out TAnnotation annotation );
}