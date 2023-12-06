// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using Microsoft.AspNetCore.Http;

/// <content>
/// content>
/// Provides additional implementation specific to ASP.NET Core.
/// </content>
[CLSCompliant( false )]
public partial interface IApiVersionSelector
{
    /// <summary>
    /// Selects an API version given the specified HTTP request and API version information.
    /// </summary>
    /// <param name="request">The current <see cref="HttpRequest">HTTP request</see> to select the version for.</param>
    /// <param name="model">The <see cref="ApiVersionModel">model</see> to select the version from.</param>
    /// <param name="cancellationToken">The token that can be used to cancel the operation.</param>
    /// <returns>A <see cref="ValueTask{TResult}">task</see> containing the selected <see cref="ApiVersion">API version</see>.</returns>
    ValueTask<ApiVersion> SelectVersionAsync(
        HttpRequest request,
        ApiVersionModel model,
        CancellationToken cancellationToken ) =>
        ValueTask.FromResult( SelectVersion( request, model ) );
}