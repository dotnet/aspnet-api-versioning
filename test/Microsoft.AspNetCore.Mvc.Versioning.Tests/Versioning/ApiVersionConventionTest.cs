namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using ApplicationModels;
    using FluentAssertions;
    using System.Linq;
    using System.Reflection;
    using Xunit;
    using static System.Type;

    public class ApiVersionConventionTest
    {
        [Fact]
        public void convention_should_apply_api_version_model()
        {
            // arrange
            var supported = new[] { new ApiVersion( 1, 0 ), new ApiVersion( 2, 0 ), new ApiVersion( 3, 0 ) };
            var deprecated = new[] { new ApiVersion( 0, 9 ) };
            var model = new ApiVersionModel( supported, deprecated );
            var type = typeof( object );
            var attributes = new object[]
            {
                new ApiVersionAttribute( "1.0" ),
                new ApiVersionAttribute( "2.0" ),
                new ApiVersionAttribute( "3.0" ),
                new ApiVersionAttribute( "0.9" ) { Deprecated = true }
            };
            var actionMethod = type.GetRuntimeMethod( nameof( object.ToString ), EmptyTypes );
            var controller = new ControllerModel( type.GetTypeInfo(), attributes )
            {
                Actions = { new ActionModel( actionMethod, attributes ) }
            };
            var application = new ApplicationModel() { Controllers = { controller } };
            var convention = new ApiVersionConvention();

            // act
            convention.Apply( application );

            // assert
            controller.GetProperty<ApiVersionModel>().ShouldBeEquivalentTo(
                new
                {
                    IsApiVersionNeutral = false,
                    DeclaredApiVersions = deprecated.Union( supported ).ToArray(),
                    ImplementedApiVersions = deprecated.Union( supported ).ToArray(),
                    SupportedApiVersions = supported,
                    DeprecatedApiVersions = deprecated
                } );
            controller.Actions.Single().GetProperty<ApiVersionModel>().ShouldBeEquivalentTo(
                new
                {
                    IsApiVersionNeutral = false,
                    DeclaredApiVersions = deprecated.Union( supported ).ToArray(),
                    ImplementedApiVersions = deprecated.Union( supported ).ToArray(),
                    SupportedApiVersions = supported,
                    DeprecatedApiVersions = deprecated
                } );
        }

        [Fact]
        public void convention_should_apply_api_versionX2Dneutral_model()
        {
            // arrange
            var model = ApiVersionModel.Neutral;
            var type = typeof( object );
            var attributes = new object[] { new ApiVersionNeutralAttribute() };
            var actionMethod = type.GetRuntimeMethod( nameof( object.ToString ), EmptyTypes );
            var controller = new ControllerModel( type.GetTypeInfo(), attributes )
            {
                Actions = { new ActionModel( actionMethod, attributes ) }
            };
            var application = new ApplicationModel() { Controllers = { controller } };
            var convention = new ApiVersionConvention();

            // act
            convention.Apply( application );

            // assert
            controller.GetProperty<ApiVersionModel>().Should().BeSameAs( model );
            controller.Actions.Single().GetProperty<ApiVersionModel>().Should().BeSameAs( model );
        }

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
            var convention = new ApiVersionConvention( new ApiVersion( 1, 0 ) );

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
        public void convention_should_not_apply_implicit_api_version_model_to_controller_and_actions_with_explicit_api_versions()
        {
            // arrange
            var type = typeof( object );
            var attributes = new object[] { new ApiVersionAttribute( "2.0" ) };
            var actionMethod = type.GetRuntimeMethod( nameof( object.ToString ), EmptyTypes );
            var v1 = new ApiVersion( 1, 0 );
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
            var convention = new ApiVersionConvention( v1 );

            // act
            convention.Apply( application );

            // assert
            application.Controllers.Single().GetProperty<ApiVersionModel>().ImplementedApiVersions.Should().NotContain( v1 );
            application.Controllers.Single().Actions.Single().GetProperty<ApiVersionModel>().ImplementedApiVersions.Should().NotContain( v1 );
        }
    }
}