// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Microsoft.AspNetCore.Builder;

using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

internal sealed class ApiVersionDescriptionProviderFactory : IApiVersionDescriptionProviderFactory
{
    private readonly IServiceProvider serviceProvider;
    private readonly Func<EndpointDataSource, IActionDescriptorCollectionProvider, ISunsetPolicyManager, IOptions<ApiExplorerOptions>, IApiVersionDescriptionProvider> activator;

    public ApiVersionDescriptionProviderFactory(
        IServiceProvider serviceProvider,
        Func<EndpointDataSource, IActionDescriptorCollectionProvider, ISunsetPolicyManager, IOptions<ApiExplorerOptions>, IApiVersionDescriptionProvider> activator )
    {
        this.serviceProvider = serviceProvider;
        this.activator = activator;
    }

    public IApiVersionDescriptionProvider Create( EndpointDataSource endpointDataSource )
    {
        var actionDescriptorCollectionProvider = serviceProvider.GetRequiredService<IActionDescriptorCollectionProvider>();
        var sunsetPolicyManager = serviceProvider.GetRequiredService<ISunsetPolicyManager>();
        var options = serviceProvider.GetRequiredService<IOptions<ApiExplorerOptions>>();
        return activator( endpointDataSource, actionDescriptorCollectionProvider, sunsetPolicyManager, options );
    }
}