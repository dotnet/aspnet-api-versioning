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
    using static Microsoft.Web.Http.Versioning.ApiVersionMapping;
    using static System.Linq.Enumerable;

    public class HttpActionDescriptorExtensionsTest
    {
        [Fact]
        public void action_should_be_implicitly_versioned_when_no_api_versions_have_been_mapped()
        {
            // arrange
            var controller = new HttpControllerDescriptor();
            var action = new Mock<HttpActionDescriptor>() { CallBase = true }.Object;
            var version = new ApiVersion( 1, 0 );
            var model = new ApiVersionModel( version );

            controller.Properties[typeof( ApiVersionModel )] = model;
            action.ControllerDescriptor = controller;

            // act
            var result = action.MappingTo( version );

            // assert
            result.Should().Be( Implicit );
        }

        [Fact]
        public void implicitly_versioned_action_should_not_be_mapped_to_specific_api_version()
        {
            // arrange
            var action = new Mock<HttpActionDescriptor>() { CallBase = true }.Object;
            var version = new ApiVersion( 42, 0 );

            action.ControllerDescriptor = new HttpControllerDescriptor();

            // act
            var result = action.MappingTo( version );

            // assert
            result.Should().Be( None );
        }

        [Fact]
        public void action_should_be_mapped_to_specific_api_version()
        {
            // arrange
            var action = new Mock<HttpActionDescriptor>() { CallBase = true }.Object;
            var version = new ApiVersion( 42, 0 );
            var model = new ApiVersionModel( version );

            action.Properties[typeof( ApiVersionModel )] = model;

            // act
            var result = action.MappingTo( version );

            // assert
            result.Should().Be( Explicit );
        }

        [Fact]
        public void get_api_version_model_should_return_new_instance_for_action_descriptor()
        {
            // arrange
            var controller = new Mock<IHttpController>().Object;
            var controllerDescriptor = new HttpControllerDescriptor( new HttpConfiguration(), "Tests", controller.GetType() );
            var actionDescriptor = new Mock<HttpActionDescriptor>( controllerDescriptor ) { CallBase = true }.Object;

            actionDescriptor.Properties.Clear();

            // act
            var model = actionDescriptor.GetApiVersionModel();

            // assert
            model.Should().NotBeNull();
            actionDescriptor.Properties.ContainsKey( typeof( ApiVersionModel ) ).Should().BeFalse();
        }

        [Fact]
        public void get_api_version_model_should_return_exising_instance_for_action_descriptor()
        {
            // arrange
            var controller = new Mock<IHttpController>().Object;
            var controllerDescriptor = new HttpControllerDescriptor( new HttpConfiguration(), "Tests", controller.GetType() );
            var actionDescriptor = new Mock<HttpActionDescriptor>( controllerDescriptor ) { CallBase = true }.Object;
            var assignedModel = ApiVersionModel.Default;

            actionDescriptor.Properties[typeof( ApiVersionModel )] = assignedModel;

            // act
            var model = actionDescriptor.GetApiVersionModel();

            // assert
            model.Should().Be( assignedModel );
        }

        [Fact]
        public void is_api_neutral_should_return_false_for_undecorated_action_descriptor()
        {
            // arrange
            var controller = new Mock<IHttpController>().Object;
            var controllerDescriptor = new HttpControllerDescriptor( new HttpConfiguration(), "Tests", controller.GetType() );
            var actionDescriptor = new Mock<HttpActionDescriptor>( controllerDescriptor ) { CallBase = true }.Object;

            // act
            var versionNeutral = actionDescriptor.GetApiVersionModel().IsApiVersionNeutral;

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

            actionDescriptor.Properties[typeof( ApiVersionModel )] = ApiVersionModel.Neutral;

            // act
            var versionNeutral = actionDescriptor.GetApiVersionModel().IsApiVersionNeutral;

            // assert
            versionNeutral.Should().BeTrue();
        }

        [Theory]
        [MemberData( nameof( ApiVersionData ) )]
        public void get_api_versions_should_return_expected_action_descriptor_results( HttpActionDescriptor actionDescriptor, IEnumerable<ApiVersion> expectedVersions )
        {
            // arrange

            // act
            var declaredVersions = actionDescriptor.GetApiVersionModel().DeclaredApiVersions;

            // assert
            declaredVersions.Should().BeEquivalentTo( expectedVersions );
        }

        public static IEnumerable<object[]> ApiVersionData
        {
            get
            {
                var runs = new[]
                {
                    Tuple.Create( typeof( TestController ), nameof( TestController.Get ), new ApiVersion[0] ),
                    Tuple.Create( typeof( TestVersion2Controller ), nameof( TestVersion2Controller.Get3 ), new[] { new ApiVersion( 3, 0 ) } )
                };
                return CreateActionDescriptorData( runs );
            }
        }

        static IEnumerable<object[]> CreateActionDescriptorData( Tuple<Type, string, ApiVersion[]>[] runs )
        {
            foreach ( var run in runs )
            {
                var controllerType = run.Item1;
                var method = controllerType.GetMethod( run.Item2 );
                var expected = run.Item3;
                var controllerDescriptor = new HttpControllerDescriptor( new HttpConfiguration(), "Tests", controllerType );
                var actionDescriptor = new ReflectedHttpActionDescriptor( controllerDescriptor, method )
                {
                    Properties =
                    {
                        [typeof( ApiVersionModel )] = new ApiVersionModel( expected, Empty<ApiVersion>(), Empty<ApiVersion>(), Empty<ApiVersion>() ),
                    },
                };
                yield return new object[] { actionDescriptor, expected };
            }
        }
    }
}