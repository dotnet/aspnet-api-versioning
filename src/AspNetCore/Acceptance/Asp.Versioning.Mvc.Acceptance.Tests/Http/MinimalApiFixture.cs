// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using Asp.Versioning.Conventions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

public class MinimalApiFixture : HttpServerFixture
{
    protected override void OnConfigureEndpoints( IEndpointRouteBuilder endpoints )
    {
        var values = endpoints.NewApiVersionSet( "Values" )
                              .HasApiVersion( 1.0 )
                              .HasApiVersion( 2.0 )
                              .ReportApiVersions()
                              .Build();

        var helloWorld = endpoints.NewApiVersionSet( "Hello World" )
                                  .HasApiVersion( 1.0 )
                                  .HasApiVersion( 2.0 )
                                  .ReportApiVersions()
                                  .Build();

        var orders = endpoints.NewApiVersionSet( "Orders" ).Build();

        endpoints.MapGet( "api/ping", () => Results.NoContent() )
                 .UseApiVersioning( endpoints.NewApiVersionSet().Build() )
                 .IsApiVersionNeutral();

        endpoints.MapGet( "api/values", () => "Value 1" )
                 .UseApiVersioning( values )
                 .MapToApiVersion( 1.0 );

        endpoints.MapGet( "api/values", () => "Value 2" )
                 .UseApiVersioning( values )
                 .MapToApiVersion( 2.0 );

        endpoints.MapGet( "api/v{version:apiVersion}/hello", () => "Hello world!" )
                 .UseApiVersioning( helloWorld )
                 .MapToApiVersion( 1.0 );

        endpoints.MapGet( "api/v{version:apiVersion}/hello/{text}", ( string text ) => text )
                 .UseApiVersioning( helloWorld )
                 .MapToApiVersion( 1.0 );

        endpoints.MapGet( "api/v{version:apiVersion}/hello", () => "Hello world! (v2)" )
                 .UseApiVersioning( helloWorld )
                 .MapToApiVersion( 2.0 );

        endpoints.MapGet( "api/v{version:apiVersion}/hello/{text}", ( string text ) => text + " (v2)" )
                 .UseApiVersioning( helloWorld )
                 .MapToApiVersion( 2.0 );

        endpoints.MapPost( "api/v{version:apiVersion}/hello", () => { } )
                 .UseApiVersioning( helloWorld );

        endpoints.MapGet( "api/order", () => { } )
                 .UseApiVersioning( orders )
                 .HasApiVersion( 1.0 )
                 .HasApiVersion( 2.0 );

        endpoints.MapGet( "api/order/{id}", ( int id ) => { } )
                 .UseApiVersioning( orders )
                 .HasDeprecatedApiVersion( 0.9 )
                 .HasApiVersion( 1.0 )
                 .HasApiVersion( 2.0 );

        endpoints.MapPost( "api/order", () => { } )
                 .UseApiVersioning( orders )
                 .HasApiVersion( 1.0 )
                 .HasApiVersion( 2.0 );

        endpoints.MapDelete( "api/order/{id}", ( int id ) => { } )
                 .UseApiVersioning( orders )
                 .IsApiVersionNeutral();
    }
}