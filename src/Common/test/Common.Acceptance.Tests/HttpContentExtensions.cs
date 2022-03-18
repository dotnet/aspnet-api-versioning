// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

#if !NETFRAMEWORK
using Microsoft.AspNetCore.Mvc;
#endif
using System.Net.Http;
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
        content.ReadAsAsync<ProblemDetails>( MediaTypeFormatters, cancellationToken );

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