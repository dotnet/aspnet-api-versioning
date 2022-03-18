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

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultApiVersionReporter"/> class.
    /// </summary>
    public DefaultApiVersionReporter() => Mapping = Explicit | Implicit;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultApiVersionReporter"/> class.
    /// </summary>
    /// <param name="mapping">One or more of API versioning mappings.</param>
    public DefaultApiVersionReporter( ApiVersionMapping mapping ) => Mapping = mapping;

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

        AddApiVersionHeader( headers, ApiSupportedVersions, apiVersionModel.SupportedApiVersions );
        AddApiVersionHeader( headers, ApiDeprecatedVersions, apiVersionModel.DeprecatedApiVersions );

#if NETFRAMEWORK
        var request = response.RequestMessage;
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
        var dependencyResolver = request.GetConfiguration().DependencyResolver;
        var policyManager = dependencyResolver.GetSunsetPolicyManager();
        var name = request.GetActionDescriptor().GetApiVersionMetadata().Name;
        var version = request.GetRequestedApiVersion();
#else
        var policyManager = context.RequestServices.GetRequiredService<ISunsetPolicyManager>();
        var name = context.GetEndpoint()!.Metadata.GetMetadata<ApiVersionMetadata>()?.Name;
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