// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using System.Net.Http;

/// <summary>
/// Defines the behavior of a factory to produce <see cref="ProblemDetails">problem details</see>.
/// </summary>
public interface IProblemDetailsFactory
{
    /// <summary>
    /// Creates and returns a new problem details instance.
    /// </summary>
    /// <param name="request">The current <see cref="HttpRequestMessage">HTTP request</see>.</param>
    /// <param name="statusCode">The value for <see cref="ProblemDetails.Status"/>.</param>
    /// <param name="title">The value for <see cref="ProblemDetails.Title" />.</param>
    /// <param name="type">The value for <see cref="ProblemDetails.Type" />.</param>
    /// <param name="detail">The value for <see cref="ProblemDetails.Detail" />.</param>
    /// <param name="instance">The value for <see cref="ProblemDetails.Instance" />.</param>
    /// <returns>A new <see cref="ProblemDetails"/> instance.</returns>
    ProblemDetails CreateProblemDetails(
        HttpRequestMessage request,
        int? statusCode = null,
        string? title = null,
        string? type = null,
        string? detail = null,
        string? instance = null );
}