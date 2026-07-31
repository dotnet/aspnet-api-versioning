// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using Google.Protobuf.Reflection;
using Grpc.Core;

/// <summary>
/// Validates each message read from a client stream against the requested API version.
/// </summary>
/// <typeparam name="T">The type of message read.</typeparam>
internal sealed class ValidatingStreamReader<T>(
    IAsyncStreamReader<T> stream,
    IAnnotation<FieldDescriptor, ApiVersionRange> annotations,
    ApiVersion apiVersion ) : IAsyncStreamReader<T>
    where T : class
{
    public T Current => stream.Current;

    public async Task<bool> MoveNext( CancellationToken cancellationToken )
    {
        if ( !await stream.MoveNext( cancellationToken ).ConfigureAwait( false ) )
        {
            return false;
        }

        MessageFields.Validate( annotations, stream.Current, apiVersion );

        return true;
    }
}