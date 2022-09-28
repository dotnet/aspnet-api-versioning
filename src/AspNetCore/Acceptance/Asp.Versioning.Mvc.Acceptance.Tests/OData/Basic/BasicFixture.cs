// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OData.Basic;

using Asp.Versioning.OData;
using Asp.Versioning.OData.Basic.Controllers;
using Microsoft.AspNetCore.Builder;

public class BasicFixture : ODataFixture
{
    public BasicFixture()
    {
        FilteredControllerTypes.Add( typeof( OrdersController ) );
        FilteredControllerTypes.Add( typeof( PeopleController ) );
        FilteredControllerTypes.Add( typeof( People2Controller ) );
        FilteredControllerTypes.Add( typeof( CustomersController ) );
        FilteredControllerTypes.Add( typeof( WeatherForecastsController ) );
    }

    protected override void OnAddApiVersioning( ApiVersioningOptions options ) =>
        options.ReportApiVersions = true;

    protected override void OnEnableOData( ODataApiVersioningOptions options ) =>
        options.AddRouteComponents( "api" )
               .AddRouteComponents( "v{version:apiVersion}" );

    protected override void OnBuildApplication( IApplicationBuilder app ) =>
        app.UseVersionedODataBatching()
           .UseRouting()
           .UseEndpoints( OnConfigureEndpoints );
}