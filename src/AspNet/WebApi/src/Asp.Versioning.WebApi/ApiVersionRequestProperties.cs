// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using Asp.Versioning.Routing;
using System.ComponentModel;
using System.Globalization;
using System.Web.Http;
using System.Web.Http.Controllers;

/// <summary>
/// Represents current API versioning request properties.
/// </summary>
public class ApiVersionRequestProperties
{
    private readonly HttpRequestMessage request;
    private IReadOnlyList<string>? rawApiVersions;
    private ApiVersion? apiVersion;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiVersionRequestProperties"/> class.
    /// </summary>
    /// <param name="request">The current <see cref="HttpRequestMessage">HTTP request</see>.</param>
    public ApiVersionRequestProperties( HttpRequestMessage request ) => this.request = request;

    /// <summary>
    /// Gets or sets the name of the route parameter containing the API Version value.
    /// </summary>
    /// <value>The name of the API version route parameter or <c>null</c>.</value>
    /// <remarks>This property will be <c>null</c> unless versioning by URL segment and the incoming request
    /// matches the <see cref="ApiVersionRouteConstraint">API version route constraint</see>.</remarks>
    public string? RouteParameter { get; set; }

    /// <summary>
    /// Gets or sets the raw, unparsed API versions for the current request.
    /// </summary>
    /// <value>The unparsed API version values for the current request.</value>
    public IReadOnlyList<string> RawRequestedApiVersions
    {
        get => rawApiVersions ??= request.GetApiVersioningOptions().ApiVersionReader.Read( request );
        set => rawApiVersions = value.ToArray();
    }

    /// <summary>
    /// Gets or sets the raw, unparsed API version for the current request.
    /// </summary>
    /// <value>The unparsed API version value for the current request.</value>
    public string? RawRequestedApiVersion
    {
        get
        {
            var values = RawRequestedApiVersions;

            return values.Count switch
            {
                0 => default,
                1 => values[0],
                _ => throw new AmbiguousApiVersionException(
                        string.Format( CultureInfo.CurrentCulture, CommonSR.MultipleDifferentApiVersionsRequested, string.Join( ", ", values ) ),
                        values ),
            };
        }
        set
        {
            rawApiVersions = string.IsNullOrEmpty( value ) ? default : new[] { value! };
        }
    }

    /// <summary>
    /// Gets or sets the API version for the current request.
    /// </summary>
    /// <value>The current <see cref="RequestedApiVersion">API version</see> for the current request.</value>
    /// <remarks>If an API version was not provided for the current request or the value
    /// provided is invalid, this property will return <c>null</c>.</remarks>
    public ApiVersion? RequestedApiVersion
    {
        get
        {
            if ( apiVersion is not null )
            {
                return apiVersion;
            }

            var value = RawRequestedApiVersion;

            if ( string.IsNullOrEmpty( value ) )
            {
                return apiVersion;
            }

            var parser = request.GetConfiguration().GetApiVersionParser();

            try
            {
                apiVersion = parser.Parse( value );
            }
            catch ( FormatException )
            {
                apiVersion = default;
            }

            return apiVersion;
        }
        set => apiVersion = value;
    }

    /// <summary>
    /// Gets or sets the controller selected during a request.
    /// </summary>
    /// <value>The <see cref="HttpControllerDescriptor">controller</see> select during a request.
    /// The default value is <c>null</c>.</value>
    /// <remarks>This API is meant for infrastructure and should not be used by application code.</remarks>
    [EditorBrowsable( EditorBrowsableState.Never )]
    public HttpControllerDescriptor? SelectedController { get; set; }
}