// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Microsoft.AspNetCore.Builder;

using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

internal sealed class ApiVersionDescriptionProviderFactory : IApiVersionDescriptionProviderFactory
{
    private readonly ISunsetPolicyManager sunsetPolicyManager;
    private readonly IApiVersionMetadataCollationProvider[] providers;
    private readonly IOptions<ApiExplorerOptions> options;
    private readonly Func<IEnumerable<IApiVersionMetadataCollationProvider>, ISunsetPolicyManager, IOptions<ApiExplorerOptions>, IApiVersionDescriptionProvider> activator;

    public ApiVersionDescriptionProviderFactory(
        Func<IEnumerable<IApiVersionMetadataCollationProvider>, ISunsetPolicyManager, IOptions<ApiExplorerOptions>, IApiVersionDescriptionProvider> activator,
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
        var collators = new List<IApiVersionMetadataCollationProvider>( capacity: providers.Length + 1 );

        collators.Add( new EndpointApiVersionMetadataCollationProvider( endpointDataSource ) );
        collators.AddRange( providers );

        return activator( collators, sunsetPolicyManager, options );
    }
}