// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using Asp.Versioning.Conventions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

public class MinimalApiFixture : HttpServerFixture
{
    protected override void OnConfigureEndpoints( IEndpointRouteBuilder endpoints )
    {
        endpoints.DefineApi()
                 .IsApiVersionNeutral()
                 .HasMapping( api => api.MapGet( "api/ping", () => { } ) );

        endpoints.DefineApi( "Values" )
                 .HasApiVersion( 1.0 )
                 .HasApiVersion( 2.0 )
                 .ReportApiVersions()
                 .HasMapping(
                    api =>
                    {
                        api.MapGet( "api/values", () => "Value 1" ).MapToApiVersion( 1.0 );
                        api.MapGet( "api/values", () => "Value 2" ).MapToApiVersion( 2.0 );
                    } );

        endpoints.DefineApi( "Hello World" )
                 .HasApiVersion( 1.0 )
                 .HasApiVersion( 2.0 )
                 .ReportApiVersions()
                 .HasMapping(
                     api =>
                     {
                         api.MapGet( "api/v{version:apiVersion}/hello", () => "Hello world!" ).MapToApiVersion( 1.0 );
                         api.MapGet( "api/v{version:apiVersion}/hello/{text}", ( string text ) => text ).MapToApiVersion( 1.0 );

                         api.MapGet( "api/v{version:apiVersion}/hello", () => "Hello world! (v2)" ).MapToApiVersion( 2.0 );
                         api.MapGet( "api/v{version:apiVersion}/hello/{text}", ( string text ) => text + " (v2)" ).MapToApiVersion( 2.0 );

                         api.MapPost( "api/v{version:apiVersion}/hello", () => { } );
                     } );

        endpoints.DefineApi( "Orders" )
                 .HasMapping(
                    api =>
                    {
                        api.MapGet( "api/order", () => { } ).HasApiVersion( 1.0 ).HasApiVersion( 2.0 );
                        api.MapGet( "api/order/{id}", ( int id ) => { } ).HasDeprecatedApiVersion( 0.9 ).HasApiVersion( 1.0 ).HasApiVersion( 2.0 );
                        api.MapPost( "api/order", () => { } ).HasApiVersion( 1.0 ).HasApiVersion( 2.0 );
                        api.MapDelete( "api/order/{id}", ( int id ) => { } ).IsApiVersionNeutral();
                    } );
    }
}