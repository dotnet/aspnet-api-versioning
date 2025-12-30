// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable CA1812 // Avoid uninstantiated internal classes

namespace Asp.Versioning.ApiExplorer;

using Asp.Versioning;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;

internal sealed class ApiVersionDescriptionProviderFactory : IApiVersionDescriptionProviderFactory
{
    private readonly IPolicyManager<SunsetPolicy> sunsetPolicyManager;
    private readonly IPolicyManager<DeprecationPolicy> deprecationPolicyManager;
    private readonly IApiVersionMetadataCollationProvider[] providers;
    private readonly IEndpointInspector endpointInspector;
    private readonly IOptions<ApiExplorerOptions> options;

    public ApiVersionDescriptionProviderFactory(
        IPolicyManager<SunsetPolicy> sunsetPolicyManager,
        IPolicyManager<DeprecationPolicy> deprecationPolicyManager,
        IEnumerable<IApiVersionMetadataCollationProvider> providers,
        IEndpointInspector endpointInspector,
        IOptions<ApiExplorerOptions> options )
    {
        this.sunsetPolicyManager = sunsetPolicyManager;
        this.deprecationPolicyManager = deprecationPolicyManager;
        this.providers = [.. providers];
        this.endpointInspector = endpointInspector;
        this.options = options;
    }

    public IApiVersionDescriptionProvider Create( EndpointDataSource endpointDataSource )
    {
        var collators = new List<IApiVersionMetadataCollationProvider>( capacity: providers.Length + 1 )
        {
            new EndpointApiVersionMetadataCollationProvider( endpointDataSource, endpointInspector ),
        };

        collators.AddRange( providers );

        return new DefaultApiVersionDescriptionProvider( collators, sunsetPolicyManager, deprecationPolicyManager, options );
    }
}