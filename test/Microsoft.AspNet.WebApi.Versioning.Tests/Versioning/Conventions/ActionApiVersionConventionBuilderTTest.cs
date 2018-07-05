namespace Microsoft.Web.Http.Versioning.Conventions
{
    using FluentAssertions;
    using Moq;
    using System.Collections.ObjectModel;
    using System.Reflection;
    using System.Web.Http;
    using System.Web.Http.Controllers;
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
            var actionDescriptor = new Mock<HttpActionDescriptor>() { CallBase = true };

            actionDescriptor.Setup( ad => ad.GetCustomAttributes<IApiVersionProvider>() ).Returns( new Collection<IApiVersionProvider>() );

            // act
            actionBuilder.ApplyTo( actionDescriptor.Object );

            // assert
            actionDescriptor.Object.GetApiVersionModel().Should().BeEquivalentTo(
                new
                {
                    IsApiVersionNeutral = false,
                    DeclaredApiVersions = new ApiVersion[0],
                    SupportedApiVersions = new ApiVersion[0],
                    DeprecatedApiVersions = new ApiVersion[0],
                    ImplementedApiVersions = new ApiVersion[0]
                } );
        }

        [Fact]
        public void apply_to_should_assign_model_with_declared_api_versions_from_mapped_convention()
        {
            // arrange
            var controllerBuilder = new ControllerApiVersionConventionBuilder<UndecoratedController>();
            var actionBuilder = new ActionApiVersionConventionBuilder<UndecoratedController>( controllerBuilder );
            var actionDescriptor = new Mock<HttpActionDescriptor>() { CallBase = true };

            actionDescriptor.Setup( ad => ad.GetCustomAttributes<IApiVersionProvider>() ).Returns( new Collection<IApiVersionProvider>() );
            actionBuilder.MapToApiVersion( new ApiVersion( 2, 0 ) );

            // act
            actionBuilder.ApplyTo( actionDescriptor.Object );

            // assert
            actionDescriptor.Object.GetApiVersionModel().Should().BeEquivalentTo(
                new
                {
                    IsApiVersionNeutral = false,
                    DeclaredApiVersions = new[] { new ApiVersion( 2, 0 ) },
                    SupportedApiVersions = new[] { new ApiVersion( 2, 0 ) },
                    DeprecatedApiVersions = new ApiVersion[0],
                    ImplementedApiVersions = new[] { new ApiVersion( 2, 0 ) }
                } );
        }

        [Fact]
        public void apply_to_should_assign_model_with_declared_api_versions_from_mapped_convention_and_attributes()
        {
            // arrange
            var controllerBuilder = new ControllerApiVersionConventionBuilder<DecoratedController>();
            var actionBuilder = new ActionApiVersionConventionBuilder<DecoratedController>( controllerBuilder );
            var controllerDescriptor = new HttpControllerDescriptor() { ControllerType = typeof( DecoratedController ) };
            var method = typeof( DecoratedController ).GetMethod( nameof( DecoratedController.Get ) );
            var actionDescriptor = new ReflectedHttpActionDescriptor( controllerDescriptor, method );

            actionBuilder.MapToApiVersion( new ApiVersion( 2, 0 ) )
                         .MapToApiVersion( new ApiVersion( 3, 0 ) );

            // act
            actionBuilder.ApplyTo( actionDescriptor );

            // assert
            actionDescriptor.GetApiVersionModel().Should().BeEquivalentTo(
                new
                {
                    IsApiVersionNeutral = false,
                    DeclaredApiVersions = new[] { new ApiVersion( 2, 0 ), new ApiVersion( 3, 0 ) },
                    SupportedApiVersions = new[] { new ApiVersion( 2, 0 ), new ApiVersion( 3, 0 ) },
                    DeprecatedApiVersions = new ApiVersion[0],
                    ImplementedApiVersions = new[] { new ApiVersion( 2, 0 ), new ApiVersion( 3, 0 ) }
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

        public sealed class UndecoratedController : ApiController
        {
            public IHttpActionResult Get() => Ok();
        }

        public sealed class DecoratedController : ApiController
        {
            public IHttpActionResult Get() => Ok();

            [MapToApiVersion( "2.0" )]
            [MapToApiVersion( "3.0" )]
            public IHttpActionResult GetV2() => Ok();
        }
    }
}