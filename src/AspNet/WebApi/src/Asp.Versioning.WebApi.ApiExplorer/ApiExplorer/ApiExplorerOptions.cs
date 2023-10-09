// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.ApiExplorer;

using Asp.Versioning.Routing;
using System.Web.Http;

/// <content>
/// Provides additional implementation specific to ASP.NET Web API.
/// </content>
public partial class ApiExplorerOptions
{
    private readonly Lazy<ApiVersioningOptions> options;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiExplorerOptions"/> class.
    /// </summary>
    /// <param name="configuration">The current <see cref="HttpConfiguration">configuration</see> associated with the options.</param>
    public ApiExplorerOptions( HttpConfiguration configuration ) => options = new( configuration.GetApiVersioningOptions );

    /// <summary>
    /// Gets the default API version applied to services that do not have explicit versions.
    /// </summary>
    /// <value>The default <see cref="ApiVersion">API version</see>.</value>
    public ApiVersion DefaultApiVersion => options.Value.DefaultApiVersion;

    /// <summary>
    /// Gets a value indicating whether a default version is assumed when a client does
    /// does not provide a service API version.
    /// </summary>
    /// <value>True if the a default API version should be assumed when a client does not
    /// provide a service API version; otherwise, false. The default value derives from
    /// <see cref="ApiVersioningOptions.AssumeDefaultVersionWhenUnspecified"/>.</value>
    public bool AssumeDefaultVersionWhenUnspecified => options.Value.AssumeDefaultVersionWhenUnspecified;

    /// <summary>
    /// Gets the source for defining API version parameters.
    /// </summary>
    /// <value>The <see cref="IApiVersionParameterSource">API version parameter source</see> used to describe API version parameters.</value>
    public IApiVersionParameterSource ApiVersionParameterSource => options.Value.ApiVersionReader;

    /// <summary>
    /// Gets the name associated with the API version route constraint.
    /// </summary>
    /// <value>The name associated with the <see cref="ApiVersionRouteConstraint">API version route constraint</see>.</value>
    public string RouteConstraintName => options.Value.RouteConstraintName;

    /// <summary>
    /// Gets or sets the API version selector.
    /// </summary>
    /// <value>An <see cref="IApiVersionSelector">API version selector</see> object.</value>
    public IApiVersionSelector ApiVersionSelector
    {
        get => apiVersionSelector ?? options.Value.ApiVersionSelector;
        set => apiVersionSelector = value;
    }
}