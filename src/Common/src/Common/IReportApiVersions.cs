// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

#if NETFRAMEWORK
using HttpResponse = System.Net.Http.HttpResponseMessage;
#else
using Microsoft.AspNetCore.Http;
#endif

/// <summary>
/// Defines the behavior of an object that reports API versions as HTTP headers.
/// </summary>
#if !NETFRAMEWORK
[CLSCompliant( false )]
#endif
public interface IReportApiVersions
{
    /// <summary>
    /// Gets reported API version mapping.
    /// </summary>
    /// <value>One or more of the <see cref="ApiVersionMapping"/> values.</value>
    ApiVersionMapping Mapping { get; }

    /// <summary>
    /// Reports the API versions defined in the specified models using the provided collection of HTTP headers.
    /// </summary>
    /// <param name="response">The current <see cref="HttpResponse">HTTP response</see>.</param>
    /// <param name="apiVersionModel">The <see cref="ApiVersionModel">model</see> containing the API versions to report.</param>
    void Report( HttpResponse response, ApiVersionModel apiVersionModel );
}