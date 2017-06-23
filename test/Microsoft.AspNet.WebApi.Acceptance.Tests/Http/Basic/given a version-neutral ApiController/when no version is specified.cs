namespace given_a_version_neutral_ApiController
{
    using FluentAssertions;
    using Microsoft.Web.Http.Basic;
    using System.Threading.Tasks;
    using Xunit;
    using static System.Net.HttpStatusCode;

    public class when_no_version_is_specified : BasicAcceptanceTest
    {
        [Fact]
        public async Task then_get_should_return_204()
        {
            // arrange


            // act
            var response = await GetAsync( "api/ping" );

            // assert
            response.StatusCode.Should().Be( NoContent );
        }
    }
}