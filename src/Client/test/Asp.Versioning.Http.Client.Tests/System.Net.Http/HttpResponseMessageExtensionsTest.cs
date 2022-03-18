// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace System.Net.Http;

using Asp.Versioning;

public class HttpResponseMessageExtensionsTest
{
    [Fact]
    public void read_sunset_policy_should_parse_response()
    {
        // arrange
        var date = DateTimeOffset.Now;
        var request = new HttpRequestMessage( HttpMethod.Get, "http://tempuri.org" );
        var response = new HttpResponseMessage() { RequestMessage = request };

        response.Headers.Add( "sunset", date.ToString( "r" ) );
        response.Headers.Add( "link", "<policy>; rel=\"sunset\"; type=\"text/html\"" );

        // act
        var policy = response.ReadSunsetPolicy();

        // assert
        policy.Date.Value.ToLocalTime().Should().BeCloseTo( date, TimeSpan.FromMinutes( 1d ) );
        policy.Links.Single().Should().BeEquivalentTo(
            new LinkHeaderValue(
                new Uri( "http://tempuri.org/policy" ),
                "sunset" )
            {
                Type = "text/html",
            } );
    }

    [Fact]
    public void read_sunset_policy_should_use_greatest_date()
    {
        // arrange
        var date = DateTimeOffset.Now;
        var expected = date.AddDays( 14 );
        var request = new HttpRequestMessage( HttpMethod.Get, "http://tempuri.org" );
        var response = new HttpResponseMessage() { RequestMessage = request };

        response.Headers.Add(
            "sunset",
            new string[]
            {
                date.ToString( "r" ),
                expected.ToString( "r" ),
                date.AddDays( 3 ).ToString( "r" ),
            } );

        // act
        var policy = response.ReadSunsetPolicy();

        // assert
        policy.Date.Value.ToLocalTime().Should().BeCloseTo( expected, TimeSpan.FromMinutes( 1d ) );
        policy.HasLinks.Should().BeFalse();
    }

    [Fact]
    public void read_sunset_policy_should_ignore_unrelated_links()
    {
        // arrange
        var request = new HttpRequestMessage( HttpMethod.Get, "http://tempuri.org" );
        var response = new HttpResponseMessage() { RequestMessage = request };

        response.Headers.Add(
            "link",
            new[]
            {
                "<swagger.json>; rel=\"openapi\"; type=\"application/json\" title=\"OpenAPI\"",
                "<policy>; rel=\"sunset\"; type=\"text/html\"",
                "<docs>; rel=\"info\"; type=\"text/html\" title=\"Documentation\"",
            } );

        // act
        var policy = response.ReadSunsetPolicy();

        // assert
        policy.Date.Should().BeNull();
        policy.Links.Single().Should().BeEquivalentTo(
            new LinkHeaderValue(
                new Uri( "http://tempuri.org/policy" ),
                "sunset" )
            {
                Type = "text/html",
            } );
    }

    [Fact]
    public void get_open_api_document_urls_should_return_expected_values()
    {
        // arrange
        var request = new HttpRequestMessage( HttpMethod.Get, "http://tempuri.org" );
        var response = new HttpResponseMessage() { RequestMessage = request };

        response.Headers.Add(
            "link",
            new[]
            {
                "<swagger/swagger.json>; rel=\"openapi\"; type=\"application/json\" title=\"OpenAPI\"",
                "<policy>; rel=\"sunset\"; type=\"text/html\"",
                "<docs>; rel=\"info\"; type=\"text/html\" title=\"Documentation\"",
                "<swagger/v1/swagger.json>; rel=\"swagger\"; type=\"application/json\"; api-version=\"1.0\"",
            } );

        // act
        var urls = response.GetOpenApiDocumentUrls();

        // assert
        urls.Should().BeEquivalentTo(
            new Dictionary<ApiVersion, Uri>()
            {
                [new( 1.0 )] = new( "http://tempuri.org/swagger/v1/swagger.json" ),
                [ApiVersion.Neutral] = new( "http://tempuri.org/swagger/swagger.json" ),
            } );
    }
}