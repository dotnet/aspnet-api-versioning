// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable CA1812

namespace Asp.Versioning;

using Google.Protobuf.Reflection;
using System.Collections.Concurrent;

internal sealed class ApiVersionMetadataCache( IApiVersionParser parser ) : IMemberFilter<FieldDescriptor>
{
    private readonly ConcurrentDictionary<FieldDescriptor, ApiVersionRange> cache = new();

    public ApiVersionRange Get( FieldDescriptor field ) => cache.GetOrAdd( field, Add );

    public bool IsVisible( FieldDescriptor member, ApiVersion apiVersion ) => Get( member ).Contains( apiVersion );

    private ApiVersionRange Add( FieldDescriptor field )
    {
        if ( field.GetOptions()?.GetExtension( AnnotationsExtensions.Version ) is not { Count: > 0 } versions )
        {
            return ApiVersionRange.Any;
        }

        return ApiVersionRange.Parse( parser, versions );
    }
}