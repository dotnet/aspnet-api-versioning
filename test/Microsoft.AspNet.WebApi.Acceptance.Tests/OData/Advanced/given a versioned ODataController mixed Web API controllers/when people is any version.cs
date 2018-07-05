namespace given_a_versioned_ODataController_mixed_Web_API_controllers
{
    using FluentAssertions;
    using Microsoft.AspNet.OData.Advanced;
    using Microsoft.Web;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Xunit;
    using static System.Net.HttpStatusCode;

    public class when_people_is_any_version : AdvancedAcceptanceTest
    {
        [Fact]
        public async Task then_patch_should_return_400_for_an_unsupported_version()
        {
            // arrange
            var person = new { lastName = "Me" };

            // act
            var response = await PatchAsync( $"api/people(42)?api-version=4.0", person );
            var content = await response.Content.ReadAsAsync<OneApiErrorResponse>();

            // assert
            response.StatusCode.Should().Be( BadRequest );
            content.Error.Code.Should().Be( "UnsupportedApiVersion" );
        }
    }
}