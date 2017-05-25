namespace given_a_versionX2Dneutral_UI_Controller
{
    using FluentAssertions;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Basic;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Xunit;

    public class when_accessing_a_view : BasicAcceptanceTest
    {
        [Theory]
        [InlineData( "http://localhost" )]
        [InlineData( "http://localhost/home" )]
        [InlineData( "http://localhost/home/index" )]
        public async Task then_get_should_return_200( string requestUrl )
        {
            // arrange
            Client.DefaultRequestHeaders.Accept.Add( new MediaTypeWithQualityHeaderValue( "text/html" ) );

            // act
            var response = await GetAsync( requestUrl ).EnsureSuccessStatusCode();

            // assert
            response.Content.Headers.ContentType.MediaType.Should().Be( "text/html" );
        }
    }
}