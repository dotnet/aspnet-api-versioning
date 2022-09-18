// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

#if NETFRAMEWORK
using System.Web.Http;
using HttpResponse = System.Net.Http.HttpResponseMessage;
#else
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
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
    private readonly string apiSupportedVersionsName;
    private readonly string apiDeprecatedVersionsName;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultApiVersionReporter"/> class.
    /// </summary>
    /// <param name="supportedHeaderName">The HTTP header name used for supported API versions.
    /// The default value is "api-supported-versions".</param>
    /// <param name="deprecatedHeaderName">THe HTTP header name used for deprecated API versions.
    /// The default value is "api-deprecated-versions".</param>
    /// <param name="mapping">One or more of API versioning mappings. The default value is
    /// <see cref="ApiVersionMapping.Explicit"/> and <see cref="ApiVersionMapping.Implicit"/>.</param>
    public DefaultApiVersionReporter(
        string supportedHeaderName = ApiSupportedVersions,
        string deprecatedHeaderName = ApiDeprecatedVersions,
        ApiVersionMapping mapping = Explicit | Implicit )
    {
        Mapping = mapping;

        if ( string.IsNullOrEmpty( apiSupportedVersionsName = supportedHeaderName ) )
        {
            throw new ArgumentNullException( nameof( supportedHeaderName ) );
        }

        if ( string.IsNullOrEmpty( apiDeprecatedVersionsName = deprecatedHeaderName ) )
        {
            throw new ArgumentNullException( nameof( deprecatedHeaderName ) );
        }
    }

    /// <inheritdoc />
    public ApiVersionMapping Mapping { get; }

    /// <inheritdoc />
    public void Report( HttpResponse response, ApiVersionModel apiVersionModel )
    {
        if ( response == null )
        {
            throw new ArgumentNullException( nameof( response ) );
        }

        if ( apiVersionModel == null )
        {
            throw new ArgumentNullException( nameof( apiVersionModel ) );
        }

        if ( apiVersionModel.IsApiVersionNeutral )
        {
            return;
        }

        var headers = response.Headers;

        AddApiVersionHeader( headers, apiSupportedVersionsName, apiVersionModel.SupportedApiVersions );
        AddApiVersionHeader( headers, apiDeprecatedVersionsName, apiVersionModel.DeprecatedApiVersions );

#if NETFRAMEWORK
        var statusCode = (int) response.StatusCode;
#else
        var context = response.HttpContext;
        var statusCode = response.StatusCode;
#endif

        if ( statusCode < 200 || statusCode > 299 )
        {
            return;
        }

#if NETFRAMEWORK
        if ( response.RequestMessage is not HttpRequestMessage request ||
             request.GetActionDescriptor()?.GetApiVersionMetadata() is not ApiVersionMetadata metadata )
        {
            return;
        }

        var name = metadata.Name;
        var policyManager = request.GetConfiguration().DependencyResolver.GetSunsetPolicyManager();
        var version = request.GetRequestedApiVersion();
#else
        if ( context.GetEndpoint()?.Metadata.GetMetadata<ApiVersionMetadata>() is not ApiVersionMetadata metadata )
        {
            return;
        }

        var name = metadata.Name;
        var policyManager = context.RequestServices.GetRequiredService<ISunsetPolicyManager>();
        var version = context.GetRequestedApiVersion();
#endif

        if ( policyManager.TryGetPolicy( name, version, out var policy ) ||
             ( !string.IsNullOrEmpty( name ) && policyManager.TryGetPolicy( name, out policy ) ) ||
             ( version != null && policyManager.TryGetPolicy( version, out policy ) ) )
        {
            response.WriteSunsetPolicy( policy );
        }
    }
}