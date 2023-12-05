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
    private readonly ApiVersionHeaderEnumerable enumerable;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiVersionHandlerLogger{T}"/> class.
    /// </summary>
    /// <param name="logger">The <see cref="ILogger{TCategoryName}">logger</see> used to log API notifications.</param>
    /// <param name="parser">The <see cref="IApiVersionParser">parser</see> used to process API versions.</param>
    /// <param name="enumerable">The <see cref="ApiVersionHeaderEnumerable">enumerable</see> used to enumerate API versions.</param>
    public ApiVersionHandlerLogger( ILogger<T> logger, IApiVersionParser parser, ApiVersionHeaderEnumerable enumerable )
    {
        this.logger = logger ?? throw new System.ArgumentNullException( nameof( logger ) );
        this.parser = parser ?? throw new System.ArgumentNullException( nameof( parser ) );
        this.enumerable = enumerable ?? throw new System.ArgumentNullException( nameof( enumerable ) );
    }

    /// <inheritdoc />
    protected override void OnApiDeprecated( ApiNotificationContext context )
    {
        ArgumentNullException.ThrowIfNull( context );

        var requestUrl = context.Response.RequestMessage!.RequestUri!;
        var apiVersion = context.ApiVersion;
        var sunsetPolicy = context.SunsetPolicy;

        logger.ApiVersionDeprecated( requestUrl, apiVersion, sunsetPolicy );
    }

    /// <inheritdoc />
    protected override void OnNewApiAvailable( ApiNotificationContext context )
    {
        ArgumentNullException.ThrowIfNull( context );

        var requestUrl = context.Response.RequestMessage!.RequestUri!;
        var currentApiVersion = context.ApiVersion;
        var sunsetPolicy = context.SunsetPolicy;
        var newApiVersion = enumerable.Supported( context.Response, parser ).Max() ?? currentApiVersion;

        logger.NewApiVersionAvailable( requestUrl, currentApiVersion, newApiVersion, sunsetPolicy );
    }
}