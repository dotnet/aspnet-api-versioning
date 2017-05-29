namespace given_a_versionX2Dneutral_UI_Controller
{
    using FluentAssertions;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Basic;
    using Microsoft.AspNetCore.Mvc.Basic.Controllers.WithViewsUsingConventions;
    using System.Net.Http.Headers;
    using System.Reflection;
    using System.Threading.Tasks;
    using Xunit;

    public class when_accessing_a_view_using_convention_routing : BasicAcceptanceTest
    {
        public when_accessing_a_view_using_convention_routing()
        {
            FilteredControllerTypes.Clear();
            FilteredControllerTypes.Add( typeof( HomeController ).GetTypeInfo() );
        }

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