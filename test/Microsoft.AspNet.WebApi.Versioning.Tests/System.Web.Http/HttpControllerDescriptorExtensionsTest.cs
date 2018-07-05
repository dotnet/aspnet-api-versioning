namespace System.Web.Http
{
    using Collections.Generic;
    using Controllers;
    using FluentAssertions;
    using Microsoft.Web.Http;
    using Microsoft.Web.Http.Simulators;
    using Microsoft.Web.Http.Versioning;
    using Moq;
    using Xunit;

    public class HttpControllerDescriptorExtensionsTest
    {
        public static IEnumerable<object[]> DeclaredApiVersionData
        {
            get
            {
                yield return new object[]
                {
                    new HttpControllerDescriptor( new HttpConfiguration(), "Tests", typeof( TestController ) ),
                    new[] { ApiVersion.Default }
                };
                yield return new object[]
                {
                    new HttpControllerDescriptor( new HttpConfiguration(), "Tests", typeof( TestVersion2Controller ) ),
                    new[] { new ApiVersion( 1, 8 ), new ApiVersion( 1, 9 ), new ApiVersion( 2, 0 ), new ApiVersion( 3, 0 ) }
                };
                yield return new object[]
                {
                    new HttpControllerDescriptor( new HttpConfiguration(), "Tests", typeof( AttributeRoutedTest4Controller ) ),
                    new[] { new ApiVersion( 4, 0 ) }
                };
            }
        }

        public static IEnumerable<object[]> SupportedApiVersionData
        {
            get
            {
                yield return new object[]
                {
                    new HttpControllerDescriptor( new HttpConfiguration(), "Tests", typeof( TestController ) ),
                    new[] { ApiVersion.Default }
                };
                yield return new object[]
                {
                    new HttpControllerDescriptor( new HttpConfiguration(), "Tests", typeof( TestVersion2Controller ) ),
                    new[] { new ApiVersion( 2, 0 ), new ApiVersion( 3, 0 ) }
                };
                yield return new object[]
                {
                    new HttpControllerDescriptor( new HttpConfiguration(), "Tests", typeof( AttributeRoutedTest4Controller ) ),
                    new[] { new ApiVersion( 1, 0 ), new ApiVersion( 2, 0 ), new ApiVersion( 3, 0 ), new ApiVersion( 4, 0 ) }
                };
            }
        }

        public static IEnumerable<object[]> DeprecatedApiVersionData
        {
            get
            {
                yield return new object[]
                {
                    new HttpControllerDescriptor( new HttpConfiguration(), "Tests", typeof( TestController ) ),
                    new ApiVersion[0]
                };
                yield return new object[]
                {
                    new HttpControllerDescriptor( new HttpConfiguration(), "Tests", typeof( TestVersion2Controller ) ),
                    new[] { new ApiVersion( 1, 8 ), new ApiVersion( 1, 9 ) }
                };
                yield return new object[]
                {
                    new HttpControllerDescriptor( new HttpConfiguration(), "Tests", typeof( AttributeRoutedTest4Controller ) ),
                    new[] { new ApiVersion( 3, 0, "Alpha" ) }
                };
            }
        }

        [Fact]
        public void get_api_version_info_should_add_and_return_new_instance_for_controller_descriptor()
        {
            // arrange
            var controller = new Mock<IHttpController>().Object;
            var controllerDescriptor = new HttpControllerDescriptor( new HttpConfiguration(), "Tests", controller.GetType() );

            controllerDescriptor.Properties.Clear();

            // act
            var versionInfo = controllerDescriptor.GetApiVersionModel();

            // assert
            versionInfo.Should().NotBeNull();
        }

        [Fact]
        public void get_api_version_info_should_returne_exising_instance_for_controller_descriptor()
        {
            // arrange
            var controller = new Mock<IHttpController>().Object;
            var controllerDescriptor = new HttpControllerDescriptor( new HttpConfiguration(), "Tests", controller.GetType() );
            var assignedVersionInfo = ApiVersionModel.Default;

            controllerDescriptor.Properties["MS_ApiVersionInfo"] = assignedVersionInfo;

            // act
            var versionInfo = controllerDescriptor.GetApiVersionModel();

            // assert
            versionInfo.Should().Be( assignedVersionInfo );
        }

        [Fact]
        public void is_api_neutral_should_return_false_for_undecorated_controller_descriptor()
        {
            // arrange
            var controller = new Mock<IHttpController>().Object;
            var controllerDescriptor = new HttpControllerDescriptor( new HttpConfiguration(), "Tests", controller.GetType() );

            // act
            var versionNeutral = controllerDescriptor.IsApiVersionNeutral();

            // assert
            versionNeutral.Should().BeFalse();
        }

        [Fact]
        public void is_api_neutral_should_return_true_for_decorated_controller_descriptor()
        {
            // arrange
            var controller = new TestVersionNeutralController();
            var controllerDescriptor = new HttpControllerDescriptor( new HttpConfiguration(), "Tests", controller.GetType() );

            // actd
            var versionNeutral = controllerDescriptor.IsApiVersionNeutral();

            // assert
            versionNeutral.Should().BeTrue();
        }

        [Fact]
        public void versionX2Dneutral_controller_should_not_have_any_version_info()
        {
            // arrange
            var controller = new TestVersionNeutralController();
            var controllerDescriptor = new HttpControllerDescriptor( new HttpConfiguration(), "Tests", controller.GetType() );
            IReadOnlyList<ApiVersion> emptyVersions = new ApiVersion[0];

            // act
            var versionInfo = controllerDescriptor.GetApiVersionModel();

            // assert
            versionInfo.Should().BeEquivalentTo(
                new
                {
                    IsApiVersionNeutral = true,
                    DeclaredApiVersions = emptyVersions,
                    ImplementedApiVersions = emptyVersions,
                    SupportedApiVersions = emptyVersions,
                    DeprecatedApiVersions = emptyVersions,
                } );
        }

        [Theory]
        [MemberData( nameof( DeclaredApiVersionData ) )]
        public void get_declared_api_versions_should_return_expected_controller_descriptor_results( HttpControllerDescriptor controllerDescriptor, IEnumerable<ApiVersion> expectedVersions )
        {
            // arrange

            // act
            var declaredVersions = controllerDescriptor.GetDeclaredApiVersions();

            // assert
            declaredVersions.Should().BeEquivalentTo( expectedVersions );
        }

        [Theory]
        [MemberData( nameof( DeclaredApiVersionData ) )]
        public void get_implemented_api_versions_should_return_expected_controller_descriptor_results( HttpControllerDescriptor controllerDescriptor, IEnumerable<ApiVersion> expectedVersions )
        {
            // arrange

            // act
            var implementedVersions = controllerDescriptor.GetImplementedApiVersions();

            // assert
            implementedVersions.Should().BeEquivalentTo( expectedVersions );
        }

        [Theory]
        [MemberData( nameof( SupportedApiVersionData ) )]
        public void get_supported_api_versions_should_return_expected_controller_descriptor_results( HttpControllerDescriptor controllerDescriptor, IEnumerable<ApiVersion> expectedVersions )
        {
            // arrange

            // act
            var supportedVersions = controllerDescriptor.GetSupportedApiVersions();

            // assert
            supportedVersions.Should().BeEquivalentTo( expectedVersions );
        }

        [Theory]
        [MemberData( nameof( DeprecatedApiVersionData ) )]
        public void get_deprecated_api_versions_should_return_expected_controller_descriptor_results( HttpControllerDescriptor controllerDescriptor, IEnumerable<ApiVersion> expectedVersions )
        {
            // arrange

            // act
            var deprecatedVersions = controllerDescriptor.GetDeprecatedApiVersions();

            // assert
            deprecatedVersions.Should().BeEquivalentTo( expectedVersions );
        }
    }
}