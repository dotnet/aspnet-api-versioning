namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using ApplicationModels;
    using FluentAssertions;
    using System.Linq;
    using System.Reflection;
    using Xunit;
    using static System.Type;

    public class ImplicitControllerVersionConventionTest
    {
        [Fact]
        public void convention_should_apply_implicit_api_version_model()
        {
            // arrange
            var type = typeof( object );
            var attributes = new object[0];
            var actionMethod = type.GetRuntimeMethod( nameof( object.ToString ), EmptyTypes );
            var application = new ApplicationModel()
            {
                Controllers =
                {
                    new ControllerModel( type.GetTypeInfo(), attributes )
                    {
                        Actions =
                        {
                            new ActionModel( actionMethod, attributes )
                        }
                    }
                }
            };
            var convention = new ImplicitControllerVersionConvention( new ApiVersion( 1, 0 ) );

            // act
            convention.Apply( application );

            // assert
            application.Controllers.Single().GetProperty<ApiVersionModel>().ShouldBeEquivalentTo(
                new
                {
                    IsApiVersionNeutral = false,
                    DeclaredApiVersions = new[] { new ApiVersion( 1, 0 ) },
                    ImplementedApiVersions = new[] { new ApiVersion( 1, 0 ) },
                    SupportedApiVersions = new[] { new ApiVersion( 1, 0 ) },
                    DeprecatedApiVersions = new ApiVersion[0],
                } );
            application.Controllers.Single().Actions.Single().GetProperty<ApiVersionModel>().ShouldBeEquivalentTo(
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
        public void convention_should_not_apply_to_controller_and_actions_with_explicit_api_versions()
        {
            // arrange
            var type = typeof( object );
            var attributes = new object[] { new ApiVersionAttribute( "2.0" ) };
            var actionMethod = type.GetRuntimeMethod( nameof( object.ToString ), EmptyTypes );
            var application = new ApplicationModel()
            {
                Controllers =
                {
                    new ControllerModel( type.GetTypeInfo(), attributes )
                    {
                        Actions =
                        {
                            new ActionModel( actionMethod, attributes )
                        }
                    }
                }
            };
            var convention = new ImplicitControllerVersionConvention( new ApiVersion( 1, 0 ) );

            // act
            convention.Apply( application );

            // assert
            application.Controllers.Single().GetProperty<ApiVersionModel>().Should().BeNull();
            application.Controllers.Single().Actions.Single().GetProperty<ApiVersionModel>().Should().BeNull();
        }
    }
}
