namespace Microsoft.AspNetCore.Mvc.Versioning.Conventions
{
    using ApplicationModels;
    using FluentAssertions;
    using System.Linq;
    using System.Reflection;
    using Xunit;

    public class ControllerApiVersionConventionBuilderTTest
    {
        private sealed class UndecoratedController : Controller
        {
            public IActionResult Get() => Ok();
        }

        [ApiVersion( "2.0" )]
        [ApiVersion( "0.9", Deprecated = true )]
        [AdvertiseApiVersions( "3.0" )]
        [AdvertiseApiVersions( "3.0-Beta", Deprecated = true )]
        private sealed class DecoratedController : Controller
        {
            public IActionResult Get() => Ok();
        }

        [Fact]
        public void apply_to_should_assign_conventions_to_controller()
        {
            // arrange
            var controllerModel = new ControllerModel( typeof( UndecoratedController ).GetTypeInfo(), new object[0] );
            var controllerBuilder = new ControllerApiVersionConventionBuilder<UndecoratedController>();

            controllerBuilder.HasDeprecatedApiVersion( 0, 9 )
                             .HasApiVersion( 2, 0 )
                             .AdvertisesApiVersion( 3, 0 )
                             .AdvertisesDeprecatedApiVersion( 3, 0, "Beta" );

            // act
            controllerBuilder.ApplyTo( controllerModel );

            // assert
            controllerModel.GetProperty<ApiVersionModel>().ShouldBeEquivalentTo(
                new
                {
                    IsApiVersionNeutral = false,
                    DeclaredApiVersions = new[] { new ApiVersion( 0, 9 ), new ApiVersion( 2, 0 ) },
                    SupportedApiVersions = new[] { new ApiVersion( 2, 0 ), new ApiVersion( 3, 0 ) },
                    DeprecatedApiVersions = new[] { new ApiVersion( 0, 9 ), new ApiVersion( 3, 0, "Beta" ) },
                    ImplementedApiVersions = new[] { new ApiVersion( 0, 9 ), new ApiVersion( 2, 0 ), new ApiVersion( 3, 0 ), new ApiVersion( 3, 0, "Beta" ) }
                } );
        }

        [Fact]
        public void apply_to_should_assign_empt_conventions_to_api_version_neutral_controller()
        {
            // arrange
            var controllerModel = new ControllerModel( typeof( UndecoratedController ).GetTypeInfo(), new object[0] );
            var controllerBuilder = new ControllerApiVersionConventionBuilder<UndecoratedController>();

            controllerBuilder.HasDeprecatedApiVersion( 0, 9 )
                             .HasApiVersion( 2, 0 )
                             .AdvertisesApiVersion( 3, 0 )
                             .AdvertisesDeprecatedApiVersion( 3, 0, "Beta" )
                             .IsApiVersionNeutral();

            // act
            controllerBuilder.ApplyTo( controllerModel );

            // assert
            controllerModel.GetProperty<ApiVersionModel>().ShouldBeEquivalentTo(
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
            var attributes = typeof( DecoratedController ).GetTypeInfo().GetCustomAttributes().Cast<object>().ToArray();
            var controllerModel = new ControllerModel( typeof( DecoratedController ).GetTypeInfo(), attributes );
            var controllerBuilder = new ControllerApiVersionConventionBuilder<UndecoratedController>();

            controllerBuilder.HasApiVersion( 1, 0 )
                             .AdvertisesApiVersion( 4, 0 );

            // act
            controllerBuilder.ApplyTo( controllerModel );

            // assert
            controllerModel.GetProperty<ApiVersionModel>().ShouldBeEquivalentTo(
                new
                {
                    IsApiVersionNeutral = false,
                    DeclaredApiVersions = new[] { new ApiVersion( 0, 9 ), new ApiVersion( 1, 0 ), new ApiVersion( 2, 0 ) },
                    SupportedApiVersions = new[] { new ApiVersion( 1, 0 ), new ApiVersion( 2, 0 ), new ApiVersion( 3, 0 ), new ApiVersion( 4, 0 ) },
                    DeprecatedApiVersions = new[] { new ApiVersion( 0, 9 ), new ApiVersion( 3, 0, "Beta" ) },
                    ImplementedApiVersions = new[] { new ApiVersion( 0, 9 ), new ApiVersion( 1, 0 ), new ApiVersion( 2, 0 ), new ApiVersion( 3, 0 ), new ApiVersion( 3, 0, "Beta" ), new ApiVersion( 4, 0 ) }
                } );
        }
    }
}
