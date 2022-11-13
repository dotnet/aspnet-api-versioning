// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

#if NETFRAMEWORK
using HttpRequest = System.Net.Http.HttpRequestMessage;
#else
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
#endif

/// <summary>
/// Defines the behavior of a factory to produce <see cref="ProblemDetails">problem details</see>.
/// </summary>
#if !NETFRAMEWORK
[CLSCompliant( false )]
#endif
public interface IProblemDetailsFactory
{
    /// <summary>
    /// Creates and returns a new problem details instance.
    /// </summary>
    /// <param name="request">The current <see cref="HttpRequest">HTTP request</see>.</param>
    /// <param name="statusCode">The value for <see cref="ProblemDetails.Status"/>.</param>
    /// <param name="title">The value for <see cref="ProblemDetails.Title" />.</param>
    /// <param name="type">The value for <see cref="ProblemDetails.Type" />.</param>
    /// <param name="detail">The value for <see cref="ProblemDetails.Detail" />.</param>
    /// <param name="instance">The value for <see cref="ProblemDetails.Instance" />.</param>
    /// <returns>A new <see cref="ProblemDetails"/> instance.</returns>
    ProblemDetails CreateProblemDetails(
        HttpRequest request,
        int? statusCode = null,
        string? title = null,
        string? type = null,
        string? detail = null,
        string? instance = null );
}