// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Http;

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
                 .WithApiVersionSet( endpoints.NewApiVersionSet().Build() )
                 .IsApiVersionNeutral();

        endpoints.MapGet( "api/values", () => "Value 1" )
                 .WithApiVersionSet( values )
                 .MapToApiVersion( 1.0 );

        endpoints.MapGet( "api/values", () => "Value 2" )
                 .WithApiVersionSet( values )
                 .MapToApiVersion( 2.0 );

        endpoints.MapGet( "api/v{version:apiVersion}/hello", () => "Hello world!" )
                 .WithApiVersionSet( helloWorld )
                 .MapToApiVersion( 1.0 );

        endpoints.MapGet( "api/v{version:apiVersion}/hello/{text}", ( string text ) => text )
                 .WithApiVersionSet( helloWorld )
                 .MapToApiVersion( 1.0 );

        endpoints.MapGet( "api/v{version:apiVersion}/hello", () => "Hello world! (v2)" )
                 .WithApiVersionSet( helloWorld )
                 .MapToApiVersion( 2.0 );

        endpoints.MapGet( "api/v{version:apiVersion}/hello/{text}", ( string text ) => text + " (v2)" )
                 .WithApiVersionSet( helloWorld )
                 .MapToApiVersion( 2.0 );

        endpoints.MapPost( "api/v{version:apiVersion}/hello", () => { } )
                 .WithApiVersionSet( helloWorld );

        endpoints.MapGet( "api/order", () => { } )
                 .WithApiVersionSet( orders )
                 .HasApiVersion( 1.0 )
                 .HasApiVersion( 2.0 );

        endpoints.MapGet( "api/order/{id}", ( int id ) => { } )
                 .WithApiVersionSet( orders )
                 .HasDeprecatedApiVersion( 0.9 )
                 .HasApiVersion( 1.0 )
                 .HasApiVersion( 2.0 );

        endpoints.MapPost( "api/order", () => { } )
                 .WithApiVersionSet( orders )
                 .HasApiVersion( 1.0 )
                 .HasApiVersion( 2.0 );

        endpoints.MapDelete( "api/order/{id}", ( int id ) => { } )
                 .WithApiVersionSet( orders )
                 .IsApiVersionNeutral();
    }
}