// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace System.Net.Http;

using Asp.Versioning;
using Asp.Versioning.Http;
using System.Globalization;

public class HttpClientExtensionsTest
{
    [Fact]
    public async Task get_api_information_async_should_return_expected_result()
    {
        // arrange
        var date = DateTimeOffset.Now.AddMonths( 6 );
        var roundtripDate = DateTimeOffset.Parse( date.ToString( "r" ), null, DateTimeStyles.RoundtripKind );
        var response = new HttpResponseMessage();

        response.Headers.Add( "api-supported-versions", "2.0" );
        response.Headers.Add( "api-deprecated-versions", "1.0" );
        response.Headers.Add( "sunset", date.ToString( "r" ) );
        response.Headers.Add(
            "link",
            new[]
            {
                "<policy?api-version=1.0>; rel=\"sunset\"; type=\"text/html\"",
                "<swagger/v1/swagger.json>; rel=\"openapi\"; type=\"application/json\"; api-version=\"1.0\"",
            } );
        using var server = new TestServer( response );
        var client = new HttpClient( server ) { BaseAddress = new( "http://tempuri.org" ) };

        // act
        var info = await client.GetApiInformationAsync( "/?api-version=1.0" );

        // assert
        info.Should().BeEquivalentTo(
            new ApiInformation(
                new[] { new ApiVersion( 2.0 ) },
                new[] { new ApiVersion( 1.0 ) },
                new( roundtripDate, new( new( "http://tempuri.org/policy?api-version=1.0" ), "sunset" )
                {
                    Type = "text/html",
                } ),
                new Dictionary<ApiVersion, Uri>() { [new( 1.0 )] = new( "http://tempuri.org/swagger/v1/swagger.json" ) } ) );
    }
}