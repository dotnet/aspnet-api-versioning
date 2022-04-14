// Copyright (c) .NET Foundation and contributors. All rights reserved.

// REF: https://github.com/dotnet/aspnetcore/blob/main/src/Http/Http.Extensions/src/HttpResponseJsonExtensions.cs
namespace Microsoft.AspNetCore.Http;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Text.Json;

internal static class HttpResponseJsonExtensions
{
    private static readonly JsonSerializerOptions DefaultSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public static Task WriteAsJsonAsync<TValue>(
        this HttpResponse response,
        TValue value,
        JsonSerializerOptions? options,
        string? contentType,
        CancellationToken cancellationToken = default )
    {
        if ( response == null )
        {
            throw new ArgumentNullException( nameof( response ) );
        }

        options ??= ResolveSerializerOptions( response.HttpContext );

        response.ContentType = contentType ?? JsonConstants.JsonContentTypeWithCharset;

        // if no user provided token, pass the RequestAborted token and ignore OperationCanceledException
        if ( !cancellationToken.CanBeCanceled )
        {
            return WriteAsJsonAsyncSlow( response.Body, value, options, response.HttpContext.RequestAborted );
        }

        return JsonSerializer.SerializeAsync( response.Body, value, options, cancellationToken );
    }

    private static async Task WriteAsJsonAsyncSlow<TValue>(
        Stream body,
        TValue value,
        JsonSerializerOptions? options,
        CancellationToken cancellationToken )
    {
        try
        {
            await JsonSerializer.SerializeAsync( body, value, options, cancellationToken ).ConfigureAwait( false );
        }
        catch ( OperationCanceledException )
        {
        }
    }

    private static JsonSerializerOptions ResolveSerializerOptions( HttpContext httpContext )
    {
        // Attempt to resolve options from DI then fallback to default options
        return httpContext.RequestServices?.GetService<IOptions<JsonOptions>>()?.Value?.JsonSerializerOptions ?? DefaultSerializerOptions;
    }

    private static class JsonConstants
    {
        public const string JsonContentType = "application/json";
        public const string JsonContentTypeWithCharset = JsonContentType + "; charset=utf-8";
    }
}