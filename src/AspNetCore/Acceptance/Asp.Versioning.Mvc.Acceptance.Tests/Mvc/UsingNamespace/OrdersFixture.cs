// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Mvc.UsingNamespace;

using Asp.Versioning;
using Asp.Versioning.Conventions;

public class OrdersFixture : HttpServerFixture
{
    public OrdersFixture()
    {
        FilteredControllerTypes.Add( typeof( Controllers.V1.OrdersController ) );
        FilteredControllerTypes.Add( typeof( Controllers.V2.OrdersController ) );
        FilteredControllerTypes.Add( typeof( Controllers.V3.OrdersController ) );
    }

    protected override void OnAddApiVersioning( ApiVersioningOptions options ) =>
        options.ReportApiVersions = true;

    protected override void OnAddMvcApiVersioning( MvcApiVersioningOptions options ) =>
        options.Conventions.Add( new VersionByNamespaceConvention() );
}