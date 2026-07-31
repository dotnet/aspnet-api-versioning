// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable CA1812

namespace Asp.Versioning;

using Google.Protobuf.Reflection;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

internal sealed class FieldInterceptor : Interceptor
{
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation )
    {
        if ( !TryResolve( context, out var annotations, out var apiVersion ) )
        {
            return await continuation( request, context ).ConfigureAwait( false );
        }

        MessageFields.Validate( annotations, request, apiVersion );

        var response = await continuation( request, context ).ConfigureAwait( false );

        MessageFields.Filter( annotations, response, apiVersion );

        return response;
    }

    public override Task<TResponse> ClientStreamingServerHandler<TRequest, TResponse>(
        IAsyncStreamReader<TRequest> requestStream,
        ServerCallContext context,
        ClientStreamingServerMethod<TRequest, TResponse> continuation )
    {
        if ( !TryResolve( context, out var annotations, out var apiVersion ) )
        {
            return continuation( requestStream, context );
        }

        return Filtered( requestStream, context, continuation, annotations, apiVersion );

        static async Task<TResponse> Filtered(
            IAsyncStreamReader<TRequest> requestStream,
            ServerCallContext context,
            ClientStreamingServerMethod<TRequest, TResponse> continuation,
            IAnnotation<FieldDescriptor, ApiVersionRange> annotations,
            ApiVersion apiVersion )
        {
            var reader = new ValidatingStreamReader<TRequest>( requestStream, annotations, apiVersion );
            var response = await continuation( reader, context ).ConfigureAwait( false );

            MessageFields.Filter( annotations, response, apiVersion );

            return response;
        }
    }

    public override Task ServerStreamingServerHandler<TRequest, TResponse>(
        TRequest request,
        IServerStreamWriter<TResponse> responseStream,
        ServerCallContext context,
        ServerStreamingServerMethod<TRequest, TResponse> continuation )
    {
        if ( !TryResolve( context, out var annotations, out var apiVersion ) )
        {
            return continuation( request, responseStream, context );
        }

        MessageFields.Validate( annotations, request, apiVersion );

        var writer = new FilteringStreamWriter<TResponse>( responseStream, annotations, apiVersion );

        return continuation( request, writer, context );
    }

    public override Task DuplexStreamingServerHandler<TRequest, TResponse>(
        IAsyncStreamReader<TRequest> requestStream,
        IServerStreamWriter<TResponse> responseStream,
        ServerCallContext context,
        DuplexStreamingServerMethod<TRequest, TResponse> continuation )
    {
        if ( !TryResolve( context, out var annotations, out var apiVersion ) )
        {
            return continuation( requestStream, responseStream, context );
        }

        var reader = new ValidatingStreamReader<TRequest>( requestStream, annotations, apiVersion );
        var writer = new FilteringStreamWriter<TResponse>( responseStream, annotations, apiVersion );

        return continuation( reader, writer, context );
    }

    // a call that did not resolve an API version is passed through untouched. there is no version to compare a
    // field against, so no field can be shown to be out of range
    private static bool TryResolve(
        ServerCallContext context,
        [NotNullWhen( true )] out IAnnotation<FieldDescriptor, ApiVersionRange>? annotations,
        [NotNullWhen( true )] out ApiVersion? apiVersion )
    {
        if ( context.GetHttpContext() is not { } http ||
             http.ApiVersioningFeature.RequestedApiVersion is not { } version )
        {
            annotations = default;
            apiVersion = default;
            return false;
        }

        annotations = http.RequestServices.GetRequiredService<IAnnotation<FieldDescriptor, ApiVersionRange>>();
        apiVersion = version;

        return true;
    }
}