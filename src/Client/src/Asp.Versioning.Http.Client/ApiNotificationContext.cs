// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Http;

/// <summary>
/// Represents the arguments for a HTTP client API event.
/// </summary>
public class ApiNotificationContext
{
    private SunsetPolicy? sunsetPolicy;
    private DeprecationPolicy? deprecationPolicy;

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
    /// Initializes a new instance of the <see cref="ApiNotificationContext"/> class.
    /// </summary>
    /// <param name="response">The current HTTP response.</param>
    /// <param name="apiVersion">The requested API version.</param>
    /// <param name="sunsetPolicy">The sunset policy which was previously read from the <paramref name="response"/>.</param>
    /// <param name="deprecationPolicy">The deprecation policy which was previously read from the <paramref name="response"/>.</param>
    public ApiNotificationContext( HttpResponseMessage response, ApiVersion apiVersion, SunsetPolicy? sunsetPolicy = null, DeprecationPolicy? deprecationPolicy = null )
        : this( response, apiVersion )
    {
        this.sunsetPolicy = sunsetPolicy;
        this.deprecationPolicy = deprecationPolicy;
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
    public SunsetPolicy SunsetPolicy => sunsetPolicy ??= Response.SunsetPolicy;

    /// <summary>
    /// Gets the API deprecation policy reported in the response.
    /// </summary>
    /// <value>The reported API <see cref="DeprecationPolicy">deprecation policy</see>.</value>
    public DeprecationPolicy DeprecationPolicy => deprecationPolicy ??= Response.DeprecationPolicy;
}