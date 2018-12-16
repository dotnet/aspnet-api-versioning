namespace given_a_versioned_ODataController_mixed_with_Web_API_controllers
{
    using FluentAssertions;
    using Microsoft.AspNet.OData;
    using Microsoft.AspNet.OData.Advanced;
    using Microsoft.Web;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Xunit;
    using static System.Net.HttpStatusCode;

    [Collection( nameof( AdvancedODataCollection ) )]
    public class when_people_is_v3 : ODataAcceptanceTest
    {
        [Fact]
        public async Task then_get_should_return_200()
        {
            // arrange
            var example = new { value = new[] { new { id = 0, firstName = "", lastName = "", email = "", phone = "" } } };

            // act
            var response = await Client.GetAsync( "api/people?api-version=3.0" );
            var people = await response.EnsureSuccessStatusCode().Content.ReadAsExampleAsync( example );

            // assert
            people.value.Should().BeEquivalentTo(
                new[] { new { id = 1, firstName = "Bill", lastName = "Mei", email = "bill.mei@somewhere.com", phone = "555-555-5555" } },
                options => options.ExcludingMissingMembers() );
        }

        [Fact]
        public async Task then_get_with_key_should_return_200()
        {
            // arrange
            var example = new { id = 0, firstName = "", lastName = "", email = "", phone = "" };

            // act
            var response = await Client.GetAsync( "api/people(42)?api-version=3.0" );
            var order = await response.EnsureSuccessStatusCode().Content.ReadAsExampleAsync( example );

            // assert
            order.Should().BeEquivalentTo(
                new { id = 42, firstName = "Bill", lastName = "Mei", email = "bill.mei@somewhere.com", phone = "555-555-5555" },
                options => options.ExcludingMissingMembers() );
        }

        [Fact]
        public async Task then_patch_should_return_405_if_supported_in_any_version()
        {
            // arrange
            var person = new { lastName = "Me" };

            // act
            var response = await PatchAsync( $"api/people(42)?api-version=3.0", person );
            var content = await response.Content.ReadAsAsync<OneApiErrorResponse>();

            // assert
            response.StatusCode.Should().Be( MethodNotAllowed );
            content.Error.Code.Should().Be( "UnsupportedApiVersion" );
        }

        public when_people_is_v3( AdvancedFixture fixture ) : base( fixture ) { }
    }
}