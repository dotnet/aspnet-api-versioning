// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OData.UsingConventions;

using Asp.Versioning;
using Asp.Versioning.Conventions;
using Asp.Versioning.OData;
using Asp.Versioning.OData.Configuration;
using Asp.Versioning.OData.UsingConventions.Controllers;
using System.Web.Http;

public class ConventionsFixture : ODataFixture
{
    public ConventionsFixture()
    {
        FilteredControllerTypes.Add( typeof( OrdersController ) );
        FilteredControllerTypes.Add( typeof( PeopleController ) );
        FilteredControllerTypes.Add( typeof( People2Controller ) );
        FilteredControllerTypes.Add( typeof( CustomersController ) );
    }

    protected override void OnAddApiVersioning( ApiVersioningOptions options )
    {
        options.ReportApiVersions = true;
        options.Conventions.Controller<OrdersController>()
                           .HasApiVersion( 1, 0 );
        options.Conventions.Controller<PeopleController>()
                           .HasApiVersion( 1, 0 )
                           .HasApiVersion( 2, 0 )
                           .Action( c => c.Patch( default, null, null ) ).MapToApiVersion( 2, 0 );
        options.Conventions.Controller<People2Controller>()
                           .HasApiVersion( 3, 0 );
        options.Conventions.Controller<CustomersController>()
                           .Action( c => c.Get() ).HasApiVersion( 2, 0 ).HasApiVersion( 3, 0 )
                           .Action( c => c.Get( default ) ).HasApiVersion( 1, 0 ).HasApiVersion( 2, 0 ).HasApiVersion( 3, 0 )
                           .Action( c => c.Post( default ) ).HasApiVersion( 1, 0 ).HasApiVersion( 2, 0 ).HasApiVersion( 3, 0 )
                           .Action( c => c.Put( default, default ) ).HasApiVersion( 3, 0 )
                           .Action( c => c.Delete( default ) ).IsApiVersionNeutral();
    }

    protected override void OnConfigure( HttpConfiguration configuration )
    {
        var modelBuilder = new VersionedODataModelBuilder( configuration )
        {
            ModelConfigurations =
            {
                new PersonModelConfiguration(),
                new OrderModelConfiguration(),
                new CustomerModelConfiguration(),
            },
        };
        var models = modelBuilder.GetEdmModels();

        configuration.MapVersionedODataRoute( "odata", "api", models );
        configuration.MapVersionedODataRoute( "odata-bypath", "v{apiVersion}", models );
    }
}