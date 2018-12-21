namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using ApplicationModels;
    using FluentAssertions;
    using Microsoft.Extensions.Options;
    using System;
    using System.Linq;
    using System.Reflection;
    using Xunit;
    using static System.Type;

    public class ApiVersioningApplicationModelProviderTest
    {
        [Fact]
        public void on_providers_executed_should_apply_api_version_model_conventions()
        {
            // arrange
            var supported = new[] { new ApiVersion( 1, 0 ), new ApiVersion( 2, 0 ), new ApiVersion( 3, 0 ) };
            var deprecated = new[] { new ApiVersion( 0, 9 ) };
            var type = typeof( object );
            var attributes = new object[]
            {
                new ApiControllerAttribute(),
                new ApiVersionAttribute( "1.0" ),
                new ApiVersionAttribute( "2.0" ),
                new ApiVersionAttribute( "3.0" ),
                new ApiVersionAttribute( "0.9" ) { Deprecated = true }
            };
            var actionMethod = type.GetRuntimeMethod( nameof( object.ToString ), EmptyTypes );
            var controller = new ControllerModel( type.GetTypeInfo(), attributes )
            {
                Actions = { new ActionModel( actionMethod, Array.Empty<object>() ) }
            };
            var options = Options.Create( new ApiVersioningOptions() { UseApiBehavior = true } );
            var filter = new DefaultApiControllerFilter( Array.Empty<IApiControllerSpecification>() );
            var context = new ApplicationModelProviderContext( new[] { controller.ControllerType } );
            var provider = new ApiVersioningApplicationModelProvider( options, filter );

            context.Result.Controllers.Add( controller );

            // act
            provider.OnProvidersExecuted( context );

            // assert
            controller.Actions.Single().GetProperty<ApiVersionModel>().Should().BeEquivalentTo(
                new
                {
                    IsApiVersionNeutral = false,
                    DeclaredApiVersions = Array.Empty<ApiVersion>(),
                    ImplementedApiVersions = deprecated.Union( supported ).ToArray(),
                    SupportedApiVersions = supported,
                    DeprecatedApiVersions = deprecated
                } );
        }

        [Fact]
        public void on_providers_executed_should_apply_api_versionX2Dneutral_model_conventions()
        {
            // arrange
            var model = ApiVersionModel.Neutral;
            var type = typeof( object );
            var attributes = new object[] { new ApiVersionNeutralAttribute() };
            var actionMethod = type.GetRuntimeMethod( nameof( object.ToString ), EmptyTypes );
            var controller = new ControllerModel( type.GetTypeInfo(), attributes )
            {
                Actions = { new ActionModel( actionMethod, new object[0] ) }
            };
            var options = Options.Create( new ApiVersioningOptions() { UseApiBehavior = false } );
            var filter = new DefaultApiControllerFilter( Array.Empty<IApiControllerSpecification>() );
            var context = new ApplicationModelProviderContext( new[] { controller.ControllerType } );
            var provider = new ApiVersioningApplicationModelProvider( options, filter );

            context.Result.Controllers.Add( controller );

            // act
            provider.OnProvidersExecuted( context );

            // assert
            controller.Actions.Single().GetProperty<ApiVersionModel>().Should().BeSameAs( model );
        }

        [Fact]
        public void on_providers_executed_should_apply_implicit_api_version_model_conventions()
        {
            // arrange
            var type = typeof( object );
            var attributes = new object[0];
            var actionMethod = type.GetRuntimeMethod( nameof( object.ToString ), EmptyTypes );
            var controller = new ControllerModel( type.GetTypeInfo(), attributes )
            {
                Actions = { new ActionModel( actionMethod, new object[0] ) }
            };
            var options = Options.Create( new ApiVersioningOptions() { UseApiBehavior = false } );
            var filter = new DefaultApiControllerFilter( Array.Empty<IApiControllerSpecification>() );
            var context = new ApplicationModelProviderContext( new[] { controller.ControllerType } );
            var provider = new ApiVersioningApplicationModelProvider( options, filter );

            context.Result.Controllers.Add( controller );

            // act
            provider.OnProvidersExecuted( context );

            // assert
            controller.Actions.Single().GetProperty<ApiVersionModel>().Should().BeEquivalentTo(
                new
                {
                    IsApiVersionNeutral = false,
                    DeclaredApiVersions = new[] { new ApiVersion( 1, 0 ) },
                    ImplementedApiVersions = new[] { new ApiVersion( 1, 0 ) },
                    SupportedApiVersions = new[] { new ApiVersion( 1, 0 ) },
                    DeprecatedApiVersions = new ApiVersion[0],
                } );
        }

        [Fact]
        public void on_providers_executed_should_not_apply_implicit_api_version_model_conventions_to_controller_and_actions_with_explicit_api_versions()
        {
            // arrange
            var type = typeof( object );
            var attributes = new object[] { new ApiVersionAttribute( "2.0" ) };
            var actionMethod = type.GetRuntimeMethod( nameof( object.ToString ), EmptyTypes );
            var v1 = new ApiVersion( 1, 0 );
            var controller = new ControllerModel( type.GetTypeInfo(), attributes )
            {
                Actions = { new ActionModel( actionMethod, new object[0] ) }
            };
            var options = Options.Create( new ApiVersioningOptions() { DefaultApiVersion = v1, UseApiBehavior = false } );
            var filter = new DefaultApiControllerFilter( Array.Empty<IApiControllerSpecification>() );
            var context = new ApplicationModelProviderContext( new[] { controller.ControllerType } );
            var provider = new ApiVersioningApplicationModelProvider( options, filter );

            context.Result.Controllers.Add( controller );

            // act
            provider.OnProvidersExecuted( context );

            // assert
            controller.Actions.Single().GetProperty<ApiVersionModel>().ImplementedApiVersions.Should().NotContain( v1 );
        }

        [Fact]
        public void on_providers_executed_should_only_apply_api_version_model_conventions_with_api_behavior()
        {
            // arrange
            var supported = new[] { new ApiVersion( 1, 0 ) };
            var deprecated = new ApiVersion[0];
            var type = typeof( object );
            var attributes = new object[]
            {
                new ApiControllerAttribute(),
                new ApiVersionAttribute( "1.0" ),
            };
            var actionMethod = type.GetRuntimeMethod( nameof( object.ToString ), EmptyTypes );
            var apiController = new ControllerModel( type.GetTypeInfo(), attributes )
            {
                Actions = { new ActionModel( actionMethod, new object[0] ) },
            };
            var uiController = new ControllerModel( type.GetTypeInfo(), new object[0] )
            {
                Actions = { new ActionModel( actionMethod, new object[0] ) },
            };
            var controllers = new[] { apiController, uiController };
            var controllerTypes = new[] { apiController.ControllerType, uiController.ControllerType };
            var options = Options.Create( new ApiVersioningOptions() { UseApiBehavior = true } );
            var filter = new DefaultApiControllerFilter( new IApiControllerSpecification[] { new ApiBehaviorSpecification() } );
            var context = new ApplicationModelProviderContext( controllerTypes );
            var provider = new ApiVersioningApplicationModelProvider( options, filter );

            context.Result.Controllers.Add( apiController );
            context.Result.Controllers.Add( uiController );

            // act
            provider.OnProvidersExecuted( context );

            // assert
            apiController.Actions.Single().GetProperty<ApiVersionModel>().Should().BeEquivalentTo(
                new
                {
                    IsApiVersionNeutral = false,
                    DeclaredApiVersions = Array.Empty<ApiVersion>(),
                    ImplementedApiVersions = supported,
                    SupportedApiVersions = supported,
                    DeprecatedApiVersions = deprecated,
                } );
            uiController.Actions.Single().GetProperty<ApiVersionModel>().Should().BeNull();
        }
    }
}