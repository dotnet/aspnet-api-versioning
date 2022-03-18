// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OData.UsingConventions;

using Asp.Versioning;
using Asp.Versioning.Conventions;
using Asp.Versioning.OData.UsingConventions.Controllers;
using Microsoft.AspNetCore.Mvc;

public class ConventionsFixture : ODataFixture
{
    public ConventionsFixture()
    {
        FilteredControllerTypes.Add( typeof( OrdersController ) );
        FilteredControllerTypes.Add( typeof( PeopleController ) );
        FilteredControllerTypes.Add( typeof( People2Controller ) );
        FilteredControllerTypes.Add( typeof( CustomersController ) );
    }

    protected override void OnAddApiVersioning( ApiVersioningOptions options ) =>
        options.ReportApiVersions = true;

    protected override void OnAddMvcApiVersioning( MvcApiVersioningOptions options )
    {
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

    protected override void OnEnableOData( ODataApiVersioningOptions options ) =>
        options.AddRouteComponents( "api" ).AddRouteComponents( "v{version:apiVersion}" );
}