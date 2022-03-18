// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OData.Advanced;

using Asp.Versioning.OData.Advanced.Controllers;
using Asp.Versioning.OData.Configuration;

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

    protected override void OnEnableOData( ODataApiVersioningOptions options )
    {
        var builder = options.ModelBuilder;
        var configurations = builder.ModelConfigurations;

        configurations.Clear();
        configurations.Add( new PersonModelConfiguration() );
        configurations.Add( new OrderModelConfiguration( supportedApiVersion: new ApiVersion( 2, 0 ) ) );

        options.AddRouteComponents( "api" );
    }
}