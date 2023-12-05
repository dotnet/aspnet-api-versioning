// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Http;

/// <summary>
/// Represents a HTTP message handler than handles sending and receiving API version information.
/// </summary>
public class ApiVersionHandler : DelegatingHandler
{
    private readonly IApiVersionWriter apiVersionWriter;
    private readonly ApiVersion apiVersion;
    private readonly ApiVersionHeaderEnumerable enumerable;
    private readonly IApiNotification notification;
    private readonly IApiVersionParser parser;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiVersionHandler"/> class.
    /// </summary>
    /// <param name="apiVersionWriter">The <see cref="IApiVersionWriter">writer</see> used to write
    /// API versions into HTTP requests.</param>
    /// <param name="apiVersion">The associated <see cref="ApiVersion">API version</see>.</param>
    /// <param name="notification">The optional <see cref="IApiNotification">API notification</see>
    /// that is signaled when changes to an API are detected.</param>
    /// <param name="parser">The optional <see cref="IApiVersionParser">parser</see> used to process
    /// API versions from HTTP responses.</param>
    /// <param name="enumerable">The optional <see cref="ApiVersionHeaderEnumerable">enumerable</see>
    /// used to enumerate retrieved API versions from HTTP responses.</param>
    public ApiVersionHandler(
        IApiVersionWriter apiVersionWriter,
        ApiVersion apiVersion,
        IApiNotification? notification = default,
        IApiVersionParser? parser = default,
        ApiVersionHeaderEnumerable? enumerable = default )
    {
        this.apiVersionWriter = apiVersionWriter ?? throw new System.ArgumentNullException( nameof( apiVersionWriter ) );
        this.apiVersion = apiVersion ?? throw new System.ArgumentNullException( nameof( apiVersion ) );
        this.notification = notification ?? ApiNotification.None;
        this.parser = parser ?? ApiVersionParser.Default;
        this.enumerable = enumerable ?? new();
    }

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync( HttpRequestMessage request, CancellationToken cancellationToken )
    {
        apiVersionWriter.Write( request, apiVersion );

        var response = await base.SendAsync( request, cancellationToken ).ConfigureAwait( false );

        if ( IsDeprecatedApi( response ) )
        {
            response.RequestMessage ??= request;
            await notification.OnApiDeprecatedAsync( new( response, apiVersion ), cancellationToken ).ConfigureAwait( false );
        }
        else if ( IsNewApiAvailable( response ) )
        {
            response.RequestMessage ??= request;
            await notification.OnNewApiAvailableAsync( new( response, apiVersion ), cancellationToken ).ConfigureAwait( false );
        }

        return response;
    }

    /// <summary>
    /// Determines whether the requested API is deprecated.
    /// </summary>
    /// <param name="response">The <see cref="HttpResponseMessage">HTTP response</see> from the requested API.</param>
    /// <returns>True if the requested API has been deprecated; otherwise, false.</returns>
    protected virtual bool IsDeprecatedApi( HttpResponseMessage response )
    {
        ArgumentNullException.ThrowIfNull( response );

        foreach ( var reportedApiVersion in enumerable.Deprecated( response, parser ) )
        {
            // don't use '==' operator because a derived type may not overload it
            if ( apiVersion.Equals( reportedApiVersion ) )
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether the requested API has a newer, supported version.
    /// </summary>
    /// <param name="response">The <see cref="HttpResponseMessage">HTTP response</see> from the requested API.</param>
    /// <returns>True if the requested API has a newer, supported version than the one requested; otherwise, false.</returns>
    protected virtual bool IsNewApiAvailable( HttpResponseMessage response )
    {
        ArgumentNullException.ThrowIfNull( response );

        foreach ( var reportedApiVersion in enumerable.Supported( response, parser ) )
        {
            // don't use '<' operator because a derived type may not overload it
            if ( apiVersion.CompareTo( reportedApiVersion ) < 0 )
            {
                return true;
            }
        }

        return false;
    }
}