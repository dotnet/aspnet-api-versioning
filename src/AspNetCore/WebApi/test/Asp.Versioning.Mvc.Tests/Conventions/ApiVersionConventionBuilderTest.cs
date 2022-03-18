// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Conventions;

using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System.Reflection;
using static Asp.Versioning.ApiVersionMapping;

public partial class ApiVersionConventionBuilderTest
{
    [Fact]
    public void apply_should_apply_configured_conventions()
    {
        // arrange
        var controllerType = typeof( v2.UndecoratedController ).GetTypeInfo();
        var action = controllerType.GetRuntimeMethod( nameof( v2.UndecoratedController.Get ), Type.EmptyTypes );
        var attributes = Array.Empty<object>();
        var actionModel = new ActionModel( action, attributes );
        var controllerModel = new ControllerModel( controllerType, attributes ) { Actions = { actionModel } };
        var conventionBuilder = new ApiVersionConventionBuilder();
        var actionDescriptor = new ActionDescriptor();

        actionModel.Controller = controllerModel;
        conventionBuilder.Add( new VersionByNamespaceConvention() );

        // act
        conventionBuilder.ApplyTo( controllerModel );

        // assert
        var metadata = actionModel.Selectors.Single().EndpointMetadata.OfType<ApiVersionMetadata>().Single();

        metadata.MappingTo( new ApiVersion( 2, 0 ) ).Should().Be( Implicit );
    }
}