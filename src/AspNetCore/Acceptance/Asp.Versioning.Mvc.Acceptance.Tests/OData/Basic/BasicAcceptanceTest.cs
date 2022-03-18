// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OData.Basic;

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

        // assert
        response.StatusCode.Should().Be( NotFound );
    }

    [Fact]
    public async Task then_X24metadata_should_return_404_for_unsupported_url_api_version()
    {
        // arrange

        // act
        var response = await GetAsync( "v4/$metadata" );

        // assert
        response.StatusCode.Should().Be( NotFound );
    }

    protected BasicAcceptanceTest( ODataFixture fixture ) : base( fixture ) { }
}