// Copyright (c) .NET Foundation and contributors. All rights reserved.

//// Ignore Spelling: Dneutral

namespace Asp.Versioning.Routing;

using Microsoft.AspNet.OData;
using System.Web.Http;
using System.Web.Http.Controllers;

public class VersionedAttributeRoutingConventionTest
{
    [Fact]
    public void should_map_controller_should_return_true_for_versionX2Dneutral_controller()
    {
        // arrange
        var configuration = new HttpConfiguration();
        var controller = new HttpControllerDescriptor( configuration, string.Empty, typeof( NeutralController ) );
        var convention = new VersionedAttributeRoutingConvention( "Tests", configuration );

        controller.Properties[typeof( ApiVersionModel )] = ApiVersionModel.Neutral;

        // act
        var result = convention.ShouldMapController( controller, new ApiVersion( 1, 0 ) );

        // assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData( 1 )]
    [InlineData( 2 )]
    public void should_map_controller_should_return_expected_result_for_controller_version( int majorVersion )
    {
        // arrange
        var configuration = new HttpConfiguration();
        var controller = new HttpControllerDescriptor( configuration, string.Empty, typeof( ControllerV1 ) );
        var convention = new VersionedAttributeRoutingConvention( "Tests", configuration );

        controller.Properties[typeof( ApiVersionModel )] = new ApiVersionModel( new ApiVersion( majorVersion, 0 ) );

        // act
        var result = convention.ShouldMapController( controller, new ApiVersion( majorVersion, 0 ) );

        // assert
        result.Should().BeTrue();
    }

    [ApiVersionNeutral]
    private sealed class NeutralController : ODataController { }

    [ApiVersion( "1.0" )]
    private sealed class ControllerV1 : ODataController { }
}