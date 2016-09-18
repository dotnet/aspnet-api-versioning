namespace given
{
    using FluentAssertions;
    using Microsoft.Web;
    using Microsoft.Web.OData.Advanced;
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Xunit;
    using static System.Net.HttpStatusCode;

    public class _a_mixX2Din_people_ODataController_split_into_two_types : AdvancedAcceptanceTest
    {
        [Theory]
        [InlineData( "api/people" )]
        [InlineData( "api/people?api-version=1.0" )]
        public async Task _get_should_return_200_for_v1( string requestUrl )
        {
            // arrange
            var example = new { value = new[] { new { id = 0, firstName = "", lastName = "" } } };

            // act
            var response = await Client.GetAsync( requestUrl );
            var people = await response.EnsureSuccessStatusCode().Content.ReadAsExampleAsync( example );

            // assert
            people.value.ShouldBeEquivalentTo(
                new[] { new { id = 1, firstName = "Bill", lastName = "Mei" } },
                options => options.ExcludingMissingMembers() );
        }

        [Theory]
        [InlineData( "api/people(42)" )]
        [InlineData( "api/people(42)?api-version=1.0" )]
        public async Task _get_with_key_should_return_200_for_v1( string requestUrl )
        {
            // arrange
            var example = new { id = 0, firstName = "", lastName = "" };

            // act
            var response = await Client.GetAsync( requestUrl );
            var order = await response.EnsureSuccessStatusCode().Content.ReadAsExampleAsync( example );

            // assert
            order.ShouldBeEquivalentTo(
                new { id = 42, firstName = "Bill", lastName = "Mei" },
                options => options.ExcludingMissingMembers() );
        }

        [Fact]
        public async Task _get_should_return_200_for_v2()
        {
            // arrange
            var example = new { value = new[] { new { id = 0, firstName = "", lastName = "", email = "" } } };

            // act
            var response = await Client.GetAsync( "api/people?api-version=2.0" );
            var people = await response.EnsureSuccessStatusCode().Content.ReadAsExampleAsync( example );

            // assert
            people.value.ShouldBeEquivalentTo(
                new[] { new { id = 1, firstName = "Bill", lastName = "Mei", email = "bill.mei@somewhere.com" } },
                options => options.ExcludingMissingMembers() );
        }

        [Fact]
        public async Task _get_with_key_should_return_200_for_v2()
        {
            // arrange
            var example = new { id = 0, firstName = "", lastName = "", email = "" };

            // act
            var response = await Client.GetAsync( "api/people(42)?api-version=2.0" );
            var order = await response.EnsureSuccessStatusCode().Content.ReadAsExampleAsync( example );

            // assert
            order.ShouldBeEquivalentTo(
                new { id = 42, firstName = "Bill", lastName = "Mei", email = "bill.mei@somewhere.com" },
                options => options.ExcludingMissingMembers() );
        }

        [Fact]
        public async Task _get_should_return_200_for_v3()
        {
            // arrange
            var example = new { value = new[] { new { id = 0, firstName = "", lastName = "", email = "", phone = "" } } };

            // act
            var response = await Client.GetAsync( "api/people?api-version=3.0" );
            var people = await response.EnsureSuccessStatusCode().Content.ReadAsExampleAsync( example );

            // assert
            people.value.ShouldBeEquivalentTo(
                new[] { new { id = 1, firstName = "Bill", lastName = "Mei", email = "bill.mei@somewhere.com", phone = "555-555-5555" } },
                options => options.ExcludingMissingMembers() );
        }

        [Fact]
        public async Task _get_with_key_should_return_200_for_v3()
        {
            // arrange
            var example = new { id = 0, firstName = "", lastName = "", email = "", phone = "" };

            // act
            var response = await Client.GetAsync( "api/people(42)?api-version=3.0" );
            var order = await response.EnsureSuccessStatusCode().Content.ReadAsExampleAsync( example );

            // assert
            order.ShouldBeEquivalentTo(
                new { id = 42, firstName = "Bill", lastName = "Mei", email = "bill.mei@somewhere.com", phone = "555-555-5555" },
                options => options.ExcludingMissingMembers() );
        }

        [Fact]
        public async Task _patch_should_return_204()
        {
            // arrange
            var person = new { email = "bmei@somewhere.com" };

            // act
            var response = await PatchAsync( "api/people(42)?api-version=2.0", person );

            // assert
            response.StatusCode.Should().Be( NoContent );
        }

        [Fact]
        public async Task _patch_should_return_400_when_updating_member_that_does_not_exist_in_api_version()
        {
            // arrange
            var person = new { phone = "bmei@somewhere.com" };

            // act
            var response = await PatchAsync( "api/people(42)?api-version=2.0", person );

            // assert
            response.StatusCode.Should().Be( BadRequest );
        }

        [Theory]
        [InlineData( "1.0" )]
        [InlineData( "3.0" )]
        [InlineData( "4.0" )]
        public async Task _patch_should_return_400_when_version_is_unsupported( string apiVersion )
        {
            // arrange
            var person = new { lastName = "Me" };

            // act
            var response = await PatchAsync( $"api/people(42)?api-version={apiVersion}", person );
            var content = await response.Content.ReadAsAsync<OneApiErrorResponse>();

            // assert
            response.StatusCode.Should().Be( BadRequest );
            content.Error.Code.Should().Be( "UnsupportedApiVersion" );
        }
    }
}