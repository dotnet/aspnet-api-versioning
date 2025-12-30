// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using Asp.Versioning.Conventions;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using static Asp.Versioning.ApiVersionMapping;

public class ApiVersionCollatorTest
{
    [Theory]
    [MemberData( nameof( ActionDescriptorProviderContexts ) )]
    public void on_providers_executed_should_aggregate_api_version_models_by_controller( ContextKind kind )
    {
        // arrange
        var context = NewContext( kind );
        var collator = new ApiVersionCollator( ControllerNameConvention.Default );
        var expected = new ApiVersion[] { new( 1, 0 ), new( 2, 0 ), new( 3, 0 ) };

        // act
        collator.OnProvidersExecuted( context );

        // assert
        var actions = context.Results.Where( a => a.GetProperty<ApiVersionModel>() != null );

        actions.All( a => a.GetApiVersionMetadata().Map( Explicit ).SupportedApiVersions.SequenceEqual( expected ) ).Should().BeTrue();
    }

    public enum ContextKind
    {
        /// <summary>
        /// Gets an action context with route values.
        /// </summary>
        WithRouteValues,

        /// <summary>
        /// Gets an action context by controller name.
        /// </summary>
        ByControllerName,
    }

    private static ActionDescriptorProviderContext NewContext( ContextKind kind ) => kind switch
    {
        ContextKind.WithRouteValues => ActionsWithRouteValues,
        ContextKind.ByControllerName => ActionsByControllerName,
        _ => throw new ArgumentOutOfRangeException( nameof( kind ) ),
    };

    public static TheoryData<ContextKind> ActionDescriptorProviderContexts =>
        new( ContextKind.WithRouteValues, ContextKind.ByControllerName );

    private static ApiVersionMetadata NewApiVersionMetadata( double version )
    {
        var model = new ApiVersionModel( new ApiVersion( version ) );
        return new( model, model );
    }

    private static ActionDescriptorProviderContext ActionsWithRouteValues =>
        new()
        {
            Results =
            {
                new()
                {
                    RouteValues = new Dictionary<string, string>()
                    {
                        ["controller"] = "Values",
                        ["action"] = "Get",
                    },
                    EndpointMetadata = [NewApiVersionMetadata( 1.0 )],
                },
                new()
                {
                    RouteValues = new Dictionary<string, string>()
                    {
                        ["page"] = "/Some/Page",
                    },
                },
                new()
                {
                    RouteValues = new Dictionary<string, string>()
                    {
                        ["controller"] = "Values",
                        ["action"] = "Get",
                    },
                    EndpointMetadata = [NewApiVersionMetadata( 2.0 )],
                },
                new()
                {
                    RouteValues = new Dictionary<string, string>()
                    {
                        ["controller"] = "Values",
                        ["action"] = "Get",
                    },
                    EndpointMetadata = [NewApiVersionMetadata( 3.0 )],
                },
            },
        };

    private static ActionDescriptorProviderContext ActionsByControllerName =>
        new()
        {
            Results =
            {
                new ControllerActionDescriptor()
                {
                    ControllerName = "Values",
                    RouteValues = new Dictionary<string, string>()
                    {
                        ["action"] = "Get",
                    },
                    EndpointMetadata = [NewApiVersionMetadata( 1.0 )],
                },
                new ActionDescriptor()
                {
                    RouteValues = new Dictionary<string, string>()
                    {
                        ["page"] = "/Some/Page",
                    },
                },
                new ControllerActionDescriptor()
                {
                    ControllerName = "Values",
                    RouteValues = new Dictionary<string, string>()
                    {
                        ["action"] = "Get",
                    },
                    EndpointMetadata = [NewApiVersionMetadata( 2.0 )],
                },
                new ControllerActionDescriptor()
                {
                    ControllerName = "Values",
                    RouteValues = new Dictionary<string, string>()
                    {
                        ["action"] = "Get",
                    },
                    EndpointMetadata = [NewApiVersionMetadata( 3.0 )],
                },
            },
        };
}