// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace System.Net.Http;

using Asp.Versioning;
using System.Net.Http.Formatting;

internal static class HttpContentExtensions
{
    private static readonly JsonMediaTypeFormatter ProblemDetailsMediaTypeFormatter = new()
    {
        SupportedMediaTypes = { new( ProblemDetailsDefaults.MediaType.Json ) },
    };
    private static readonly IEnumerable<MediaTypeFormatter> MediaTypeFormatters = new[] { ProblemDetailsMediaTypeFormatter };

    public static Task<ProblemDetails> ReadAsProblemDetailsAsync(
        this HttpContent content,
        CancellationToken cancellationToken = default ) =>
        content.SimumateOverTheWireAsync<ProblemDetails>( cancellationToken );

#pragma warning disable IDE0060 // Remove unused parameter
    public static Task<T> ReadAsExampleAsync<T>(
        this HttpContent content,
        T example,
        CancellationToken cancellationToken = default ) =>
        content.SimumateOverTheWireAsync<T>( cancellationToken );
#pragma warning restore IDE0060 // Remove unused parameter

    private static async Task<T> SimumateOverTheWireAsync<T>(
        this HttpContent content,
        CancellationToken cancellationToken = default )
    {
        if ( content is not ObjectContent server )
        {
            return await content.ReadAsAsync<T>( MediaTypeFormatters, cancellationToken ).ConfigureAwait( false );
        }

        using var stream = new MemoryStream();

        await server.Formatter.WriteToStreamAsync(
            server.ObjectType,
            server.Value,
            stream,
            content,
            Mock.Of<TransportContext>(),
            cancellationToken ).ConfigureAwait( false );
        await stream.FlushAsync( cancellationToken ).ConfigureAwait( false );
        stream.Position = 0L;

        using var client = new StreamContent( stream );

        foreach ( var header in content.Headers )
        {
            client.Headers.TryAddWithoutValidation( header.Key, header.Value );
        }

        return await client.ReadAsAsync<T>( MediaTypeFormatters, cancellationToken ).ConfigureAwait( false );
    }
}