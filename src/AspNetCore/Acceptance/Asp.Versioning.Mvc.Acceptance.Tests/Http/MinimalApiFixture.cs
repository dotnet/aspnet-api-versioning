// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable ASP0018 // Unused route parameter

namespace Asp.Versioning.Http;

using Asp.Versioning.Conventions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

public class MinimalApiFixture : HttpServerFixture
{
    protected override void OnConfigureEndpoints( IEndpointRouteBuilder endpoints )
    {
        endpoints.MapGet( "api/ping", () => Results.NoContent() )
                 .WithApiVersionSet( endpoints.NewApiVersionSet().Build() )
                 .IsApiVersionNeutral();

        var values = endpoints.NewApiVersionSet( "Values" )
                              .HasApiVersion( 1.0 )
                              .HasApiVersion( 2.0 )
                              .ReportApiVersions()
                              .Build();

        endpoints.MapGet( "api/values", () => "Value 1" )
                 .WithApiVersionSet( values )
                 .MapToApiVersion( 1.0 );

        endpoints.MapGet( "api/values", () => "Value 2" )
                 .WithApiVersionSet( values )
                 .MapToApiVersion( 2.0 );

        var orders = endpoints.NewVersionedApi( "Orders" )
                              .MapGroup( "api/order" )
                              .HasApiVersion( 1.0 )
                              .HasApiVersion( 2.0 );

        orders.MapGet( "/", () => Results.Ok() );
        orders.MapGet( "/{id}", ( int id ) => Results.Ok() ).HasDeprecatedApiVersion( 0.9 );
        orders.MapPost( "/", () => Results.Created() );
        orders.MapDelete( "/{id}", ( int id ) => Results.NoContent() ).IsApiVersionNeutral();

        var helloWorld = endpoints.NewVersionedApi( "Orders" )
                                  .MapGroup( "api/v{version:apiVersion}/hello" )
                                  .HasApiVersion( 1.0 )
                                  .HasApiVersion( 2.0 )
                                  .ReportApiVersions();

        helloWorld.MapGet( "/", () => "Hello world!" ).MapToApiVersion( 1.0 );
        helloWorld.MapGet( "/{text}", ( string text ) => text ).MapToApiVersion( 1.0 );
        helloWorld.MapGet( "/", () => "Hello world! (v2)" ).MapToApiVersion( 2.0 );
        helloWorld.MapGet( "/{text}", ( string text ) => text + " (v2)" ).MapToApiVersion( 2.0 );
        helloWorld.MapPost( "/", () => { } );
    }

    protected override void OnAddApiVersioning( ApiVersioningOptions options )
    {
        options.ApiVersionReader = ApiVersionReader.Combine(
            new QueryStringApiVersionReader(),
            new UrlSegmentApiVersionReader(),
            new MediaTypeApiVersionReader() );
    }
}