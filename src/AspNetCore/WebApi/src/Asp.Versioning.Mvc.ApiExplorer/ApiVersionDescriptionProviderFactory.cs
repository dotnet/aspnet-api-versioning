// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable CA1812 // Avoid uninstantiated internal classes

namespace Microsoft.AspNetCore.Builder;

using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;

internal sealed class ApiVersionDescriptionProviderFactory : IApiVersionDescriptionProviderFactory
{
    private readonly ISunsetPolicyManager sunsetPolicyManager;
    private readonly IDeprecationPolicyManager deprecationPolicyManager;
    private readonly IApiVersionMetadataCollationProvider[] providers;
    private readonly IEndpointInspector endpointInspector;
    private readonly IOptions<ApiExplorerOptions> options;

    public ApiVersionDescriptionProviderFactory(
        ISunsetPolicyManager sunsetPolicyManager,
        IDeprecationPolicyManager deprecationPolicyManager,
        IEnumerable<IApiVersionMetadataCollationProvider> providers,
        IEndpointInspector endpointInspector,
        IOptions<ApiExplorerOptions> options )
    {
        this.sunsetPolicyManager = sunsetPolicyManager;
        this.deprecationPolicyManager = deprecationPolicyManager;
        this.providers = providers.ToArray();
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