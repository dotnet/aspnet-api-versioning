// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace System.Web.Http;

using Asp.Versioning;
using Asp.Versioning.Simulators;
using System.Web.Http.Controllers;
using static Asp.Versioning.ApiVersionMapping;

public class HttpActionDescriptorExtensionsTest
{
    [Fact]
    public void get_api_version_metadata_should_return_new_instance_for_action_descriptor()
    {
        // arrange
        var controller = new Mock<IHttpController>().Object;
        var controllerDescriptor = new HttpControllerDescriptor( new HttpConfiguration(), "Tests", controller.GetType() );
        var actionDescriptor = new Mock<HttpActionDescriptor>( controllerDescriptor ) { CallBase = true }.Object;

        actionDescriptor.Properties.Clear();

        // act
        var model = actionDescriptor.GetApiVersionMetadata();

        // assert
        model.Should().NotBeNull();
        actionDescriptor.Properties.ContainsKey( typeof( ApiVersionMetadata ) ).Should().BeFalse();
    }

    [Fact]
    public void get_api_version_metadata_should_return_existing_instance_for_action_descriptor()
    {
        // arrange
        var controller = new Mock<IHttpController>().Object;
        var controllerDescriptor = new HttpControllerDescriptor( new HttpConfiguration(), "Tests", controller.GetType() );
        var actionDescriptor = new Mock<HttpActionDescriptor>( controllerDescriptor ) { CallBase = true }.Object;
        var endpointModel = ApiVersionModel.Default;

        actionDescriptor.Properties[typeof( ApiVersionMetadata )] = new ApiVersionMetadata( ApiVersionModel.Empty, endpointModel );

        // act
        var model = actionDescriptor.GetApiVersionMetadata().Map( Explicit );

        // assert
        model.Should().Be( endpointModel );
    }

    [Fact]
    public void is_api_neutral_should_return_false_for_undecorated_action_descriptor()
    {
        // arrange
        var controller = new Mock<IHttpController>().Object;
        var controllerDescriptor = new HttpControllerDescriptor( new HttpConfiguration(), "Tests", controller.GetType() );
        var actionDescriptor = new Mock<HttpActionDescriptor>( controllerDescriptor ) { CallBase = true }.Object;

        // act
        var versionNeutral = actionDescriptor.GetApiVersionMetadata().IsApiVersionNeutral;

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

        actionDescriptor.Properties[typeof( ApiVersionMetadata )] = ApiVersionMetadata.Neutral;

        // act
        var versionNeutral = actionDescriptor.GetApiVersionMetadata().IsApiVersionNeutral;

        // assert
        versionNeutral.Should().BeTrue();
    }

    [Theory]
    [MemberData( nameof( ApiVersionData ) )]
    public void get_api_versions_should_return_expected_action_descriptor_results( HttpActionDescriptor actionDescriptor, IEnumerable<ApiVersion> expectedVersions )
    {
        // arrange

        // act
        var declaredVersions = actionDescriptor.GetApiVersionMetadata().Map( Explicit ).DeclaredApiVersions;

        // assert
        declaredVersions.Should().BeEquivalentTo( expectedVersions );
    }

    public static IEnumerable<object[]> ApiVersionData
    {
        get
        {
            var runs = new[]
            {
                Tuple.Create( typeof( TestController ), nameof( TestController.Get ), Array.Empty<ApiVersion>() ),
                Tuple.Create( typeof( TestVersion2Controller ), nameof( TestVersion2Controller.Get3 ), new[] { new ApiVersion( 3, 0 ) } ),
            };
            return CreateActionDescriptorData( runs );
        }
    }

    private static IEnumerable<object[]> CreateActionDescriptorData( Tuple<Type, string, ApiVersion[]>[] runs )
    {
        foreach ( var run in runs )
        {
            var controllerType = run.Item1;
            var method = controllerType.GetMethod( run.Item2 );
            var expected = run.Item3;
            var metadata = new ApiVersionMetadata(
                ApiVersionModel.Empty,
                new ApiVersionModel(
                    expected,
                    Enumerable.Empty<ApiVersion>(),
                    Enumerable.Empty<ApiVersion>(),
                    Enumerable.Empty<ApiVersion>() ) );
            var controllerDescriptor = new HttpControllerDescriptor( new HttpConfiguration(), "Tests", controllerType );
            var actionDescriptor = new ReflectedHttpActionDescriptor( controllerDescriptor, method )
            {
                Properties = { [typeof( ApiVersionMetadata )] = metadata },
            };
            yield return new object[] { actionDescriptor, expected };
        }
    }
}