namespace System.Web.Http
{
    using Collections.Generic;
    using Controllers;
    using FluentAssertions;
    using Microsoft.Web.Http;
    using Microsoft.Web.Http.Simulators;
    using Microsoft.Web.Http.Versioning;
    using Moq;
    using System;
    using Xunit;

    public class HttpActionDescriptorExtensionsTest
    {
        private static IEnumerable<object[]> CreateActionDescriptorData( Tuple<Type, string, ApiVersion[]>[] runs )
        {
            foreach ( var run in runs )
            {
                var controllerType = run.Item1;
                var method = controllerType.GetMethod( run.Item2 );
                var expected = run.Item3;
                var controllerDescriptor = new HttpControllerDescriptor( new HttpConfiguration(), "Tests", controllerType );
                var actionDescriptor = new ReflectedHttpActionDescriptor( controllerDescriptor, method );
                yield return new object[] { actionDescriptor, expected };
            }
        }

        public static IEnumerable<object[]> ApiVersionData
        {
            get
            {
                var runs = new[]
                {
                    Tuple.Create( typeof( TestController ), nameof( TestController.Get ), new ApiVersion[0] ),
                    Tuple.Create( typeof( TestVersion2Controller ), nameof( TestVersion2Controller.Get3 ), new[] { new ApiVersion( 3, 0 ), new ApiVersion( 3, 0, "Alpha" ) } )
                };
                return CreateActionDescriptorData( runs );
            }
        }

        [Fact]
        public void get_api_version_info_should_add_and_return_new_instance_for_action_descriptor()
        {
            // arrange
            var controller = new Mock<IHttpController>().Object;
            var controllerDescriptor = new HttpControllerDescriptor( new HttpConfiguration(), "Tests", controller.GetType() );
            var actionDescriptor = new Mock<HttpActionDescriptor>( controllerDescriptor ) { CallBase = true }.Object;

            actionDescriptor.Properties.Clear();

            // act
            var versionInfo = actionDescriptor.GetApiVersionModel();

            // assert
            versionInfo.Should().NotBeNull();
            actionDescriptor.Properties.ContainsKey( "MS_ApiVersionInfo" ).Should().BeTrue();
        }

        [Fact]
        public void get_api_version_info_should_returne_exising_instance_for_action_descriptor()
        {
            // arrange
            var controller = new Mock<IHttpController>().Object;
            var controllerDescriptor = new HttpControllerDescriptor( new HttpConfiguration(), "Tests", controller.GetType() );
            var actionDescriptor = new Mock<HttpActionDescriptor>( controllerDescriptor ) { CallBase = true }.Object;
            var assignedVersionInfo = ApiVersionModel.Default;

            actionDescriptor.Properties["MS_ApiVersionInfo"] = assignedVersionInfo;

            // act
            var versionInfo = actionDescriptor.GetApiVersionModel();

            // assert
            versionInfo.Should().Be( assignedVersionInfo );
        }

        [Fact]
        public void is_api_neutral_should_return_false_for_undecorated_action_descriptor()
        {
            // arrange
            var controller = new Mock<IHttpController>().Object;
            var controllerDescriptor = new HttpControllerDescriptor( new HttpConfiguration(), "Tests", controller.GetType() );
            var actionDescriptor = new Mock<HttpActionDescriptor>( controllerDescriptor ) { CallBase = true }.Object;

            // act
            var versionNeutral = actionDescriptor.IsApiVersionNeutral();

            // assert
            versionNeutral.Should().BeFalse();
        }

        [Fact]
        public void is_api_neutral_should_return_true_for_decorated_action_descriptor()
        {
            // arrange
            var controller = new TestVersionNeutralController();
            var controllerDescriptor = new HttpControllerDescriptor( new HttpConfiguration(), "Tests", controller.GetType() );
            var actionDescriptor = new Mock<HttpActionDescriptor>( controllerDescriptor ) { CallBase = true }.Object;

            // act
            var versionNeutral = actionDescriptor.IsApiVersionNeutral();

            // assert
            versionNeutral.Should().BeTrue();
        }

        [Theory]
        [MemberData( nameof( ApiVersionData ) )]
        public void get_api_versions_should_return_expected_action_descriptor_results( HttpActionDescriptor actionDescriptor, IEnumerable<ApiVersion> expectedVersions )
        {
            // arrange

            // act
            var declaredVersions = actionDescriptor.GetApiVersions();

            // assert
            declaredVersions.Should().BeEquivalentTo( expectedVersions );
        }
    }
}
