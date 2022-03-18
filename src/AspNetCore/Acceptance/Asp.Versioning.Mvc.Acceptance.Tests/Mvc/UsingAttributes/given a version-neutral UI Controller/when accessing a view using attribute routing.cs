// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace given_a_versionX2Dneutral_UI_Controller;

using Asp.Versioning;
using Asp.Versioning.Mvc.UsingAttributes;
using Asp.Versioning.Mvc.UsingAttributes.Controllers.WithViewsUsingAttributes;
using System.Net.Http.Headers;
using static System.Net.Http.HttpMethod;

public class when_accessing_a_view_using_attribute_routing : AcceptanceTest, IClassFixture<UIFixture>
{
    [Theory]
    [InlineData( "http://localhost" )]
    [InlineData( "http://localhost/home" )]
    [InlineData( "http://localhost/home/index" )]
    [InlineData( "http://localhost/index" )]
    public async Task then_get_should_return_200( string requestUrl )
    {
        // arrange
        using var request = new HttpRequestMessage( Get, requestUrl )
        {
            Headers = { Accept = { new MediaTypeWithQualityHeaderValue( "text/html" ) } },
        };

        // act
        var response = await Client.SendAsync( request );
        var mediaType = response.EnsureSuccessStatusCode().Content.Headers.ContentType.MediaType;

        // assert
        mediaType.Should().Be( "text/html" );
    }

    public when_accessing_a_view_using_attribute_routing( UIFixture fixture, ITestOutputHelper console ) : base( fixture )
    {
        fixture.FilteredControllerTypes.Add( typeof( HomeController ) );
        console.WriteLine( fixture.DirectedGraphVisualizationUrl );
    }
}