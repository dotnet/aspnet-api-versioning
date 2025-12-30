// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

#if !NETFRAMEWORK
using Microsoft.AspNetCore.Mvc;
#endif
using System.Net.Http;
#if NETFRAMEWORK
using System.Net.Http.Formatting;
#else
using System.Net.Http.Json;
#endif

internal static class HttpContentExtensions
{
#if NETFRAMEWORK
    private static readonly JsonMediaTypeFormatter ProblemDetailsMediaTypeFormatter = new()
    {
        SupportedMediaTypes = { new( ProblemDetailsDefaults.MediaType.Json ) },
    };
    private static readonly IEnumerable<MediaTypeFormatter> MediaTypeFormatters = [ProblemDetailsMediaTypeFormatter];
#endif

    public static Task<ProblemDetails> ReadAsProblemDetailsAsync(
        this HttpContent content,
        CancellationToken cancellationToken = default ) =>
#if NETFRAMEWORK
        content.ReadAsAsync<ProblemDetails>( MediaTypeFormatters, cancellationToken );
#else
        content.ReadFromJsonAsync<ProblemDetails>( cancellationToken );
#endif

#pragma warning disable IDE0060 // Remove unused parameter
#pragma warning disable IDE0079 // Remove unnecessary suppression
    public static Task<T> ReadAsExampleAsync<T>(
        this HttpContent content,
        T example,
        CancellationToken cancellationToken = default ) =>
        content.ReadAsAsync<T>( cancellationToken );
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore IDE0079 // Remove unnecessary suppression
}