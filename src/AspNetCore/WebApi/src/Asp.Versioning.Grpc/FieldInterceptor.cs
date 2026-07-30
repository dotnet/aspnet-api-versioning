// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable CA1812

namespace Asp.Versioning;

using Asp.Versioning.ApiExplorer;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

internal sealed class FieldInterceptor : Interceptor
{
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation )
    {
        var response = await continuation( request, context ).ConfigureAwait( false );

        if ( response is not IMessage message )
        {
            return response;
        }

        var http = context.GetHttpContext();
        var feature = http.ApiVersioningFeature;

        if ( feature.RequestedApiVersion is not { } apiVersion )
        {
            return response;
        }

        var filter = http.RequestServices.GetRequiredService<IMemberFilter<FieldDescriptor>>();

        FilterFields( filter, message, apiVersion );

        return response;
    }

    private static void FilterFields( IMemberFilter<FieldDescriptor> filter, IMessage message, ApiVersion apiVersion )
    {
        var descriptor = message.Descriptor;

        foreach ( var field in descriptor.Fields.InDeclarationOrder() )
        {
            if ( !filter.IsVisible( field, apiVersion ) )
            {
                field.Accessor.Clear( message );
                continue;
            }

            if ( field.FieldType == FieldType.Message && field.Accessor.GetValue( message ) is IMessage nested )
            {
                FilterFields( filter, nested, apiVersion );
            }
        }
    }
}