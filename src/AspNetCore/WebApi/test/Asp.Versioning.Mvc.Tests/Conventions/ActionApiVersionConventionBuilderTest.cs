// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Conventions;

using Asp.Versioning.Builder;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
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
        var actionModel = new ActionModel( method, [] )
        {
            Controller = new( typeof( ControllerBase ).GetTypeInfo(), [] ),
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
            Controller = new( typeof( ControllerBase ).GetTypeInfo(), [] ),
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
            Controller = new( typeof( ControllerBase ).GetTypeInfo(), [] ),
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

    [Fact]
    public void apply_to_should_expand_declared_api_versions_from_introduced_convention()
    {
        // arrange
        var controllerBuilder = new ControllerApiVersionConventionBuilder( typeof( UndecoratedController ) );
        var actionBuilder = new ActionApiVersionConventionBuilder( controllerBuilder );
        var actionModel = NewActionModel( typeof( UndecoratedController ), nameof( UndecoratedController.Get ) );

        actionModel.Controller.Properties[typeof( ApiVersionModel )] = NewControllerModel();
        actionBuilder.IntroducedInApiVersion( new ApiVersion( 2, 0 ) );

        // act
        actionBuilder.ApplyTo( actionModel );

        // assert
        actionModel.Selectors
                   .Single()
                   .EndpointMetadata
                   .OfType<ApiVersionMetadata>()
                   .Single()
                   .Map( Explicit )
                   .DeclaredApiVersions
                   .Should()
                   .Equal( new ApiVersion( 2, 0 ), new ApiVersion( 3, 0 ) );
    }

    [Fact]
    public void apply_to_should_intersect_supported_api_versions_with_introduced_convention()
    {
        // arrange
        var controllerBuilder = new ControllerApiVersionConventionBuilder( typeof( UndecoratedController ) );
        var actionBuilder = new ActionApiVersionConventionBuilder( controllerBuilder );
        var actionModel = NewActionModel( typeof( UndecoratedController ), nameof( UndecoratedController.Get ) );
        var version1 = new ApiVersion( 1, 0 );
        var version2 = new ApiVersion( 2, 0 );
        var version3 = new ApiVersion( 3, 0 );

        actionModel.Controller.Properties[typeof( ApiVersionModel )] = NewControllerModel();
        actionBuilder.IntroducedInApiVersion( version2 );

        // act
        actionBuilder.ApplyTo( actionModel );

        // assert
        var metadata = GetApiVersionMetadata( actionModel );
        var model = metadata.Map( Explicit | Implicit );

        model.SupportedApiVersions.Should().NotContain( version1 );
        model.SupportedApiVersions.Should().ContainInOrder( version2, version3 );
        metadata.MappingTo( version2 ).Should().Be( Explicit );
        metadata.MappingTo( version3 ).Should().Be( Explicit );
    }

    [Fact]
    public void apply_to_should_preserve_inherited_supported_api_versions_with_mapped_convention()
    {
        // arrange
        var controllerBuilder = new ControllerApiVersionConventionBuilder( typeof( UndecoratedController ) );
        var actionBuilder = new ActionApiVersionConventionBuilder( controllerBuilder );
        var actionModel = NewActionModel( typeof( UndecoratedController ), nameof( UndecoratedController.Get ) );
        var version1 = new ApiVersion( 1, 0 );
        var version2 = new ApiVersion( 2, 0 );
        var version3 = new ApiVersion( 3, 0 );

        actionModel.Controller.Properties[typeof( ApiVersionModel )] = NewControllerModel();
        actionBuilder.MapToApiVersion( version2 );

        // act
        actionBuilder.ApplyTo( actionModel );

        // assert
        GetApiVersionMetadata( actionModel )
            .Map( Explicit | Implicit )
            .SupportedApiVersions
            .Should()
            .Equal( version1, version2, version3 );
    }

    [Fact]
    public void introduced_in_api_version_should_support_fluent_overloads()
    {
        // arrange
        var controllerBuilder = new ControllerApiVersionConventionBuilder( typeof( UndecoratedController ) );
        var actionBuilder = new ActionApiVersionConventionBuilder( controllerBuilder );
        var actionModel = NewActionModel( typeof( UndecoratedController ), nameof( UndecoratedController.Get ) );

        actionBuilder.IntroducedInApiVersion( new ApiVersion( 2, 0 ), 409 )
                     .IntroducedInApiVersion( 3, 0 )
                     .IntroducedInApiVersion( 4.0 )
                     .IntroducedInApiVersion( 2026, 12, 1 )
                     .IntroducedInApiVersion( new DateOnly( 2027, 6, 1 ) );

        // act
        actionBuilder.ApplyTo( actionModel );

        // assert
        actionModel.Selectors
                   .Single()
                   .EndpointMetadata
                   .OfType<IntroducedInApiVersionMetadata>()
                   .Should()
                   .BeEquivalentTo(
                       new IntroducedInApiVersionMetadata[]
                       {
                           new( new ApiVersion( 2, 0 ), 409 ),
                           new( new ApiVersion( 3, 0 ), IntroducedInApiVersionAttribute.DefaultStatusCode ),
                           new( new ApiVersion( 4.0 ), IntroducedInApiVersionAttribute.DefaultStatusCode ),
                           new( new ApiVersion( new DateOnly( 2026, 12, 1 ) ), IntroducedInApiVersionAttribute.DefaultStatusCode ),
                           new( new ApiVersion( new DateOnly( 2027, 6, 1 ) ), IntroducedInApiVersionAttribute.DefaultStatusCode ),
                       },
                       options => options.WithStrictOrdering() );
    }

    [Fact]
    public void introduced_convention_should_match_attribute_declared_versions()
    {
        // arrange
        var conventionAction = NewActionModel( typeof( UndecoratedController ), nameof( UndecoratedController.Get ) );
        var attributeAction = NewActionModel( typeof( DecoratedController ), nameof( DecoratedController.GetIntroduced ) );
        var controllerBuilder = new ControllerApiVersionConventionBuilder( typeof( UndecoratedController ) );
        var actionBuilder = new ActionApiVersionConventionBuilder( controllerBuilder );

        conventionAction.Controller.Properties[typeof( ApiVersionModel )] = NewControllerModel();
        attributeAction.Controller.Properties[typeof( ApiVersionModel )] = NewControllerModel();
        actionBuilder.IntroducedInApiVersion( new ApiVersion( 2, 0 ) );

        // act
        actionBuilder.ApplyTo( conventionAction );
        new ActionApiVersionConventionBuilder( new ControllerApiVersionConventionBuilder( typeof( DecoratedController ) ) )
            .ApplyTo( attributeAction );

        // assert
        GetApiVersionMetadata( conventionAction ).Map( Explicit ).DeclaredApiVersions.Should().Equal(
            GetApiVersionMetadata( attributeAction ).Map( Explicit ).DeclaredApiVersions );
    }

    [Fact]
    public void introduced_convention_should_match_minimal_api_mapped_versions()
    {
        // arrange
        var action = NewActionModel( typeof( UndecoratedController ), nameof( UndecoratedController.Get ) );
        var controllerBuilder = new ControllerApiVersionConventionBuilder( typeof( UndecoratedController ) );
        var actionBuilder = new ActionApiVersionConventionBuilder( controllerBuilder );

        action.Controller.Properties[typeof( ApiVersionModel )] = NewControllerModel(
            new ApiVersion( 1, 0 ),
            new ApiVersion( 2, 0 ),
            new ApiVersion( 3, 0 ),
            new ApiVersion( 4, 0 ) );
        actionBuilder.MapToApiVersion( new ApiVersion( 2, 0 ) )
                     .IntroducedInApiVersion( new ApiVersion( 3, 0 ) );

        // act
        actionBuilder.ApplyTo( action );
        var mvcModel = GetApiVersionMetadata( action ).Map( Explicit );
        var minimalModel = NewMinimalApiVersionMetadata(
            route => route.MapToApiVersion( 2.0 ).IntroducedInApiVersion( 3.0 ) ).Map( Explicit );

        // assert
        minimalModel.DeclaredApiVersions.Should().Equal( mvcModel.DeclaredApiVersions );
        minimalModel.ImplementedApiVersions.Should().Equal( mvcModel.ImplementedApiVersions );
    }

    [Fact]
    public void introduced_convention_should_match_minimal_api_supported_versions()
    {
        // arrange
        var action = NewActionModel( typeof( UndecoratedController ), nameof( UndecoratedController.Get ) );
        var controllerBuilder = new ControllerApiVersionConventionBuilder( typeof( UndecoratedController ) );
        var actionBuilder = new ActionApiVersionConventionBuilder( controllerBuilder );

        action.Controller.Properties[typeof( ApiVersionModel )] = NewControllerModel(
            new ApiVersion( 1, 0 ),
            new ApiVersion( 2, 0 ),
            new ApiVersion( 3, 0 ),
            new ApiVersion( 4, 0 ) );
        actionBuilder.HasApiVersion( new ApiVersion( 1, 0 ) )
                     .IntroducedInApiVersion( new ApiVersion( 3, 0 ) );

        // act
        actionBuilder.ApplyTo( action );
        var mvcModel = GetApiVersionMetadata( action ).Map( Explicit );
        var minimalModel = NewMinimalApiVersionMetadata(
            route => route.HasApiVersion( 1.0 ).IntroducedInApiVersion( 3.0 ) ).Map( Explicit );

        // assert
        minimalModel.DeclaredApiVersions.Should().Equal( mvcModel.DeclaredApiVersions );
        minimalModel.ImplementedApiVersions.Should().Equal( mvcModel.ImplementedApiVersions );
    }

    private static ActionModel NewActionModel( Type controllerType, string actionName )
    {
        var controller = new ControllerModel( controllerType.GetTypeInfo(), [] )
        {
            ControllerName = controllerType.Name.Replace( "Controller", string.Empty, StringComparison.Ordinal ),
        };
        var method = controllerType.GetMethod( actionName );
        var action = new ActionModel( method, method.GetCustomAttributes().Cast<object>().ToArray() )
        {
            Controller = controller,
        };

        return action;
    }

    private static ApiVersionMetadata GetApiVersionMetadata( ActionModel action ) =>
        action.Selectors.Single().EndpointMetadata.OfType<ApiVersionMetadata>().Single();

    private static ApiVersionModel NewControllerModel()
    {
        var versions = new ApiVersion[] { new( 1, 0 ), new( 2, 0 ), new( 3, 0 ) };

        return NewControllerModel( versions );
    }

    private static ApiVersionModel NewControllerModel( params ApiVersion[] versions ) => new( versions, versions, [], [], [] );

    private static ApiVersionMetadata NewMinimalApiVersionMetadata( Action<RouteHandlerBuilder> configure )
    {
        var dataSources = new List<EndpointDataSource>();
        var app = new Mock<IEndpointRouteBuilder>();

        app.SetupGet( a => a.ServiceProvider ).Returns( new MockServiceProvider() );
        app.SetupGet( a => a.DataSources ).Returns( dataSources );

        var versionSet = app.Object.NewApiVersionSet()
                                   .HasApiVersion( 1.0 )
                                   .HasApiVersion( 2.0 )
                                   .HasApiVersion( 3.0 )
                                   .HasApiVersion( 4.0 )
                                   .Build();
        var route = app.Object.MapGet( "/test", () => Results.Ok() )
                              .WithApiVersionSet( versionSet );

        configure( route );

        return dataSources.Single()
                          .Endpoints
                          .Single()
                          .Metadata
                          .OfType<ApiVersionMetadata>()
                          .Single();
    }

    private sealed class MockServiceProvider : IServiceProvider
    {
        private readonly IOptions<ApiVersioningOptions> options = Options.Create( new ApiVersioningOptions() );

        public object GetService( Type serviceType )
        {
            if ( typeof( IOptions<ApiVersioningOptions> ) == serviceType )
            {
                return options;
            }

            if ( typeof( IApiVersionParameterSource ) == serviceType )
            {
                return options.Value.ApiVersionReader;
            }

            return null;
        }
    }
}