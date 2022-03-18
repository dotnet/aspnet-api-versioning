// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Http.UsingNamespace;

using Asp.Versioning;
using Asp.Versioning.Conventions;
using Asp.Versioning.Routing;
using System.Web.Http;
using System.Web.Http.Routing;

public class OrdersFixture : HttpServerFixture
{
    public OrdersFixture()
    {
        FilteredControllerTypes.Add( typeof( Controllers.V1.OrdersController ) );
        FilteredControllerTypes.Add( typeof( Controllers.V2.OrdersController ) );
        FilteredControllerTypes.Add( typeof( Controllers.V3.OrdersController ) );
    }

    protected override void OnAddApiVersioning( ApiVersioningOptions options )
    {
        options.ReportApiVersions = true;
        options.Conventions.Add( new VersionByNamespaceConvention() );
    }

    protected override void OnConfigure( HttpConfiguration configuration )
    {
        var constraintResolver = new DefaultInlineConstraintResolver()
        {
            ConstraintMap = { ["apiVersion"] = typeof( ApiVersionRouteConstraint ) },
        };

        configuration.MapHttpAttributeRoutes( constraintResolver );
    }
}