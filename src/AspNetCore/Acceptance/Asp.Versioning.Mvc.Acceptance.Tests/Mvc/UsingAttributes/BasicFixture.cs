// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Mvc.UsingAttributes;

using Asp.Versioning.Mvc.UsingAttributes.Controllers;

public class BasicFixture : HttpServerFixture
{
    public BasicFixture()
    {
        FilteredControllerTypes.Add( typeof( ValuesController ) );
        FilteredControllerTypes.Add( typeof( Values2Controller ) );
        FilteredControllerTypes.Add( typeof( HelloWorldController ) );
        FilteredControllerTypes.Add( typeof( HelloWorld2Controller ) );
        FilteredControllerTypes.Add( typeof( PingController ) );
        FilteredControllerTypes.Add( typeof( OrdersController ) );
    }

    protected override void OnAddApiVersioning( ApiVersioningOptions options ) => options.ReportApiVersions = true;
}