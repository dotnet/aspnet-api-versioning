// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Http.UsingNamespace;

using Asp.Versioning;
using Asp.Versioning.Conventions;
using Asp.Versioning.Routing;
using System.Web.Http;
using System.Web.Http.Routing;
using HelloWorldControllerV1 = Asp.Versioning.Http.UsingNamespace.Controllers.V1.HelloWorldController;
using HelloWorldControllerV2 = Asp.Versioning.Http.UsingNamespace.Controllers.V2.HelloWorldController;
using HelloWorldControllerV3 = Asp.Versioning.Http.UsingNamespace.Controllers.V3.HelloWorldController;

public class HelloWorldFixture : HttpServerFixture
{
    public HelloWorldFixture()
    {
        FilteredControllerTypes.Add( typeof( HelloWorldControllerV1 ) );
        FilteredControllerTypes.Add( typeof( HelloWorldControllerV2 ) );
        FilteredControllerTypes.Add( typeof( HelloWorldControllerV3 ) );
    }

    protected override void OnAddApiVersioning( ApiVersioningOptions options )
    {
        options.ReportApiVersions = true;
        options.DefaultApiVersion = new ApiVersion( 2, 0 );
        options.AssumeDefaultVersionWhenUnspecified = true;
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