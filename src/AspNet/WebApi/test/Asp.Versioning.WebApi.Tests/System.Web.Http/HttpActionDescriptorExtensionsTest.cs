// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0130

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
    public void get_api_versions_should_return_expected_action_descriptor_results( Type controllerType, string actionName, ApiVersion[] expectedVersions )
    {
        // arrange
        var actionDescriptor = NewAction( controllerType, actionName, expectedVersions );

        // act
        var declaredVersions = actionDescriptor.GetApiVersionMetadata().Map( Explicit ).DeclaredApiVersions;

        // assert
        declaredVersions.Should().BeEquivalentTo( expectedVersions );
    }

    private static HttpActionDescriptor NewAction( Type controllerType, string methodName, ApiVersion[] expected )
    {
        var method = controllerType.GetMethod( methodName );
        var metadata = new ApiVersionMetadata(
            ApiVersionModel.Empty,
            new ApiVersionModel( expected, [], [], [] ) );
        var controllerDescriptor = new HttpControllerDescriptor( new HttpConfiguration(), "Tests", controllerType );

        return new ReflectedHttpActionDescriptor( controllerDescriptor, method )
        {
            Properties = { [typeof( ApiVersionMetadata )] = metadata },
        };
    }

    public static TheoryData<Type, string, ApiVersion[]> ApiVersionData => new()
    {
        { typeof( TestController ), nameof( TestController.Get ), [] },
        { typeof( TestVersion2Controller ), nameof( TestVersion2Controller.Get3 ), [new(3, 0)] },
    };
}