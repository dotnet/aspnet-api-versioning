// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace given_a_versioned_Controller_per_namespace;

using Asp.Versioning;
using Asp.Versioning.Mvc.UsingNamespace;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using static System.Net.HttpStatusCode;
using AgreementsControllerV1 = Asp.Versioning.Mvc.UsingNamespace.Controllers.V1.AgreementsController;
using AgreementsControllerV2 = Asp.Versioning.Mvc.UsingNamespace.Controllers.V2.AgreementsController;
using AgreementsControllerV3 = Asp.Versioning.Mvc.UsingNamespace.Controllers.V3.AgreementsController;

[Collection( nameof( AgreementsTestCollection ) )]
public class when_using_a_url_segment : AcceptanceTest
{
    [Theory]
    [InlineData( typeof( AgreementsControllerV1 ), "1" )]
    [InlineData( typeof( AgreementsControllerV2 ), "2" )]
    [InlineData( typeof( AgreementsControllerV3 ), "3" )]
    public async Task then_get_should_return_200( Type controllerType, string apiVersion )
    {
        // arrange
        var controller = controllerType.FullName;
        var example = new { controller = "", apiVersion = "", accountId = "" };

        // act
        var response = await GetAsync( $"v{apiVersion}/agreements/42" );
        var content = await response.EnsureSuccessStatusCode().Content.ReadAsExampleAsync( example );

        // assert
        response.Headers.GetValues( "api-supported-versions" ).Single().Should().Be( "1, 2, 3" );
        content.Should().BeEquivalentTo( new { controller, apiVersion, accountId = "42" } );
    }

    [Fact]
    public async Task then_get_should_return_404_for_an_unsupported_version()
    {
        // arrange


        // act
        var response = await GetAsync( "v4/agreements/42" );

        // assert
        response.StatusCode.Should().Be( NotFound );
    }

    public when_using_a_url_segment( AgreementsFixture fixture, ITestOutputHelper console )
        : base( fixture ) => console.WriteLine( fixture.DirectedGraphVisualizationUrl );
}