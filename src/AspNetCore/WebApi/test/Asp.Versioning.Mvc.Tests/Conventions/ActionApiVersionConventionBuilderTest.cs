// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Conventions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System.Reflection;
using static Asp.Versioning.ApiVersionMapping;

public partial class ActionApiVersionConventionBuilderTest
{
    [Fact]
    public void apply_to_should_assign_empty_model_without_api_versions_from_mapped_convention()
    {
        // arrange
        var controllerBuilder = new ControllerApiVersionConventionBuilder( typeof( UndecoratedController ) );
        var actionBuilder = new ActionApiVersionConventionBuilder( controllerBuilder );
        var method = typeof( UndecoratedController ).GetMethod( nameof( UndecoratedController.Get ) );
        var actionModel = new ActionModel( method, Array.Empty<object>() )
        {
            Controller = new( typeof( ControllerBase ).GetTypeInfo(), Array.Empty<object>() ),
        };

        // act
        actionBuilder.ApplyTo( actionModel );

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
                SupportedApiVersions = Array.Empty<ApiVersion>(),
                DeprecatedApiVersions = Array.Empty<ApiVersion>(),
                ImplementedApiVersions = Array.Empty<ApiVersion>(),
            } );
    }

    [Fact]
    public void apply_to_should_assign_model_with_declared_api_versions_from_mapped_convention()
    {
        // arrange
        var controllerBuilder = new ControllerApiVersionConventionBuilder( typeof( UndecoratedController ) );
        var actionBuilder = new ActionApiVersionConventionBuilder( controllerBuilder );
        var method = typeof( UndecoratedController ).GetMethod( nameof( UndecoratedController.Get ) );
        var attributes = new object[] { new MapToApiVersionAttribute( "2.0" ) };
        var actionModel = new ActionModel( method, attributes )
        {
            Controller = new( typeof( ControllerBase ).GetTypeInfo(), Array.Empty<object>() ),
        };

        actionBuilder.MapToApiVersion( new ApiVersion( 2, 0 ) );

        // act
        actionBuilder.ApplyTo( actionModel );

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
                DeclaredApiVersions = new[] { new ApiVersion( 2, 0 ) },
                SupportedApiVersions = Array.Empty<ApiVersion>(),
                DeprecatedApiVersions = Array.Empty<ApiVersion>(),
                ImplementedApiVersions = Array.Empty<ApiVersion>(),
            } );
    }

    [Fact]
    public void apply_to_should_assign_model_with_declared_api_versions_from_mapped_convention_and_attributes()
    {
        // arrange
        var controllerBuilder = new ControllerApiVersionConventionBuilder( typeof( DecoratedController ) );
        var actionBuilder = new ActionApiVersionConventionBuilder( controllerBuilder );
        var method = typeof( DecoratedController ).GetMethod( nameof( DecoratedController.Get ) );
        var attributes = method.GetCustomAttributes().Cast<object>().ToArray();
        var actionModel = new ActionModel( method, attributes )
        {
            Controller = new( typeof( ControllerBase ).GetTypeInfo(), Array.Empty<object>() ),
        };

        actionBuilder.MapToApiVersion( new ApiVersion( 2, 0 ) )
                     .MapToApiVersion( new ApiVersion( 3, 0 ) );

        // act
        actionBuilder.ApplyTo( actionModel );

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
                DeclaredApiVersions = new ApiVersion[] { new( 2, 0 ), new( 3, 0 ) },
                SupportedApiVersions = Array.Empty<ApiVersion>(),
                DeprecatedApiVersions = Array.Empty<ApiVersion>(),
                ImplementedApiVersions = Array.Empty<ApiVersion>(),
            } );
    }
}