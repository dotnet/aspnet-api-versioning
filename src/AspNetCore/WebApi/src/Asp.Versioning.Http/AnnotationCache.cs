// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using System.Collections.Concurrent;
using System.Reflection;

/// <summary>
/// Represents a cache of member-specified annotations; for example, <see cref="VisibleInApiVersionAttribute"/>.
/// </summary>
internal sealed class AnnotationCache : IAnnotation<MemberInfo, ApiVersionRange>
{
    private readonly ConcurrentDictionary<MemberInfo, ApiVersionRange?> cache = new();

    public bool TryGet( MemberInfo source, [MaybeNullWhen( false )] out ApiVersionRange annotation )
    {
        annotation = cache.GetOrAdd( source, Resolve );
        return annotation is not null;
    }

    private static ApiVersionRange? Resolve( MemberInfo member ) =>
        member.GetCustomAttribute<VisibleInApiVersionAttribute>( inherit: true )?.Range;
}