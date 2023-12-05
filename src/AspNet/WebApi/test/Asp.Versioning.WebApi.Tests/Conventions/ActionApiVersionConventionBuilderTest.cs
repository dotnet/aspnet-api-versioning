// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Conventions;

using System.Web.Http;
using System.Web.Http.Controllers;
using static Asp.Versioning.ApiVersionMapping;

public partial class ActionApiVersionConventionBuilderTest
{
    [Fact]
    public void apply_to_should_assign_empty_model_without_api_versions_from_mapped_convention()
    {
        // arrange
        var controllerBuilder = new ControllerApiVersionConventionBuilder( typeof( UndecoratedController ) );
        var actionBuilder = new ActionApiVersionConventionBuilder( controllerBuilder );
        var actionDescriptor = new Mock<HttpActionDescriptor>() { CallBase = true };

        actionDescriptor.Setup( ad => ad.GetCustomAttributes<IApiVersionProvider>() ).Returns( [] );
        actionDescriptor.Object.ControllerDescriptor = new();

        // act
        actionBuilder.ApplyTo( actionDescriptor.Object );

        // assert
        actionDescriptor.Object.GetApiVersionMetadata().Map( Explicit ).Should().BeEquivalentTo(
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
        var actionDescriptor = new Mock<HttpActionDescriptor>() { CallBase = true };

        actionDescriptor.Setup( ad => ad.GetCustomAttributes<IApiVersionProvider>() ).Returns( [] );
        actionDescriptor.Object.ControllerDescriptor = new();
        actionBuilder.MapToApiVersion( new ApiVersion( 2, 0 ) );

        // act
        actionBuilder.ApplyTo( actionDescriptor.Object );

        // assert
        actionDescriptor.Object.GetApiVersionMetadata().Map( Explicit ).Should().BeEquivalentTo(
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
        var controllerDescriptor = new HttpControllerDescriptor() { ControllerType = typeof( DecoratedController ) };
        var method = typeof( DecoratedController ).GetMethod( nameof( DecoratedController.Get ) );
        var actionDescriptor = new ReflectedHttpActionDescriptor( controllerDescriptor, method );

        actionBuilder.MapToApiVersion( new ApiVersion( 2, 0 ) )
                     .MapToApiVersion( new ApiVersion( 3, 0 ) );

        // act
        actionBuilder.ApplyTo( actionDescriptor );

        // assert
        actionDescriptor.GetApiVersionMetadata().Map( Explicit ).Should().BeEquivalentTo(
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