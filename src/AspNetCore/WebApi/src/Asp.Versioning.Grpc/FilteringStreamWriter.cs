// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using Google.Protobuf.Reflection;
using Grpc.Core;

/// <summary>
/// Filters each message written to a server stream for the requested API version.
/// </summary>
/// <typeparam name="T">The type of message written.</typeparam>
internal sealed class FilteringStreamWriter<T>(
    IServerStreamWriter<T> stream,
    IAnnotation<FieldDescriptor, ApiVersionRange> annotations,
    ApiVersion apiVersion ) : IServerStreamWriter<T>
    where T : class
{
    public WriteOptions? WriteOptions
    {
        get => stream.WriteOptions;
        set => stream.WriteOptions = value;
    }

    public Task WriteAsync( T message )
    {
        MessageFields.Filter( annotations, message, apiVersion );
        return stream.WriteAsync( message );
    }

    public Task WriteAsync( T message, CancellationToken cancellationToken )
    {
        MessageFields.Filter( annotations, message, apiVersion );
        return stream.WriteAsync( message, cancellationToken );
    }
}