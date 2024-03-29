﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

#if NETFRAMEWORK
using HttpRequest = System.Net.Http.HttpRequestMessage;
#else
using Microsoft.AspNetCore.Http;
#endif

/// <summary>
/// Represents a <see cref="IApiVersionSelector">API version selector</see> that selects a constant value.
/// </summary>
#if !NETFRAMEWORK
[CLSCompliant( false )]
#endif
public sealed class ConstantApiVersionSelector : IApiVersionSelector
{
    private readonly ApiVersion version;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConstantApiVersionSelector"/> class.
    /// </summary>
    /// <param name="version">The constant <see cref="ApiVersion">API version</see> the selector returns.</param>
    public ConstantApiVersionSelector( ApiVersion version ) => this.version = version;

    /// <summary>
    /// Selects an API version given the specified HTTP request and API version information.
    /// </summary>
    /// <param name="request">The <see cref="HttpRequest">HTTP request</see> to select the version for.</param>
    /// <param name="model">The <see cref="ApiVersionModel">model</see> to select the version from.</param>
    /// <returns>The selected <see cref="ApiVersion">API version</see>.</returns>
    /// <remarks>This method always returns the constant <see cref="ApiVersion">API version</see> the selector was initialized with.</remarks>
    public ApiVersion SelectVersion( HttpRequest request, ApiVersionModel model ) => version;
}