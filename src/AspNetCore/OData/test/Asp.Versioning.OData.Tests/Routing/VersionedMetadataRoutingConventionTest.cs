// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Routing;

using Asp.Versioning.Controllers;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.Routing.Conventions;
using Microsoft.OData.Edm;
using System.Reflection;

public class VersionedMetadataRoutingConventionTest
{
    [Theory]
    [InlineData( typeof( VersionedMetadataController ), true )]
    [InlineData( typeof( AnotherVersionedMetadataController ), true )]
    [InlineData( typeof( MetadataController ), false )]
    public void applies_to_controller_should_return_expected_result( Type controllerType, bool expected )
    {
        // arrange
        var context = new ODataControllerActionContext(
            string.Empty,
            EdmCoreModel.Instance,
            new ControllerModel( controllerType.GetTypeInfo(), [] ) );
        var convention = new VersionedMetadataRoutingConvention();

        // act
        var result = convention.AppliesToController( context );

        // assert
        result.Should().Be( expected );
    }

    [Fact]
    public void applied_to_action_should_return_true()
    {
        // arrange
        var controller = new ControllerModel( typeof( VersionedMetadataController ).GetTypeInfo(), [] );
        var method = controller.ControllerType.GetRuntimeMethod( nameof( VersionedMetadataController.GetOptions ), Type.EmptyTypes );
        var action = new ActionModel( method, [] ) { Controller = controller };

        controller.Actions.Add( action );

        var context = new ODataControllerActionContext( string.Empty, EdmCoreModel.Instance, controller ) { Action = action };
        var convention = new VersionedMetadataRoutingConvention();

        // act
        var result = convention.AppliesToAction( context );

        // assert
        result.Should().BeTrue();
        action.Selectors.Should().HaveCount( 1 );
    }

#pragma warning disable IDE0079
#pragma warning disable CA1812

    private sealed class AnotherVersionedMetadataController : VersionedMetadataController
    {
    }
}