// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.ApiExplorer;

using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;

internal sealed class TestActionDescriptorCollectionProvider : IActionDescriptorCollectionProvider
{
    private readonly Lazy<ActionDescriptorCollection> collection;

    public TestActionDescriptorCollectionProvider() => collection = new( CreateActionDescriptors );

    public TestActionDescriptorCollectionProvider( ActionDescriptor action, params ActionDescriptor[] otherActions )
    {
        ActionDescriptor[] actions;

        if ( otherActions.Length == 0 )
        {
            actions = [action];
        }
        else
        {
            actions = new ActionDescriptor[otherActions.Length + 1];
            actions[0] = action;
            Array.Copy( otherActions, 0, actions, 1, otherActions.Length );
        }

        collection = new( () => new( actions, 0 ) );
    }

    public ActionDescriptorCollection ActionDescriptors => collection.Value;

    private static ActionDescriptorCollection CreateActionDescriptors()
    {
        var actions = new List<ActionDescriptor>();

        AddOrderActionDescriptors( actions );
        AddPeopleActionDescriptors( actions );

        return new( [.. actions], 0 );
    }

    private static void AddOrderActionDescriptors( List<ActionDescriptor> actions )
    {
        // api version 0.9 and 1.0
        actions.Add(
            NewActionDescriptor(
                "GET-orders/{id}",
                declared: [new( 0, 9 ), new( 1, 0 )],
                supported: [new( 1, 0 )],
                deprecated: [new( 0, 9 )] ) );

        actions.Add(
            NewActionDescriptor(
                "POST-orders",
                declared: [new( 1, 0 )],
                supported: [new( 1, 0 )] ) );

        // api version 2.0
        actions.Add(
            NewActionDescriptor(
                "GET-orders",
                declared: [new( 2, 0 )],
                supported: [new( 2, 0 )] ) );

        actions.Add(
            NewActionDescriptor(
                "GET-orders/{id}",
                declared: [new( 2, 0 )],
                supported: [new( 2, 0 )] ) );

        actions.Add(
            NewActionDescriptor(
                "POST-orders",
                declared: [new( 2, 0 )],
                supported: [new( 2, 0 )] ) );

        // api version 3.0
        actions.Add(
            NewActionDescriptor(
                "GET-orders",
                declared: [new( 3, 0 )],
                supported: [new( 3, 0 )],
                advertised: [new( 4, 0 )] ) );

        actions.Add(
            NewActionDescriptor(
                "GET-orders/{id}",
                declared: [new( 3, 0 )],
                supported: [new( 3, 0 )],
                advertised: [new( 4, 0 )] ) );

        actions.Add(
            NewActionDescriptor(
                "POST-orders",
                declared: [new( 3, 0 )],
                supported: [new( 3, 0 )],
                advertised: [new( 4, 0 )] ) );

        actions.Add(
            NewActionDescriptor(
                "DELETE-orders/{id}",
                declared: [new( 3, 0 )],
                supported: [new( 3, 0 )],
                advertised: [new( 4, 0 )] ) );
    }

    private static void AddPeopleActionDescriptors( List<ActionDescriptor> actions )
    {
        // api version 0.9 and 1.0
        actions.Add(
            NewActionDescriptor(
                "GET-people/{id}",
                declared: [new( 0, 9 ), new( 1, 0 )],
                supported: [new( 1, 0 )],
                deprecated: [new( 0, 9 )] ) );

        actions.Add(
            NewActionDescriptor(
                "POST-people",
                declared: [new( 1, 0 )],
                supported: [new( 1, 0 )] ) );

        // api version 2.0
        actions.Add(
            NewActionDescriptor(
                "GET-people",
                declared: [new( 2, 0 )],
                supported: [new( 2, 0 )] ) );

        actions.Add(
            NewActionDescriptor(
                "GET-people/{id}",
                declared: [new( 2, 0 )],
                supported: [new( 2, 0 )] ) );

        actions.Add(
            NewActionDescriptor(
                "POST-people",
                declared: [new( 2, 0 )],
                supported: [new( 2, 0 )] ) );

        // api version 3.0
        actions.Add(
            NewActionDescriptor(
                "GET-people",
                declared: [new( 3, 0 )],
                supported: [new( 3, 0 )],
                advertised: [new( 4, 0 )] ) );

        actions.Add(
            NewActionDescriptor(
                "GET-people/{id}",
                declared: [new( 3, 0 )],
                supported: [new( 3, 0 )],
                advertised: [new( 4, 0 )] ) );

        actions.Add(
            NewActionDescriptor(
                "POST-people",
                declared: [new( 3, 0 )],
                supported: [new( 3, 0 )],
                advertised: [new( 4, 0 )] ) );
    }

    private static ActionDescriptor NewActionDescriptor(
        string displayName,
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
                deprecated ?? [],
                advertised ?? [],
                advertisedDeprecated ?? [] ) );

        return new()
        {
            DisplayName = displayName,
            EndpointMetadata = [metadata],
        };
    }
}