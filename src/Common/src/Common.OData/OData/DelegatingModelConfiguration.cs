// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OData;

#if NETFRAMEWORK
using Microsoft.AspNet.OData.Builder;
#else
using Microsoft.OData.ModelBuilder;
#endif

internal sealed class DelegatingModelConfiguration : IModelConfiguration
{
    private readonly Action<ODataModelBuilder, ApiVersion, string?> action;

    internal DelegatingModelConfiguration( Action<ODataModelBuilder, ApiVersion, string?> action ) => this.action = action;

    public void Apply( ODataModelBuilder builder, ApiVersion apiVersion, string? routePrefix ) => action( builder, apiVersion, routePrefix );
}