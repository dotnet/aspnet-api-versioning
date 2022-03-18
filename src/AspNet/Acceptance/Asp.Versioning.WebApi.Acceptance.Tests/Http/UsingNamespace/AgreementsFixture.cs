// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Http.UsingNamespace;

using Asp.Versioning;
using Asp.Versioning.Conventions;
using Asp.Versioning.Routing;
using System.Web.Http;
using static System.Web.Http.RouteParameter;

public class AgreementsFixture : HttpServerFixture
{
    public AgreementsFixture()
    {
        FilteredControllerTypes.Add( typeof( Controllers.V1.AgreementsController ) );
        FilteredControllerTypes.Add( typeof( Controllers.V2.AgreementsController ) );
        FilteredControllerTypes.Add( typeof( Controllers.V3.AgreementsController ) );
    }

    protected override void OnAddApiVersioning( ApiVersioningOptions options )
    {
        options.ReportApiVersions = true;
        options.Conventions.Add( new VersionByNamespaceConvention() );
    }

    protected override void OnConfigure( HttpConfiguration configuration )
    {
        configuration.Routes.MapHttpRoute(
           "VersionedQueryString",
           "api/{controller}/{accountId}",
           new { accountId = Optional } );

        configuration.Routes.MapHttpRoute(
            "VersionedUrl",
            "v{apiVersion}/{controller}/{accountId}",
            new { accountId = Optional },
            new { apiVersion = new ApiVersionRouteConstraint() } );
    }
}