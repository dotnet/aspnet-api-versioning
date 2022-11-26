// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Microsoft.AspNetCore.Builder;

using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

internal sealed class ApiVersionDescriptionProviderFactory : IApiVersionDescriptionProviderFactory
{
    private readonly IServiceProvider serviceProvider;
    private readonly Func<IEnumerable<IApiVersionMetadataCollationProvider>, ISunsetPolicyManager, IOptions<ApiExplorerOptions>, IApiVersionDescriptionProvider> activator;

    public ApiVersionDescriptionProviderFactory(
        IServiceProvider serviceProvider,
        Func<IEnumerable<IApiVersionMetadataCollationProvider>, ISunsetPolicyManager, IOptions<ApiExplorerOptions>, IApiVersionDescriptionProvider> activator )
    {
        this.serviceProvider = serviceProvider;
        this.activator = activator;
    }

    public IApiVersionDescriptionProvider Create( EndpointDataSource endpointDataSource )
    {
        var providers = serviceProvider.GetServices<IApiVersionMetadataCollationProvider>().ToList();

        providers.Insert( 0, new EndpointApiVersionMetadataCollationProvider( endpointDataSource ) );

        var sunsetPolicyManager = serviceProvider.GetRequiredService<ISunsetPolicyManager>();
        var options = serviceProvider.GetRequiredService<IOptions<ApiExplorerOptions>>();

        return activator( providers, sunsetPolicyManager, options );
    }
}