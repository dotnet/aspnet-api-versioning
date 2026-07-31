// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable CA1812

namespace Asp.Versioning;

using Google.Protobuf.Reflection;
using System.Collections.Concurrent;

internal sealed class ApiVersionMetadataCache( IApiVersionParser parser ) : IAnnotation<FieldDescriptor, ApiVersionRange>
{
    // a field descriptor is a process-lifetime object, so the annotation resolved for it never changes. an
    // unannotated field is cached as null so that its options are only inspected once
    private readonly ConcurrentDictionary<FieldDescriptor, ApiVersionRange?> cache = new();

    public bool TryGet( FieldDescriptor source, [MaybeNullWhen( false )] out ApiVersionRange annotation )
    {
        annotation = cache.GetOrAdd( source, Resolve );
        return annotation is not null;
    }

    private ApiVersionRange? Resolve( FieldDescriptor field )
    {
        if ( field.GetOptions()?.GetExtension( AnnotationsExtensions.Version ) is not { Count: > 0 } versions )
        {
            return default;
        }

        return ApiVersionRange.Parse( parser, versions );
    }
}