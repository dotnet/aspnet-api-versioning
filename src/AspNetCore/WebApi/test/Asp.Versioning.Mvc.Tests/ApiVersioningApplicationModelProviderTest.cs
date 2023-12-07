// Copyright (c) .NET Foundation and contributors. All rights reserved.

//// Ignore Spelling: Dneutral

namespace Asp.Versioning;

using Asp.Versioning.ApplicationModels;
using Asp.Versioning.Conventions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.Options;
using System.Reflection;
using static Asp.Versioning.ApiVersionMapping;

public class ApiVersioningApplicationModelProviderTest
{
    [Fact]
    public void on_providers_executing_should_apply_api_version_model_conventions()
    {
        // arrange
        var supported = new ApiVersion[] { new( 1, 0 ), new( 2, 0 ), new( 3, 0 ) };
        var deprecated = new[] { new ApiVersion( 0, 9 ) };
        var type = typeof( object );
        var attributes = new object[]
        {
                new ApiControllerAttribute(),
                new ApiVersionAttribute( "1.0" ),
                new ApiVersionAttribute( "2.0" ),
                new ApiVersionAttribute( "3.0" ),
                new ApiVersionAttribute( "0.9" ) { Deprecated = true },
        };
        var actionMethod = type.GetRuntimeMethod( nameof( object.ToString ), Type.EmptyTypes );
        var controller = new ControllerModel( type.GetTypeInfo(), attributes )
        {
            Actions = { new ActionModel( actionMethod, Array.Empty<object>() ) },
        };
        var context = new ApplicationModelProviderContext( new[] { controller.ControllerType } );
        var provider = new ApiVersioningApplicationModelProvider(
            new DefaultApiControllerFilter( Array.Empty<IApiControllerSpecification>() ),
            ControllerNameConvention.Default,
            Options.Create( new ApiVersioningOptions() ),
            Options.Create( new MvcApiVersioningOptions() ) );

        controller.Actions[0].Controller = controller;
        context.Result.Controllers.Add( controller );

        // act
        provider.OnProvidersExecuting( context );

        // assert
        var metadata = controller.Actions
                                 .Single()
                                 .Selectors
                                 .Single()
                                 .EndpointMetadata
                                 .OfType<ApiVersionMetadata>()
                                 .Single();

        metadata.Map( Explicit ).Should().BeEquivalentTo(
            new
            {
                IsApiVersionNeutral = false,
                DeclaredApiVersions = Array.Empty<ApiVersion>(),
                ImplementedApiVersions = deprecated.Union( supported ).ToArray(),
                SupportedApiVersions = supported,
                DeprecatedApiVersions = deprecated,
            } );
    }

    [Fact]
    public void on_providers_executing_should_apply_api_versionX2Dneutral_model_conventions()
    {
        // arrange
        var metadata = ApiVersionMetadata.Neutral;
        var type = typeof( object );
        var attributes = new object[] { new ApiVersionNeutralAttribute() };
        var actionMethod = type.GetRuntimeMethod( nameof( object.ToString ), Type.EmptyTypes );
        var controller = new ControllerModel( type.GetTypeInfo(), attributes )
        {
            Actions = { new ActionModel( actionMethod, Array.Empty<object>() ) },
        };
        var context = new ApplicationModelProviderContext( new[] { controller.ControllerType } );
        var provider = new ApiVersioningApplicationModelProvider(
            new NoControllerFilter(),
            ControllerNameConvention.Default,
            Options.Create( new ApiVersioningOptions() ),
            Options.Create( new MvcApiVersioningOptions() ) );

        controller.Actions[0].Controller = controller;
        context.Result.Controllers.Add( controller );

        // act
        provider.OnProvidersExecuting( context );

        // assert
        controller.Actions
                  .Single()
                  .Selectors
                  .Single()
                  .EndpointMetadata
                  .OfType<ApiVersionMetadata>()
                  .Single()
                  .IsApiVersionNeutral
                  .Should()
                  .BeTrue();
    }

    [Fact]
    public void on_providers_executing_should_apply_implicit_api_version_model_conventions()
    {
        // arrange
        var type = typeof( object );
        var attributes = Array.Empty<object>();
        var actionMethod = type.GetRuntimeMethod( nameof( object.ToString ), Type.EmptyTypes );
        var controller = new ControllerModel( type.GetTypeInfo(), attributes )
        {
            Actions = { new ActionModel( actionMethod, Array.Empty<object>() ) },
        };
        var context = new ApplicationModelProviderContext( new[] { controller.ControllerType } );
        var provider = new ApiVersioningApplicationModelProvider(
            new NoControllerFilter(),
            ControllerNameConvention.Default,
            Options.Create( new ApiVersioningOptions() ),
            Options.Create( new MvcApiVersioningOptions() ) );

        controller.Actions[0].Controller = controller;
        context.Result.Controllers.Add( controller );

        // act
        provider.OnProvidersExecuting( context );

        // assert
        var metadata = controller.Actions
                                 .Single()
                                 .Selectors
                                 .Single()
                                 .EndpointMetadata
                                 .OfType<ApiVersionMetadata>()
                                 .Single();

        metadata.Map( Explicit ).Should().BeEquivalentTo(
            new
            {
                IsApiVersionNeutral = false,
                DeclaredApiVersions = Array.Empty<ApiVersion>(),
                ImplementedApiVersions = new[] { new ApiVersion( 1, 0 ) },
                SupportedApiVersions = new[] { new ApiVersion( 1, 0 ) },
                DeprecatedApiVersions = Array.Empty<ApiVersion>(),
            } );
    }

    [Fact]
    public void on_providers_executing_should_not_apply_implicit_api_version_model_conventions_to_controller_and_actions_with_explicit_api_versions()
    {
        // arrange
        var type = typeof( object );
        var attributes = new object[] { new ApiVersionAttribute( "2.0" ) };
        var actionMethod = type.GetRuntimeMethod( nameof( object.ToString ), Type.EmptyTypes );
        var v1 = new ApiVersion( 1, 0 );
        var controller = new ControllerModel( type.GetTypeInfo(), attributes )
        {
            Actions = { new ActionModel( actionMethod, Array.Empty<object>() ) },
        };
        var context = new ApplicationModelProviderContext( new[] { controller.ControllerType } );
        var provider = new ApiVersioningApplicationModelProvider(
            new DefaultApiControllerFilter( Array.Empty<IApiControllerSpecification>() ),
            ControllerNameConvention.Default,
            Options.Create( new ApiVersioningOptions() { DefaultApiVersion = v1 } ),
            Options.Create( new MvcApiVersioningOptions() ) );

        controller.Actions[0].Controller = controller;
        context.Result.Controllers.Add( controller );

        // act
        provider.OnProvidersExecuting( context );

        // assert
        var metadata = controller.Actions
                                 .Single()
                                 .Selectors
                                 .Single()
                                 .EndpointMetadata
                                 .OfType<ApiVersionMetadata>()
                                 .Single();

        metadata.Map( Explicit ).ImplementedApiVersions.Should().NotContain( v1 );
    }

    [Fact]
    public void on_providers_executing_should_only_apply_api_version_model_conventions_with_api_behavior()
    {
        // arrange
        var supported = new[] { new ApiVersion( 1, 0 ) };
        var deprecated = Array.Empty<ApiVersion>();
        var type = typeof( object );
        var attributes = new object[]
        {
            new ApiControllerAttribute(),
            new ApiVersionAttribute( "1.0" ),
        };
        var actionMethod = type.GetRuntimeMethod( nameof( object.ToString ), Type.EmptyTypes );
        var apiController = new ControllerModel( type.GetTypeInfo(), attributes )
        {
            Actions = { new ActionModel( actionMethod, Array.Empty<object>() ) },
        };
        var uiController = new ControllerModel( type.GetTypeInfo(), Array.Empty<object>() )
        {
            Actions = { new ActionModel( actionMethod, Array.Empty<object>() ) },
        };
        var controllers = new[] { apiController, uiController };
        var controllerTypes = new[] { apiController.ControllerType, uiController.ControllerType };
        var context = new ApplicationModelProviderContext( controllerTypes );
        var provider = new ApiVersioningApplicationModelProvider(
            new DefaultApiControllerFilter( new[] { new ApiBehaviorSpecification() } ),
            ControllerNameConvention.Default,
            Options.Create( new ApiVersioningOptions() ),
            Options.Create( new MvcApiVersioningOptions() ) );

        apiController.Actions[0].Controller = apiController;
        uiController.Actions[0].Controller = uiController;
        context.Result.Controllers.Add( apiController );
        context.Result.Controllers.Add( uiController );

        // act
        provider.OnProvidersExecuting( context );

        // assert
        var metadata = apiController.Actions
                                    .Single()
                                    .Selectors
                                    .Single()
                                    .EndpointMetadata
                                    .OfType<ApiVersionMetadata>()
                                    .Single();

        metadata.Map( Explicit ).Should().BeEquivalentTo(
            new
            {
                IsApiVersionNeutral = false,
                DeclaredApiVersions = Array.Empty<ApiVersion>(),
                ImplementedApiVersions = supported.Union( deprecated ),
                SupportedApiVersions = supported,
                DeprecatedApiVersions = deprecated,
            } );
        uiController.Actions.Single().Selectors.Should().BeEmpty();
    }

    [Theory]
    [InlineData( "" )]
    [InlineData( "1" )]
    [InlineData( "42" )]
    public void on_providers_executing_should_trim_trailing_numbers_by_convention( string suffix )
    {
        // arrange
        var controllerType = typeof( object ).GetTypeInfo();
        var attributes = new object[] { new ApiControllerAttribute() };
        var controller = new ControllerModel( controllerType, attributes ) { ControllerName = "Values" + suffix };
        var controllerTypes = new[] { controller.ControllerType };
        var context = new ApplicationModelProviderContext( controllerTypes );
        var provider = new ApiVersioningApplicationModelProvider(
            new DefaultApiControllerFilter( new[] { new ApiBehaviorSpecification() } ),
            ControllerNameConvention.Default,
            Options.Create( new ApiVersioningOptions() ),
            Options.Create( new MvcApiVersioningOptions() ) );

        context.Result.Controllers.Add( controller );

        // act
        provider.OnProvidersExecuting( context );

        // assert
        controller.ControllerName.Should().Be( "Values" );
    }
}