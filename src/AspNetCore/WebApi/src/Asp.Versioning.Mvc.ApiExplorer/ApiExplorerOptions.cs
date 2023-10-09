// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.ApiExplorer;

using Asp.Versioning.Routing;
using Microsoft.AspNetCore.Http;

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
        get => parameterSource ??= ApiVersionReader.Default;
        set => parameterSource = value;
    }

    /// <summary>
    /// Gets or sets the name associated with the API version route constraint.
    /// </summary>
    /// <value>The name associated with the <see cref="ApiVersionRouteConstraint">API version route constraint</see>.</value>
    public string RouteConstraintName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the API version selector.
    /// </summary>
    /// <value>An <see cref="IApiVersionSelector">API version selector</see> object.</value>
    [CLSCompliant( false )]
    public IApiVersionSelector ApiVersionSelector
    {
        get => apiVersionSelector ??= new DefaultApiVersionSelector( this );
        set => apiVersionSelector = value;
    }

    /// <summary>
    /// Gets or sets the function used to format the combination of a group name and API version.
    /// </summary>
    /// <value>The <see cref="FormatGroupNameCallback">callback</see> used to format the combination of
    /// a group name and API version. The default value is <c>null</c>.</value>
    /// <remarks>The specified callback will only be invoked if a group name has been configured. The API
    /// version will be provided formatted according to the <see cref="GroupNameFormat">group name format</see>.</remarks>
    public FormatGroupNameCallback? FormatGroupName { get; set; }

    private sealed class DefaultApiVersionSelector : IApiVersionSelector
    {
        private readonly ApiExplorerOptions options;

        public DefaultApiVersionSelector( ApiExplorerOptions options ) => this.options = options;

        public ApiVersion SelectVersion( HttpRequest request, ApiVersionModel model ) => options.DefaultApiVersion;
    }
}