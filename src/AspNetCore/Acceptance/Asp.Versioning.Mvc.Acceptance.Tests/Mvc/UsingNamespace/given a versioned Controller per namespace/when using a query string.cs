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
public class when_using_a_query_string : AcceptanceTest
{
    [Theory]
    [InlineData( typeof( AgreementsControllerV1 ), "1.0" )]
    [InlineData( typeof( AgreementsControllerV2 ), "2.0" )]
    [InlineData( typeof( AgreementsControllerV3 ), "3.0" )]
    public async Task then_get_should_return_200( Type controllerType, string apiVersion )
    {
        // arrange
        var controller = controllerType.FullName;
        var example = new { controller = "", apiVersion = "", accountId = "" };

        // act
        var response = await GetAsync( $"api/agreements/42?api-version={apiVersion}" );
        var content = await response.EnsureSuccessStatusCode().Content.ReadAsExampleAsync( example );

        // assert
        response.Headers.GetValues( "api-supported-versions" ).Single().Should().Be( "1, 2, 3" );
        content.Should().BeEquivalentTo( new { controller, apiVersion, accountId = "42" } );
    }

    [Fact]
    public async Task then_get_should_return_400_for_an_unsupported_version()
    {
        // arrange


        // act
        var response = await GetAsync( "api/agreements/42?api-version=4.0" );

        // assert
        response.StatusCode.Should().Be( BadRequest );
    }

    [Fact]
    public async Task then_get_should_return_400_for_an_unspecified_version()
    {
        // arrange


        // act
        var response = await GetAsync( "api/agreements/42" );
        var problem = await response.Content.ReadAsProblemDetailsAsync();

        // assert
        response.StatusCode.Should().Be( BadRequest );
        problem.Type.Should().Be( ProblemDetailsDefaults.Unspecified.Type );
    }

    public when_using_a_query_string( AgreementsFixture fixture, ITestOutputHelper console )
        : base( fixture ) => console.WriteLine( fixture.DirectedGraphVisualizationUrl );
}