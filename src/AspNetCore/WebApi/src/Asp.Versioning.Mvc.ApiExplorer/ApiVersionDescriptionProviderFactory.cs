// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable SA1135 // Using directives should be qualified
#pragma warning disable SA1200 // Using directives should be placed correctly

using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Builder;

using Microsoft.AspNetCore.Routing;
using Activator = Func<IEnumerable<IApiVersionMetadataCollationProvider>, ISunsetPolicyManager, IOptions<ApiExplorerOptions>, IApiVersionDescriptionProvider>;

internal sealed class ApiVersionDescriptionProviderFactory : IApiVersionDescriptionProviderFactory
{
    private readonly ISunsetPolicyManager sunsetPolicyManager;
    private readonly IApiVersionMetadataCollationProvider[] providers;
    private readonly IOptions<ApiExplorerOptions> options;
    private readonly Activator activator;

    public ApiVersionDescriptionProviderFactory(
        Activator activator,
        ISunsetPolicyManager sunsetPolicyManager,
        IEnumerable<IApiVersionMetadataCollationProvider> providers,
        IOptions<ApiExplorerOptions> options )
    {
        this.activator = activator;
        this.sunsetPolicyManager = sunsetPolicyManager;
        this.providers = providers.ToArray();
        this.options = options;
    }

    public IApiVersionDescriptionProvider Create( EndpointDataSource endpointDataSource )
    {
        var collators = new List<IApiVersionMetadataCollationProvider>( capacity: providers.Length + 1 )
        {
            new EndpointApiVersionMetadataCollationProvider( endpointDataSource ),
        };

        collators.AddRange( providers );

        return activator( collators, sunsetPolicyManager, options );
    }
}