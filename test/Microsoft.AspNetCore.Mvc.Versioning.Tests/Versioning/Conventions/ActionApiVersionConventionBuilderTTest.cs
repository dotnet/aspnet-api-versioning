namespace Microsoft.AspNetCore.Mvc.Versioning.Conventions
{
    using ApplicationModels;
    using FluentAssertions;
    using Moq;
    using System;
    using System.Linq;
    using System.Reflection;
    using Xunit;
    using static Moq.Times;

    public class ActionApiVersionConventionBuilderTTest
    {
        [Fact]
        public void apply_to_should_assign_empty_model_without_api_versions_from_mapped_convention()
        {
            // arrange
            var controllerBuilder = new ControllerApiVersionConventionBuilder<UndecoratedController>();
            var actionBuilder = new ActionApiVersionConventionBuilder<UndecoratedController>( controllerBuilder );
            var method = typeof( UndecoratedController ).GetMethod( nameof( UndecoratedController.Get ) );
            var actionModel = new ActionModel( method, new object[0] );
            var empty = Enumerable.Empty<ApiVersion>();
            var controllerVersionInfo = Tuple.Create( empty, empty, empty, empty );

            actionModel.SetProperty( controllerVersionInfo );

            // act
            actionBuilder.ApplyTo( actionModel );

            // assert
            actionModel.GetProperty<ApiVersionModel>().Should().BeEquivalentTo(
                new
                {
                    IsApiVersionNeutral = false,
                    DeclaredApiVersions = new ApiVersion[0],
                    SupportedApiVersions = new ApiVersion[0],
                    DeprecatedApiVersions = new ApiVersion[0],
                    ImplementedApiVersions = new ApiVersion[0],
                } );
        }

        [Fact]
        public void apply_to_should_assign_model_with_declared_api_versions_from_mapped_convention()
        {
            // arrange
            var controllerBuilder = new ControllerApiVersionConventionBuilder<UndecoratedController>();
            var actionBuilder = new ActionApiVersionConventionBuilder<UndecoratedController>( controllerBuilder );
            var method = typeof( UndecoratedController ).GetMethod( nameof( UndecoratedController.Get ) );
            var attributes = new object[] { new MapToApiVersionAttribute( "2.0" ) };
            var actionModel = new ActionModel( method, attributes );
            var empty = Enumerable.Empty<ApiVersion>();
            var controllerVersionInfo = Tuple.Create( empty, empty, empty, empty );

            actionModel.SetProperty( controllerVersionInfo );
            actionBuilder.MapToApiVersion( new ApiVersion( 2, 0 ) );

            // act
            actionBuilder.ApplyTo( actionModel );

            // assert
            actionModel.GetProperty<ApiVersionModel>().Should().BeEquivalentTo(
                new
                {
                    IsApiVersionNeutral = false,
                    DeclaredApiVersions = new[] { new ApiVersion( 2, 0 ) },
                    SupportedApiVersions = new ApiVersion[0],
                    DeprecatedApiVersions = new ApiVersion[0],
                    ImplementedApiVersions = new ApiVersion[0],
                } );
        }

        [Fact]
        public void apply_to_should_assign_model_with_declared_api_versions_from_mapped_convention_and_attributes()
        {
            // arrange
            var controllerBuilder = new ControllerApiVersionConventionBuilder<DecoratedController>();
            var actionBuilder = new ActionApiVersionConventionBuilder<DecoratedController>( controllerBuilder );
            var method = typeof( DecoratedController ).GetMethod( nameof( DecoratedController.Get ) );
            var attributes = method.GetCustomAttributes().Cast<object>().ToArray();
            var actionModel = new ActionModel( method, attributes );
            var empty = Enumerable.Empty<ApiVersion>();
            var controllerVersionInfo = Tuple.Create( empty, empty, empty, empty );

            actionModel.SetProperty( controllerVersionInfo );
            actionBuilder.MapToApiVersion( new ApiVersion( 2, 0 ) )
                         .MapToApiVersion( new ApiVersion( 3, 0 ) );

            // act
            actionBuilder.ApplyTo( actionModel );

            // assert
            actionModel.GetProperty<ApiVersionModel>().Should().BeEquivalentTo(
                new
                {
                    IsApiVersionNeutral = false,
                    DeclaredApiVersions = new[] { new ApiVersion( 2, 0 ), new ApiVersion( 3, 0 ) },
                    SupportedApiVersions = new ApiVersion[0],
                    DeprecatedApiVersions = new ApiVersion[0],
                    ImplementedApiVersions = new ApiVersion[0],
                } );
        }

        [Fact]
        public void action_should_call_action_on_controller_builder()
        {
            // arrange
            var controllerBuilder = new Mock<ControllerApiVersionConventionBuilder<UndecoratedController>>();
            var actionBuilder = new ActionApiVersionConventionBuilder<UndecoratedController>( controllerBuilder.Object );
            var method = typeof( UndecoratedController ).GetMethod( nameof( UndecoratedController.Get ) );

            controllerBuilder.Setup( cb => cb.Action( It.IsAny<MethodInfo>() ) );

            // act
            actionBuilder.Action( method );

            // assert
            controllerBuilder.Verify( cb => cb.Action( method ), Once() );
        }

        public sealed class UndecoratedController : Controller
        {
            public IActionResult Get() => Ok();
        }

        public sealed class DecoratedController : Controller
        {
            public IActionResult Get() => Ok();

            [MapToApiVersion( "2.0" )]
            [MapToApiVersion( "3.0" )]
            public IActionResult GetV2() => Ok();
        }
    }
}