// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Http;

using Microsoft.Extensions.Logging;

/// <summary>
/// Represents an <see cref="IApiNotification">API notification</see> that uses a
/// <see cref="ILogger{TCategoryName}"/> to log the information it receives.
/// </summary>
/// <typeparam name="T">The type used to derive the category name.</typeparam>
public class ApiVersionHandlerLogger<T> : ApiNotification
{
    private readonly ILogger logger;
    private readonly IApiVersionParser parser;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiVersionHandlerLogger{T}"/> class.
    /// </summary>
    /// <param name="logger">The <see cref="ILogger{TCategoryName}">logger</see> used to log API notifications.</param>
    /// <param name="parser">The <see cref="IApiVersionParser">parser</see> used to process API versions.</param>
    public ApiVersionHandlerLogger( ILogger<T> logger, IApiVersionParser parser )
    {
        this.logger = logger ?? throw new ArgumentNullException( nameof( logger ) );
        this.parser = parser ?? throw new ArgumentNullException( nameof( parser ) );
    }

    /// <inheritdoc />
    protected override void OnApiDeprecated( ApiNotificationContext context )
    {
        if ( context == null )
        {
            throw new ArgumentNullException( nameof( context ) );
        }

        var requestUrl = context.Response.RequestMessage!.RequestUri!;
        var apiVersion = context.ApiVersion;
        var sunsetPolicy = context.SunsetPolicy;

        logger.ApiVersionDeprecated( requestUrl, apiVersion, sunsetPolicy );
    }

    /// <inheritdoc />
    protected override void OnNewApiAvailable( ApiNotificationContext context )
    {
        if ( context == null )
        {
            throw new ArgumentNullException( nameof( context ) );
        }

        var requestUrl = context.Response.RequestMessage!.RequestUri!;
        var currentApiVersion = context.ApiVersion;
        var sunsetPolicy = context.SunsetPolicy;
        var newApiVersion = ApiVersionEnumerator.Supported( context.Response, parser ).Max() ?? currentApiVersion;

        logger.NewApiVersionAvailable( requestUrl, currentApiVersion, newApiVersion, sunsetPolicy );
    }
}