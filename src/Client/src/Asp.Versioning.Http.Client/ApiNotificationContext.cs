// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Http;

/// <summary>
/// Represents the arguments for a HTTP client API event.
/// </summary>
public class ApiNotificationContext
{
    private SunsetPolicy? sunsetPolicy;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiNotificationContext"/> class.
    /// </summary>
    /// <param name="response">The current HTTP response.</param>
    /// <param name="apiVersion">The requested API version.</param>
    public ApiNotificationContext( HttpResponseMessage response, ApiVersion apiVersion )
    {
        Response = response ?? throw new System.ArgumentNullException( nameof( response ) );
        ApiVersion = apiVersion ?? throw new System.ArgumentNullException( nameof( apiVersion ) );
    }

    /// <summary>
    /// Gets the current HTTP response.
    /// </summary>
    /// <value>The current <see cref="HttpResponseMessage">HTTP response</see>.</value>
    public HttpResponseMessage Response { get; }

    /// <summary>
    /// Gets the requested API version.
    /// </summary>
    /// <value>The requested <see cref="ApiVersion">API version</see>.</value>
    public ApiVersion ApiVersion { get; }

    /// <summary>
    /// Gets the API sunset policy reported in the response.
    /// </summary>
    /// <value>The reported API <see cref="SunsetPolicy">sunset policy</see>.</value>
    public SunsetPolicy SunsetPolicy => sunsetPolicy ??= Response.ReadSunsetPolicy();
}