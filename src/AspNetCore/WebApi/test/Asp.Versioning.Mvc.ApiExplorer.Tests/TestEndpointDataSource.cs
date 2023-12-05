// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.ApiExplorer;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

internal sealed class TestEndpointDataSource : EndpointDataSource
{
    public override List<Endpoint> Endpoints { get; } = CreateEndpoints();

    public override IChangeToken GetChangeToken() => NullChangeToken.Singleton;

    private static List<Endpoint> CreateEndpoints()
    {
        var endpoints = new List<Endpoint>();

        AddOrderEndpoints( endpoints );
        AddPeopleEndpoints( endpoints );

        return endpoints;
    }

    private static void AddOrderEndpoints( List<Endpoint> endpoints )
    {
        // api version 0.9 and 1.0
        endpoints.Add(
            NewEndpoint(
                "GET-orders/{id}",
                "orders/{id}",
                declared: [new( 0, 9 ), new( 1, 0 )],
                supported: [new( 1, 0 )],
                deprecated: [new( 0, 9 )] ) );

        endpoints.Add(
            NewEndpoint(
                "POST-orders",
                "orders",
                declared: [new( 1, 0 )],
                supported: [new( 1, 0 )] ) );

        // api version 2.0
        endpoints.Add(
            NewEndpoint(
                "GET-orders",
                "orders",
                declared: [new( 2, 0 )],
                supported: [new( 2, 0 )] ) );

        endpoints.Add(
            NewEndpoint(
                "GET-orders/{id}",
                "orders/{id}",
                declared: [new( 2, 0 )],
                supported: [new( 2, 0 )] ) );

        endpoints.Add(
            NewEndpoint(
                "POST-orders",
                "orders",
                declared: [new( 2, 0 )],
                supported: [new( 2, 0 )] ) );

        // api version 3.0
        endpoints.Add(
            NewEndpoint(
                "GET-orders",
                "orders",
                declared: [new( 3, 0 )],
                supported: [new( 3, 0 )],
                advertised: [new( 4, 0 )] ) );

        endpoints.Add(
            NewEndpoint(
                "GET-orders/{id}",
                "orders/{id}",
                declared: [new( 3, 0 )],
                supported: [new( 3, 0 )],
                advertised: [new( 4, 0 )] ) );

        endpoints.Add(
            NewEndpoint(
                "POST-orders",
                "orders",
                declared: [new( 3, 0 )],
                supported: [new( 3, 0 )],
                advertised: [new( 4, 0 )] ) );

        endpoints.Add(
            NewEndpoint(
                "DELETE-orders/{id}",
                "orders/{id}",
                declared: [new( 3, 0 )],
                supported: [new( 3, 0 )],
                advertised: [new( 4, 0 )] ) );
    }

    private static void AddPeopleEndpoints( List<Endpoint> endpoints )
    {
        // api version 0.9 and 1.0
        endpoints.Add(
            NewEndpoint(
                "GET-people/{id}",
                "people/{id}",
                declared: [new( 0, 9 ), new( 1, 0 )],
                supported: [new( 1, 0 )],
                deprecated: [new( 0, 9 )] ) );

        endpoints.Add(
            NewEndpoint(
                "POST-people",
                "people",
                declared: [new( 1, 0 )],
                supported: [new( 1, 0 )] ) );

        // api version 2.0
        endpoints.Add(
            NewEndpoint(
                "GET-people",
                "people",
                declared: [new( 2, 0 )],
                supported: [new( 2, 0 )] ) );

        endpoints.Add(
            NewEndpoint(
                "GET-people/{id}",
                "people/{id}",
                declared: [new( 2, 0 )],
                supported: [new( 2, 0 )] ) );

        endpoints.Add(
            NewEndpoint(
                "POST-people",
                "people",
                declared: [new( 2, 0 )],
                supported: [new( 2, 0 )] ) );

        // api version 3.0
        endpoints.Add(
            NewEndpoint(
                "GET-people",
                "people",
                declared: [new( 3, 0 )],
                supported: [new( 3, 0 )],
                advertised: [new( 4, 0 )] ) );

        endpoints.Add(
            NewEndpoint(
                "GET-people/{id}",
                "people/{id}",
                declared: [new( 3, 0 )],
                supported: [new( 3, 0 )],
                advertised: [new( 4, 0 )] ) );

        endpoints.Add(
            NewEndpoint(
                "POST-people",
                "people",
                declared: [new( 3, 0 )],
                supported: [new( 3, 0 )],
                advertised: [new( 4, 0 )] ) );
    }

    private static Endpoint NewEndpoint(
        string displayName,
        string pattern,
        ApiVersion[] declared,
        ApiVersion[] supported,
        ApiVersion[] deprecated = null,
        ApiVersion[] advertised = null,
        ApiVersion[] advertisedDeprecated = null )
    {
        var metadata = new ApiVersionMetadata(
            ApiVersionModel.Empty,
            new ApiVersionModel(
                declared,
                supported,
                deprecated ?? Enumerable.Empty<ApiVersion>(),
                advertised ?? Enumerable.Empty<ApiVersion>(),
                advertisedDeprecated ?? Enumerable.Empty<ApiVersion>() ) );
        var builder = new RouteEndpointBuilder(
            Route404,
            RoutePatternFactory.Parse( pattern ),
            default )
        {
            DisplayName = displayName,
            Metadata = { metadata },
        };

        return builder.Build();
    }

    private static Task Route404( HttpContext context ) => Task.CompletedTask;
}