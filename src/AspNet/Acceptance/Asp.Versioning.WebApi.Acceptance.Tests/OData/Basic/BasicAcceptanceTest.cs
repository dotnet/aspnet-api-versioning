// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OData.Basic;

using Asp.Versioning.OData;
using static System.Net.HttpStatusCode;

[Collection( "OData" + nameof( BasicTestCollection ) )]
public abstract class BasicAcceptanceTest : ODataAcceptanceTest
{
    [Fact]
    public async Task then_service_document_should_return_404_for_unsupported_url_api_version()
    {
        // arrange
        var requestUrl = "v4";

        // act
        var response = await GetAsync( requestUrl );
        var problem = await response.Content.ReadAsProblemDetailsAsync();

        // assert
        response.StatusCode.Should().Be( NotFound );
        problem.Type.Should().Be( ProblemDetailsDefaults.Unsupported.Type );
    }

    [Theory]
    [InlineData( "?additionalQuery=true" )]
    [InlineData( "?additionalQuery=true#anchor-123" )]
    [InlineData( "#anchor-123" )]
    public async Task then_the_service_document_should_return_only_path_for_an_unsupported_version( string additionalUriPart )
    {
        // arrange
        var requestUrl = $"v4{additionalUriPart}";

        // act
        var response = await GetAsync( requestUrl );
        var problem = await response.Content.ReadAsProblemDetailsAsync();


        // assert
        response.StatusCode.Should().Be( NotFound );
        problem.Type.Should().Be( ProblemDetailsDefaults.Unsupported.Type );
        problem.Detail.Should().Contain( "v4" );
        problem.Detail.Should().NotContain( additionalUriPart );
    }

    [Fact]
    public async Task then_X24metadata_should_return_404_for_unsupported_url_api_version()
    {
        // arrange
        Client.DefaultRequestHeaders.Clear();

        // act
        var response = await Client.GetAsync( "v4/$metadata" );
        var problem = await response.Content.ReadAsProblemDetailsAsync();

        // assert
        response.StatusCode.Should().Be( NotFound );
        problem.Type.Should().Be( ProblemDetailsDefaults.Unsupported.Type );
    }

    protected BasicAcceptanceTest( BasicFixture fixture ) : base( fixture ) { }
}