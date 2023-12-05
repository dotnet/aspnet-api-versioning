// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

#if NETFRAMEWORK
using System.Web.Http;
using HttpResponse = System.Net.Http.HttpResponseMessage;
#else
using Microsoft.AspNetCore.Http;
#endif
using static Asp.Versioning.ApiVersionMapping;

/// <summary>
/// Represents the default API version reporter.
/// </summary>
#if !NETFRAMEWORK
[CLSCompliant( false )]
#endif
public sealed partial class DefaultApiVersionReporter : IReportApiVersions
{
    private const string ApiSupportedVersions = "api-supported-versions";
    private const string ApiDeprecatedVersions = "api-deprecated-versions";
    private const string Sunset = nameof( Sunset );
    private const string Link = nameof( Link );
    private readonly ISunsetPolicyManager sunsetPolicyManager;
    private readonly string apiSupportedVersionsName;
    private readonly string apiDeprecatedVersionsName;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultApiVersionReporter"/> class.
    /// </summary>
    /// <param name="sunsetPolicyManager">The <see cref="ISunsetPolicyManager">manager</see> used to resolve sunset policies.</param>
    /// <param name="supportedHeaderName">The HTTP header name used for supported API versions.
    /// The default value is "api-supported-versions".</param>
    /// <param name="deprecatedHeaderName">THe HTTP header name used for deprecated API versions.
    /// The default value is "api-deprecated-versions".</param>
    /// <param name="mapping">One or more of API versioning mappings. The default value is
    /// <see cref="ApiVersionMapping.Explicit"/> and <see cref="ApiVersionMapping.Implicit"/>.</param>
    public DefaultApiVersionReporter(
        ISunsetPolicyManager sunsetPolicyManager,
        string supportedHeaderName = ApiSupportedVersions,
        string deprecatedHeaderName = ApiDeprecatedVersions,
        ApiVersionMapping mapping = Explicit | Implicit )
    {
        ArgumentNullException.ThrowIfNull( sunsetPolicyManager );
        ArgumentException.ThrowIfNullOrEmpty( supportedHeaderName );
        ArgumentException.ThrowIfNullOrEmpty( deprecatedHeaderName );

        this.sunsetPolicyManager = sunsetPolicyManager;
        apiSupportedVersionsName = supportedHeaderName;
        apiDeprecatedVersionsName = deprecatedHeaderName;
        Mapping = mapping;
    }

    /// <inheritdoc />
    public ApiVersionMapping Mapping { get; }

    /// <inheritdoc />
    public void Report( HttpResponse response, ApiVersionModel apiVersionModel )
    {
        ArgumentNullException.ThrowIfNull( response );
        ArgumentNullException.ThrowIfNull( apiVersionModel );

        if ( apiVersionModel.IsApiVersionNeutral )
        {
            return;
        }

        var headers = response.Headers;

        AddApiVersionHeader( headers, apiSupportedVersionsName, apiVersionModel.SupportedApiVersions );
        AddApiVersionHeader( headers, apiDeprecatedVersionsName, apiVersionModel.DeprecatedApiVersions );

#if NETFRAMEWORK
        if ( response.RequestMessage is not HttpRequestMessage request ||
             request.GetActionDescriptor()?.GetApiVersionMetadata() is not ApiVersionMetadata metadata )
        {
            return;
        }

        var version = request.GetRequestedApiVersion();
#else
        var context = response.HttpContext;

        if ( context.GetEndpoint()?.Metadata.GetMetadata<ApiVersionMetadata>() is not ApiVersionMetadata metadata )
        {
            return;
        }

        var version = context.GetRequestedApiVersion();
#endif
        var name = metadata.Name;

        if ( sunsetPolicyManager.TryGetPolicy( name, version, out var policy ) ||
           ( !string.IsNullOrEmpty( name ) && sunsetPolicyManager.TryGetPolicy( name, out policy ) ) ||
           ( version != null && sunsetPolicyManager.TryGetPolicy( version, out policy ) ) )
        {
            response.WriteSunsetPolicy( policy );
        }
    }
}