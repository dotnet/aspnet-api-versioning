// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.ApiExplorer;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

internal sealed class TestEndpointDataSource : EndpointDataSource
{
    public override IReadOnlyList<Endpoint> Endpoints { get; } = CreateEndpoints();

    public override IChangeToken GetChangeToken() => NullChangeToken.Singleton;

    private static IReadOnlyList<Endpoint> CreateEndpoints()
    {
        var endpoints = new List<Endpoint>();

        AddOrderEndpoints( endpoints );
        AddPeopleEndpoints( endpoints );

        return endpoints;
    }

    private static void AddOrderEndpoints( ICollection<Endpoint> endpoints )
    {
        // api version 0.9 and 1.0
        endpoints.Add(
            NewEndpoint(
                "GET-orders/{id}",
                "orders/{id}",
                declared: new ApiVersion[] { new( 0, 9 ), new( 1, 0 ) },
                supported: new ApiVersion[] { new( 1, 0 ) },
                deprecated: new ApiVersion[] { new( 0, 9 ) } ) );

        endpoints.Add(
            NewEndpoint(
                "POST-orders",
                "orders",
                declared: new ApiVersion[] { new( 1, 0 ) },
                supported: new ApiVersion[] { new( 1, 0 ) } ) );

        // api version 2.0
        endpoints.Add(
            NewEndpoint(
                "GET-orders",
                "orders",
                declared: new ApiVersion[] { new( 2, 0 ) },
                supported: new ApiVersion[] { new( 2, 0 ) } ) );

        endpoints.Add(
            NewEndpoint(
                "GET-orders/{id}",
                "orders/{id}",
                declared: new ApiVersion[] { new( 2, 0 ) },
                supported: new ApiVersion[] { new( 2, 0 ) } ) );

        endpoints.Add(
            NewEndpoint(
                "POST-orders",
                "orders",
                declared: new ApiVersion[] { new( 2, 0 ) },
                supported: new ApiVersion[] { new( 2, 0 ) } ) );

        // api version 3.0
        endpoints.Add(
            NewEndpoint(
                "GET-orders",
                "orders",
                declared: new ApiVersion[] { new( 3, 0 ) },
                supported: new ApiVersion[] { new( 3, 0 ) },
                advertised: new ApiVersion[] { new( 4, 0 ) } ) );

        endpoints.Add(
            NewEndpoint(
                "GET-orders/{id}",
                "orders/{id}",
                declared: new ApiVersion[] { new( 3, 0 ) },
                supported: new ApiVersion[] { new( 3, 0 ) },
                advertised: new ApiVersion[] { new( 4, 0 ) } ) );

        endpoints.Add(
            NewEndpoint(
                "POST-orders",
                "orders",
                declared: new ApiVersion[] { new( 3, 0 ) },
                supported: new ApiVersion[] { new( 3, 0 ) },
                advertised: new ApiVersion[] { new( 4, 0 ) } ) );

        endpoints.Add(
            NewEndpoint(
                "DELETE-orders/{id}",
                "orders/{id}",
                declared: new ApiVersion[] { new( 3, 0 ) },
                supported: new ApiVersion[] { new( 3, 0 ) },
                advertised: new ApiVersion[] { new( 4, 0 ) } ) );
    }

    private static void AddPeopleEndpoints( ICollection<Endpoint> endpoints )
    {
        // api version 0.9 and 1.0
        endpoints.Add(
            NewEndpoint(
                "GET-people/{id}",
                "people/{id}",
                declared: new ApiVersion[] { new( 0, 9 ), new( 1, 0 ) },
                supported: new ApiVersion[] { new( 1, 0 ) },
                deprecated: new ApiVersion[] { new( 0, 9 ) } ) );

        endpoints.Add(
            NewEndpoint(
                "POST-people",
                "people",
                declared: new ApiVersion[] { new( 1, 0 ) },
                supported: new ApiVersion[] { new( 1, 0 ) } ) );

        // api version 2.0
        endpoints.Add(
            NewEndpoint(
                "GET-people",
                "people",
                declared: new ApiVersion[] { new( 2, 0 ) },
                supported: new ApiVersion[] { new( 2, 0 ) } ) );

        endpoints.Add(
            NewEndpoint(
                "GET-people/{id}",
                "people/{id}",
                declared: new ApiVersion[] { new( 2, 0 ) },
                supported: new ApiVersion[] { new( 2, 0 ) } ) );

        endpoints.Add(
            NewEndpoint(
                "POST-people",
                "people",
                declared: new ApiVersion[] { new( 2, 0 ) },
                supported: new ApiVersion[] { new( 2, 0 ) } ) );

        // api version 3.0
        endpoints.Add(
            NewEndpoint(
                "GET-people",
                "people",
                declared: new ApiVersion[] { new( 3, 0 ) },
                supported: new ApiVersion[] { new( 3, 0 ) },
                advertised: new ApiVersion[] { new( 4, 0 ) } ) );

        endpoints.Add(
            NewEndpoint(
                "GET-people/{id}",
                "people/{id}",
                declared: new ApiVersion[] { new( 3, 0 ) },
                supported: new ApiVersion[] { new( 3, 0 ) },
                advertised: new ApiVersion[] { new( 4, 0 ) } ) );

        endpoints.Add(
            NewEndpoint(
                "POST-people",
                "people",
                declared: new ApiVersion[] { new( 3, 0 ) },
                supported: new ApiVersion[] { new( 3, 0 ) },
                advertised: new ApiVersion[] { new( 4, 0 ) } ) );
    }

    private static Endpoint NewEndpoint(
        string displayName,
        string pattern,
        IEnumerable<ApiVersion> declared,
        IEnumerable<ApiVersion> supported,
        IEnumerable<ApiVersion> deprecated = null,
        IEnumerable<ApiVersion> advertised = null,
        IEnumerable<ApiVersion> advertisedDeprecated = null )
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