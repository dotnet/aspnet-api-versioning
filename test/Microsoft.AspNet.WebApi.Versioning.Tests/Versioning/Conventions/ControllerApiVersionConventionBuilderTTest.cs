namespace Microsoft.Web.Http.Versioning.Conventions
{
    using FluentAssertions;
    using Moq;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Reflection;
    using System.Web.Http;
    using System.Web.Http.Controllers;
    using Xunit;

    public class ControllerApiVersionConventionBuilderTTest
    {
        [Fact]
        public void version_neutral_should_be_false_by_default()
        {
            // arrange
            var controllerBuilder = new TestControllerApiVersionConventionBuilder();

            // act
            var versionNeutral = controllerBuilder.ProtectedVersionNeutral;

            // assert
            versionNeutral.Should().BeFalse();
        }

        [Fact]
        public void is_api_version_neutral_should_update_backing_property()
        {
            // arrange
            var controllerBuilder = new TestControllerApiVersionConventionBuilder();

            // act
            controllerBuilder.IsApiVersionNeutral();

            // assert
            controllerBuilder.ProtectedVersionNeutral.Should().BeTrue();
        }

        [Fact]
        public void action_should_add_new_action_convention_builder()
        {
            // arrange
            var method = typeof( UndecoratedController ).GetMethod( nameof( UndecoratedController.Get ) );
            var controllerBuilder = new TestControllerApiVersionConventionBuilder();

            // act
            var actionBuilder = controllerBuilder.Action( method );

            // assert
            controllerBuilder.ProtectedActionBuilders.Single().Should().BeSameAs( actionBuilder );
        }

        [Fact]
        public void action_should_return_existing_action_convention_builder()
        {
            // arrange
            var method = typeof( UndecoratedController ).GetMethod( nameof( UndecoratedController.Get ) );
            var controllerBuilder = new TestControllerApiVersionConventionBuilder();
            var originalActionBuilder = controllerBuilder.Action( method );

            // act
            var actionBuilder = controllerBuilder.Action( method );

            // assert
            actionBuilder.Should().BeSameAs( originalActionBuilder );
            controllerBuilder.ProtectedActionBuilders.Single().Should().BeSameAs( actionBuilder );
        }

        [Fact]
        public void apply_to_should_assign_conventions_to_controller()
        {
            // arrange
            var configuration = new HttpConfiguration();
            var mock = new Mock<HttpControllerDescriptor>() { CallBase = true };
            var controllerDescriptor = mock.Object;
            var controllerBuilder = default( IControllerConventionBuilder<UndecoratedController> );

            mock.Setup( cd => cd.GetCustomAttributes<IApiVersionProvider>() ).Returns( new Collection<IApiVersionProvider>() );
            controllerDescriptor.Configuration = configuration;
            controllerDescriptor.ControllerType = typeof( UndecoratedController );
            configuration.AddApiVersioning( o => controllerBuilder = o.Conventions.Controller<UndecoratedController>() );
            controllerBuilder.HasDeprecatedApiVersion( 0, 9 )
                             .HasApiVersion( 2, 0 )
                             .AdvertisesApiVersion( 3, 0 )
                             .AdvertisesDeprecatedApiVersion( 3, 0, "Beta" );

            var actionDescriptor = configuration.Services.GetActionSelector().GetActionMapping( controllerDescriptor ).SelectMany( g => g ).Single();

            // act
            controllerBuilder.ApplyTo( controllerDescriptor );

            // assert
            actionDescriptor.GetApiVersionModel().Should().BeEquivalentTo(
                new
                {
                    IsApiVersionNeutral = false,
                    DeclaredApiVersions = new ApiVersion[0],
                    SupportedApiVersions = new[] { new ApiVersion( 2, 0 ), new ApiVersion( 3, 0 ) },
                    DeprecatedApiVersions = new[] { new ApiVersion( 0, 9 ), new ApiVersion( 3, 0, "Beta" ) },
                    ImplementedApiVersions = new[] { new ApiVersion( 0, 9 ), new ApiVersion( 2, 0 ), new ApiVersion( 3, 0 ), new ApiVersion( 3, 0, "Beta" ) }
                } );
        }

        [Fact]
        public void apply_to_should_assign_empty_conventions_to_api_version_neutral_controller()
        {
            // arrange
            var configuration = new HttpConfiguration();
            var mock = new Mock<HttpControllerDescriptor>() { CallBase = true };
            var controllerDescriptor = mock.Object;
            var controllerBuilder = default( IControllerConventionBuilder<UndecoratedController> );

            mock.Setup( cd => cd.GetCustomAttributes<IApiVersionProvider>() ).Returns( new Collection<IApiVersionProvider>() );
            controllerDescriptor.Configuration = configuration;
            controllerDescriptor.ControllerType = typeof( UndecoratedController );
            configuration.AddApiVersioning( o => controllerBuilder = o.Conventions.Controller<UndecoratedController>() );
            controllerBuilder.HasDeprecatedApiVersion( 0, 9 )
                             .HasApiVersion( 2, 0 )
                             .AdvertisesApiVersion( 3, 0 )
                             .AdvertisesDeprecatedApiVersion( 3, 0, "Beta" )
                             .IsApiVersionNeutral();

            var actionDescriptor = configuration.Services.GetActionSelector().GetActionMapping( controllerDescriptor ).SelectMany( g => g ).Single();

            // act
            controllerBuilder.ApplyTo( controllerDescriptor );

            // assert
            actionDescriptor.GetApiVersionModel().Should().BeEquivalentTo(
                new
                {
                    IsApiVersionNeutral = true,
                    DeclaredApiVersions = new ApiVersion[0],
                    SupportedApiVersions = new ApiVersion[0],
                    DeprecatedApiVersions = new ApiVersion[0],
                    ImplementedApiVersions = new ApiVersion[0]
                } );
        }

        [Fact]
        public void apply_to_should_assign_model_to_controller_from_conventions_and_attributes()
        {
            // arrange
            var configuration = new HttpConfiguration();
            var mock = new Mock<HttpControllerDescriptor>() { CallBase = true };
            var controllerDescriptor = mock.Object;
            var attributes = new Collection<IApiVersionProvider>( typeof( DecoratedController ).GetCustomAttributes().OfType<IApiVersionProvider>().ToList() );
            var controllerBuilder = default( IControllerConventionBuilder<DecoratedController> );

            mock.Setup( cd => cd.GetCustomAttributes<IApiVersionProvider>() ).Returns( attributes );
            controllerDescriptor.Configuration = configuration;
            controllerDescriptor.ControllerType = typeof( DecoratedController );
            configuration.AddApiVersioning( o => controllerBuilder = o.Conventions.Controller<DecoratedController>() );
            controllerBuilder.HasApiVersion( 1, 0 )
                             .AdvertisesApiVersion( 4, 0 );

            var actionDescriptor = configuration.Services.GetActionSelector().GetActionMapping( controllerDescriptor ).SelectMany( g => g ).Single();

            // act
            controllerBuilder.ApplyTo( controllerDescriptor );

            // assert
            actionDescriptor.GetApiVersionModel().Should().BeEquivalentTo(
                new
                {
                    IsApiVersionNeutral = false,
                    DeclaredApiVersions = new ApiVersion[0],
                    SupportedApiVersions = new[] { new ApiVersion( 1, 0 ), new ApiVersion( 2, 0 ), new ApiVersion( 3, 0 ), new ApiVersion( 4, 0 ) },
                    DeprecatedApiVersions = new[] { new ApiVersion( 0, 9 ), new ApiVersion( 3, 0, "Beta" ) },
                    ImplementedApiVersions = new[] { new ApiVersion( 0, 9 ), new ApiVersion( 1, 0 ), new ApiVersion( 2, 0 ), new ApiVersion( 3, 0 ), new ApiVersion( 3, 0, "Beta" ), new ApiVersion( 4, 0 ) }
                } );
        }

        sealed class TestControllerApiVersionConventionBuilder : ControllerApiVersionConventionBuilder<IHttpController>
        {
            internal bool ProtectedVersionNeutral => VersionNeutral;

            internal ActionApiVersionConventionBuilderCollection<IHttpController> ProtectedActionBuilders => ActionBuilders;
        }

        sealed class UndecoratedController : ApiController
        {
            public IHttpActionResult Get() => Ok();
        }

        [ApiVersion( "2.0" )]
        [ApiVersion( "0.9", Deprecated = true )]
        [AdvertiseApiVersions( "3.0" )]
        [AdvertiseApiVersions( "3.0-Beta", Deprecated = true )]
        sealed class DecoratedController : ApiController
        {
            public IHttpActionResult Get() => Ok();
        }
    }
}