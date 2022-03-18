// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Http.Basic;

using Asp.Versioning;
using Asp.Versioning.Http.Basic.Controllers;
using Asp.Versioning.Routing;
using System.Web.Http;
using System.Web.Http.Routing;

public class BasicFixture : HttpServerFixture
{
    public BasicFixture()
    {
        FilteredControllerTypes.Add( typeof( ValuesController ) );
        FilteredControllerTypes.Add( typeof( Values2Controller ) );
        FilteredControllerTypes.Add( typeof( HelloWorldController ) );
        FilteredControllerTypes.Add( typeof( PingController ) );
        FilteredControllerTypes.Add( typeof( OverlappingRouteTemplateController ) );
        FilteredControllerTypes.Add( typeof( OrdersController ) );
    }

    protected override void OnAddApiVersioning( ApiVersioningOptions options )
    {
        options.ReportApiVersions = true;
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