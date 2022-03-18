// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Mvc.UsingConventions;

using Asp.Versioning;
using Asp.Versioning.Conventions;
using Asp.Versioning.Mvc.UsingConventions.Controllers;
using Microsoft.AspNetCore.Mvc;

public class ConventionsFixture : HttpServerFixture
{
    public ConventionsFixture()
    {
        FilteredControllerTypes.Add( typeof( ValuesController ) );
        FilteredControllerTypes.Add( typeof( Values2Controller ) );
        FilteredControllerTypes.Add( typeof( HelloWorldController ) );
        FilteredControllerTypes.Add( typeof( HelloWorld2Controller ) );
        FilteredControllerTypes.Add( typeof( OrdersController ) );
    }

    protected override void OnAddApiVersioning( ApiVersioningOptions options ) =>
        options.ReportApiVersions = true;

    protected override void OnAddMvcApiVersioning( MvcApiVersioningOptions options )
    {
        options.Conventions.Controller<ValuesController>().HasApiVersion( 1.0 );

        options.Conventions.Controller<Values2Controller>()
                           .HasApiVersion( 2.0 )
                           .HasApiVersion( 3.0 )
                           .Action( c => c.GetV3( default ) ).MapToApiVersion( 3.0 )
                           .Action( c => c.GetV3( default, default ) ).MapToApiVersion( 3.0 );

        options.Conventions.Controller<HelloWorldController>().HasDeprecatedApiVersion( 1.0 );

        options.Conventions.Controller<HelloWorld2Controller>()
                           .HasApiVersion( 2.0 )
                           .HasApiVersion( 3.0 )
                           .AdvertisesApiVersion( 4.0 )
                           .Action( c => c.GetV3() ).MapToApiVersion( 3.0 )
                           .Action( c => c.GetV3( default ) ).MapToApiVersion( 3.0 );

        options.Conventions.Controller<OrdersController>()
                           .Action( c => c.Get() ).HasApiVersion( 1.0 ).HasApiVersion( 2.0 )
                           .Action( c => c.Get( default ) ).HasApiVersion( 0.9 ).HasApiVersion( 1.0 ).HasApiVersion( 2.0 )
                           .Action( c => c.Post( default ) ).HasApiVersion( 1.0 ).HasApiVersion( 2.0 )
                           .Action( c => c.Put( default, default ) ).HasApiVersion( 2.0 )
                           .Action( c => c.Delete( default ) ).IsApiVersionNeutral();
    }
}