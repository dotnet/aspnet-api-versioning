// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Conventions;

using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System.Reflection;
using static Asp.Versioning.ApiVersionMapping;

public partial class ControllerApiVersionConventionBuilderTTest
{
    [Fact]
    public void apply_to_should_assign_conventions_to_controller()
    {
        // arrange
        var controllerType = typeof( UndecoratedController );
        var action = controllerType.GetRuntimeMethod( nameof( UndecoratedController.Get ), Type.EmptyTypes );
        var attributes = Array.Empty<object>();
        var actionModel = new ActionModel( action, attributes );
        var controllerModel = new ControllerModel( controllerType.GetTypeInfo(), attributes ) { Actions = { actionModel } };
        var controllerBuilder = new ControllerApiVersionConventionBuilder<UndecoratedController>();

        actionModel.Controller = controllerModel;
        controllerBuilder.HasDeprecatedApiVersion( 0, 9 )
                         .HasApiVersion( 2, 0 )
                         .AdvertisesApiVersion( 3, 0 )
                         .AdvertisesDeprecatedApiVersion( 3, 0, "Beta" );

        // act
        controllerBuilder.ApplyTo( controllerModel );

        // assert
        var metadata = actionModel.Selectors
                                  .Single()
                                  .EndpointMetadata
                                  .OfType<ApiVersionMetadata>()
                                  .Single();

        metadata.Map( Explicit ).Should().BeEquivalentTo(
            new
            {
                IsApiVersionNeutral = false,
                DeclaredApiVersions = Array.Empty<ApiVersion>(),
                SupportedApiVersions = new ApiVersion[] { new( 2, 0 ), new( 3, 0 ) },
                DeprecatedApiVersions = new ApiVersion[] { new( 0, 9 ), new( 3, 0, "Beta" ) },
                ImplementedApiVersions = new ApiVersion[] { new( 0, 9 ), new( 2, 0 ), new( 3, 0 ), new( 3, 0, "Beta" ) },
            } );
    }

    [Fact]
    public void apply_to_should_assign_empty_conventions_to_api_version_neutral_controller()
    {
        // arrange
        var controllerType = typeof( UndecoratedController );
        var action = controllerType.GetRuntimeMethod( nameof( UndecoratedController.Get ), Type.EmptyTypes );
        var attributes = Array.Empty<object>();
        var actionModel = new ActionModel( action, attributes );
        var controllerModel = new ControllerModel( controllerType.GetTypeInfo(), attributes ) { Actions = { actionModel } };
        var controllerBuilder = new ControllerApiVersionConventionBuilder<UndecoratedController>();

        controllerBuilder.HasDeprecatedApiVersion( 0, 9 )
                         .HasApiVersion( 2, 0 )
                         .AdvertisesApiVersion( 3, 0 )
                         .AdvertisesDeprecatedApiVersion( 3, 0, "Beta" )
                         .IsApiVersionNeutral();

        // act
        controllerBuilder.ApplyTo( controllerModel );

        // assert
        var metadata = actionModel.Selectors
                                  .Single()
                                  .EndpointMetadata
                                  .OfType<ApiVersionMetadata>()
                                  .Single();

        metadata.Map( Explicit ).Should().BeEquivalentTo(
            new
            {
                IsApiVersionNeutral = true,
                DeclaredApiVersions = Array.Empty<ApiVersion>(),
                SupportedApiVersions = Array.Empty<ApiVersion>(),
                DeprecatedApiVersions = Array.Empty<ApiVersion>(),
                ImplementedApiVersions = Array.Empty<ApiVersion>(),
            } );
    }

    [Fact]
    public void apply_to_should_assign_model_to_controller_from_conventions_and_attributes()
    {
        // arrange
        var controllerType = typeof( DecoratedController ).GetTypeInfo();
        var action = controllerType.GetRuntimeMethod( nameof( DecoratedController.Get ), Type.EmptyTypes );
        var attributes = controllerType.GetCustomAttributes().Cast<object>().ToArray();
        var actionModel = new ActionModel( action, Array.Empty<object>() );
        var controllerModel = new ControllerModel( controllerType, attributes ) { Actions = { actionModel } };
        var controllerBuilder = new ControllerApiVersionConventionBuilder<DecoratedController>();

        actionModel.Controller = controllerModel;
        controllerBuilder.HasApiVersion( 1, 0 )
                         .AdvertisesApiVersion( 4, 0 );

        // act
        controllerBuilder.ApplyTo( controllerModel );

        // assert
        var metadata = actionModel.Selectors
                                  .Single()
                                  .EndpointMetadata
                                  .OfType<ApiVersionMetadata>()
                                  .Single();

        metadata.Map( Explicit ).Should().BeEquivalentTo(
            new
            {
                IsApiVersionNeutral = false,
                DeclaredApiVersions = Array.Empty<ApiVersion>(),
                SupportedApiVersions = new ApiVersion[] { new( 1, 0 ), new( 2, 0 ), new( 3, 0 ), new( 4, 0 ) },
                DeprecatedApiVersions = new ApiVersion[] { new( 0, 9 ), new( 3, 0, "Beta" ) },
                ImplementedApiVersions = new ApiVersion[] { new( 0, 9 ), new( 1, 0 ), new( 2, 0 ), new( 3, 0 ), new( 3, 0, "Beta" ), new( 4, 0 ) },
            } );
    }
}