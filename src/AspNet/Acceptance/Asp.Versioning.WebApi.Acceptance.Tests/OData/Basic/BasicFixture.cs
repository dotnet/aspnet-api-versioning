// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OData.Basic;

using Asp.Versioning;
using Asp.Versioning.OData;
using Asp.Versioning.OData.Basic.Controllers;
using Asp.Versioning.OData.Configuration;
using System.Web.Http;

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

    protected override void OnConfigure( HttpConfiguration configuration )
    {
        var modelBuilder = new VersionedODataModelBuilder( configuration )
        {
            ModelConfigurations =
            {
                new PersonModelConfiguration(),
                new OrderModelConfiguration(),
                new CustomerModelConfiguration(),
                new WeatherForecastModelConfiguration(),
            },
        };
        var models = modelBuilder.GetEdmModels();

        configuration.MapVersionedODataRoute( "odata", "api", models );
        configuration.MapVersionedODataRoute( "odata-bypath", "v{apiVersion}", models );
    }
}