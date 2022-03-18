// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Mvc.UsingAttributes;

using Asp.Versioning.Mvc.UsingAttributes.Controllers;

public class OverlappingRouteTemplateFixture : HttpServerFixture
{
    public OverlappingRouteTemplateFixture() => FilteredControllerTypes.Add( typeof( OverlappingRouteTemplateController ) );

    protected override void OnAddApiVersioning( ApiVersioningOptions options ) => options.ReportApiVersions = true;
}