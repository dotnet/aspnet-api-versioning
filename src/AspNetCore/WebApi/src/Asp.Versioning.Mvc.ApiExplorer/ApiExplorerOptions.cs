﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.ApiExplorer;

using Asp.Versioning.Routing;

/// <content>
/// Provides additional implementation specific to ASP.NET Core.
/// </content>
public partial class ApiExplorerOptions
{
    private ApiVersion? defaultVersion;
    private IApiVersionParameterSource? parameterSource;

    /// <summary>
    /// Gets or sets the default API version applied to services that do not have explicit versions.
    /// </summary>
    /// <value>The default <see cref="ApiVersion">API version</see>. The default value is <see cref="ApiVersion.Default"/>.</value>
    public ApiVersion DefaultApiVersion
    {
        get => defaultVersion ??= ApiVersion.Default;
        set => defaultVersion = value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether a default version is assumed when a client does
    /// does not provide a service API version.
    /// </summary>
    /// <value>True if the a default API version should be assumed when a client does not
    /// provide a service API version; otherwise, false. The default value derives from
    /// <see cref="ApiVersioningOptions.AssumeDefaultVersionWhenUnspecified"/>.</value>
    public bool AssumeDefaultVersionWhenUnspecified { get; set; }

    /// <summary>
    /// Gets or sets the source for defining API version parameters.
    /// </summary>
    /// <value>The <see cref="IApiVersionParameterSource">API version parameter source</see> used to describe API version parameters.</value>
    public IApiVersionParameterSource ApiVersionParameterSource
    {
        get => parameterSource ??= ApiVersionReader.Combine( new QueryStringApiVersionReader(), new UrlSegmentApiVersionReader() );
        set => parameterSource = value;
    }

    /// <summary>
    /// Gets or sets the name associated with the API version route constraint.
    /// </summary>
    /// <value>The name associated with the <see cref="ApiVersionRouteConstraint">API version route constraint</see>.</value>
    public string RouteConstraintName { get; set; } = string.Empty;
}