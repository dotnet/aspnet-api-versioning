// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using Asp.Versioning.Routing;
#if NETFRAMEWORK
using System.Net;
#endif

/// <summary>
/// Represents the possible options for API versioning.
/// </summary>
public partial class ApiVersioningOptions
{
    private IApiVersionReader? apiVersionReader;
    private IApiVersionSelector? apiVersionSelector;
    private IApiVersioningPolicyBuilder? apiVersioningPolicyBuilder;

    /// <summary>
    /// Gets or sets the name associated with the API version route constraint.
    /// </summary>
    /// <value>The name associated with the <see cref="ApiVersionRouteConstraint">API version route constraint.</see>
    /// The default value is "apiVersion".</value>
    /// <remarks>The route constraint name is only applicable when versioning using the URL segment method. Changing
    /// this property is only necessary if you prefer an alternate name in for the constraint in your route templates;
    /// for example, "api-version" or simply "version".</remarks>
    public string RouteConstraintName { get; set; } = "apiVersion";

    /// <summary>
    /// Gets or sets a value indicating whether requests report the API version compatibility
    /// information in responses.
    /// </summary>
    /// <value>True if the responses contain API version compatibility information; otherwise,
    /// false. The default value is <c>false</c>.</value>
    /// <remarks>
    /// When this property is set to <c>true</c>, the HTTP headers "api-supported-versions" and
    /// "api-deprecated-versions" will be added to all valid service routes. This information is useful
    /// for advertising which versions are supported and scheduled for deprecation to clients. This
    /// information is also useful when supporting the OPTIONS method.
    /// </remarks>
    public bool ReportApiVersions { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether a default version is assumed when a client does
    /// does not provide an API version.
    /// </summary>
    /// <value>True if the a default API version should be assumed when a client does not
    /// provide an API version; otherwise, false. The default value is <c>false</c>.</value>
    /// <remarks>When a default API version is assumed, the version used is based up the
    /// result from <see cref="IApiVersionSelector"/>.</remarks>
    public bool AssumeDefaultVersionWhenUnspecified { get; set; }

    /// <summary>
    /// Gets or sets the default API version applied to services that do not have explicit versions.
    /// </summary>
    /// <value>The default <see cref="ApiVersion">API version</see>. The default value is <see cref="ApiVersion.Default"/>.</value>
    public ApiVersion DefaultApiVersion { get; set; } = ApiVersion.Default;

    /// <summary>
    /// Gets or sets the API version reader.
    /// </summary>
    /// <value>An <see cref="IApiVersionReader">API version reader</see> object. The default value
    /// is an instance of the <see cref="QueryStringApiVersionReader"/>.</value>
    /// <remarks>The <see cref="IApiVersionReader">API version reader</see> is used to read the
    /// API version specified by a client. The default value is the
    /// <see cref="QueryStringApiVersionReader"/>, which only reads the API version from
    /// the "api-version" query string parameter. Replace the default value with an alternate
    /// implementation, such as the <see cref="HeaderApiVersionReader"/>, which
    /// can read the API version from additional information like HTTP headers.</remarks>
#if !NETFRAMEWORK
    [CLSCompliant( false )]
#endif
    public IApiVersionReader ApiVersionReader
    {
        get => apiVersionReader ??= Versioning.ApiVersionReader.Default;
        set => apiVersionReader = value;
    }

    /// <summary>
    /// Gets or sets the API version selector.
    /// </summary>
    /// <value>An <see cref="IApiVersionSelector">API version selector</see> object.
    /// The default value is an instance of the <see cref="DefaultApiVersionSelector"/>.</value>
    /// <remarks>The <see cref="IApiVersionSelector">API version selector</see> is used to select
    /// an appropriate API version when a client does not specify a version. The default value is the
    /// <see cref="DefaultApiVersionSelector"/>, which always selects the <see cref="DefaultApiVersion"/>.</remarks>
#if !NETFRAMEWORK
    [CLSCompliant( false )]
#endif
    public IApiVersionSelector ApiVersionSelector
    {
        get => apiVersionSelector ??= new DefaultApiVersionSelector( this );
        set => apiVersionSelector = value;
    }

    /// <summary>
    /// Gets or sets the builder used for API versioning policies.
    /// </summary>
    /// <value>The <see cref="IApiVersioningPolicyBuilder">API versioning policy builder</see>.</value>
    public IApiVersioningPolicyBuilder Policies
    {
        get => apiVersioningPolicyBuilder ??= new ApiVersioningPolicyBuilder();
        set => apiVersioningPolicyBuilder = value;
    }

    /// <summary>
    /// Gets or sets the HTTP status code used for unsupported versions of an API.
    /// </summary>
    /// <value>The HTTP status code. The default value is 400 (Bad Request).</value>
    /// <remarks>
    /// <para>While any HTTP status code can be provided, the following are the most sensible:</para>
    /// <list type="table">
    ///     <listheader>
    ///         <term>Status</term>
    ///         <description>Description</description>
    ///     </listheader>
    ///     <item>
    ///         <term>400 (Bad Request)</term>
    ///         <description>The API doesn't support this version</description>
    ///     </item>
    ///     <item>
    ///         <term>404 (Not Found)</term>
    ///         <description>The API doesn't exist</description>
    ///     </item>
    ///     <item>
    ///         <term>501 (Not Implemented)</term>
    ///         <description>The API isn't implemented</description>
    ///     </item>
    /// </list>
    /// </remarks>
#if NETFRAMEWORK
    public HttpStatusCode UnsupportedApiVersionStatusCode { get; set; } = HttpStatusCode.BadRequest;
#else
    public int UnsupportedApiVersionStatusCode { get; set; } = 400;
#endif
}