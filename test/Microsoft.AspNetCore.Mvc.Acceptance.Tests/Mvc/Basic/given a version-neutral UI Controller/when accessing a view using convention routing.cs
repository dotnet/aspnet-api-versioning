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

    public class when_accessing_a_view_using_convention_routing : AcceptanceTest, IClassFixture<UIFixture>
    {
        [Theory]
        [InlineData( "http://localhost" )]
        [InlineData( "http://localhost/home" )]
        [InlineData( "http://localhost/home/index" )]
        public async Task then_get_should_return_200( string requestUrl )
        {
            // arrange
            Client.DefaultRequestHeaders.Clear();
            Client.DefaultRequestHeaders.Accept.Add( new MediaTypeWithQualityHeaderValue( "text/html" ) );

            // act
            var response = await GetAsync( requestUrl ).EnsureSuccessStatusCode();

            // assert
            response.Content.Headers.ContentType.MediaType.Should().Be( "text/html" );
        }

        public when_accessing_a_view_using_convention_routing( UIFixture fixture ) : base( fixture )
        {
            fixture.FilteredControllerTypes.Add( typeof( HomeController ).GetTypeInfo() );
        }
    }

    public class when_accessing_a_view_using_convention_routing_ : when_accessing_a_view_using_convention_routing, IClassFixture<UIEndpointFixture>
    {
        public when_accessing_a_view_using_convention_routing_( UIEndpointFixture fixture ) : base( fixture ) { }
    }
}