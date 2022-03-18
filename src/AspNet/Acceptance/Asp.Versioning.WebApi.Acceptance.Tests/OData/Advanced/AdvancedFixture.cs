// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OData.Advanced;

using Asp.Versioning.OData;
using Asp.Versioning.OData.Advanced.Controllers;
using Asp.Versioning.OData.Configuration;
using System.Web.Http;
using static System.Web.Http.RouteParameter;

public class AdvancedFixture : ODataFixture
{
    public AdvancedFixture()
    {
        FilteredControllerTypes.Add( typeof( OrdersController ) );
        FilteredControllerTypes.Add( typeof( Orders2Controller ) );
        FilteredControllerTypes.Add( typeof( Orders3Controller ) );
        FilteredControllerTypes.Add( typeof( PeopleController ) );
        FilteredControllerTypes.Add( typeof( People2Controller ) );
    }

    protected override void OnAddApiVersioning( ApiVersioningOptions options )
    {
        options.ReportApiVersions = true;
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ApiVersionReader = ApiVersionReader.Combine(
            new QueryStringApiVersionReader(),
            new HeaderApiVersionReader( "api-version", "x-ms-version" ) );
    }

    protected override void OnConfigure( HttpConfiguration configuration )
    {
        var modelBuilder = new VersionedODataModelBuilder( configuration )
        {
            ModelConfigurations =
            {
                new PersonModelConfiguration(),
                new OrderModelConfiguration( supportedApiVersion: new ApiVersion( 2, 0 ) ),
            },
        };
        var models = modelBuilder.GetEdmModels();

        configuration.MapVersionedODataRoute( "odata", "api", models );
        configuration.Routes.MapHttpRoute( "orders", "api/{controller}/{key}", new { key = Optional } );
        configuration.Formatters.Remove( configuration.Formatters.XmlFormatter );
    }
}