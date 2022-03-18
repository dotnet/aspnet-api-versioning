// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Http;

/// <summary>
/// Defines the behavior of an API version writer.
/// </summary>
public interface IApiVersionWriter
{
    /// <summary>
    /// Write an API version to a request.
    /// </summary>
    /// <param name="request">The <see cref="HttpRequestMessage">HTTP request</see> to write the API version to.</param>
    /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> to write.</param>
    void Write( HttpRequestMessage request, ApiVersion apiVersion );
}