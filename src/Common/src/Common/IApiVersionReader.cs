// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

#if NETFRAMEWORK
using HttpRequest = System.Net.Http.HttpRequestMessage;
#else
using Microsoft.AspNetCore.Http;
#endif

/// <summary>
/// Defines the behavior of an API version reader.
/// </summary>
#if !NETFRAMEWORK
[CLSCompliant( false )]
#endif
public interface IApiVersionReader : IApiVersionParameterSource
{
    /// <summary>
    /// Reads the API version value from a request.
    /// </summary>
    /// <param name="request">The <see cref="HttpRequest">HTTP request</see> to read the API version from.</param>
    /// <returns>The raw, unparsed API version values read from the request.</returns>
    IReadOnlyList<string> Read( HttpRequest request );
}