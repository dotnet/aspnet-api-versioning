// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

#if NETFRAMEWORK
using HttpRequest = System.Net.Http.HttpRequestMessage;
#else
using Microsoft.AspNetCore.Http;
#endif

/// <summary>
/// Defines the behavior of an API version selector.
/// </summary>
public partial interface IApiVersionSelector
{
    /// <summary>
    /// Selects an API version given the specified HTTP request and API version information.
    /// </summary>
    /// <param name="request">The current <see cref="HttpRequest">HTTP request</see> to select the version for.</param>
    /// <param name="model">The <see cref="ApiVersionModel">model</see> to select the version from.</param>
    /// <returns>The selected <see cref="ApiVersion">API version</see>.</returns>
    ApiVersion SelectVersion( HttpRequest request, ApiVersionModel model );
}