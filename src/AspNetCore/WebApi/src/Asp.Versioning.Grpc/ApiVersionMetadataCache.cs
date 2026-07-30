// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Grpc;

using Google.Protobuf.Reflection;
using System.Collections.Concurrent;

internal sealed class ApiVersionMetadataCache( IApiVersionParser parser )
{
    private readonly ConcurrentDictionary<FieldDescriptor, ApiVersionRange> cache = new();

    public ApiVersionRange Get( FieldDescriptor field ) => cache.GetOrAdd( field, Add );

    public bool IsVisibleTo( FieldDescriptor field, ApiVersion apiVersion ) => Get( field ).Contains( apiVersion );

    private ApiVersionRange Add( FieldDescriptor field )
    {
        if ( field.GetOptions()?.GetExtension( AnnotationsExtensions.Version ) is not { Count: > 0 } versions )
        {
            return ApiVersionRange.Any;
        }

        return ApiVersionRange.Parse( parser, versions );
    }
}