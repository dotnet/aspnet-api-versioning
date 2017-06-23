namespace given_a_versioned_ApiController
{
    using FluentAssertions;
    using Microsoft.Web.Http.Basic;
    using System;
    using System.Threading.Tasks;
    using Xunit;
    using static System.Net.HttpStatusCode;

    public class when_two_route_templates_overlap : BasicAcceptanceTest
    {
        [Fact]
        public async Task then_the_higher_precedence_route_should_be_selected_during_the_first_request()
        {
            // arrange
            var response = await Client.GetAsync( "api/v1/values/42/children" );
            var result1 = await response.EnsureSuccessStatusCode().Content.ReadAsStringAsync();

            // act
            response = await Client.GetAsync( "api/v1/values/42/abc" );
            var result2 = await response.EnsureSuccessStatusCode().Content.ReadAsStringAsync();

            // assert
            result1.Should().Be( "{\"id\":42}" );
            result2.Should().Be( "{\"id\":42,\"childId\":\"abc\"}" );
        }

        [Fact]
        public async Task then_the_higher_precedence_route_should_be_selected_during_the_second_request()
        {
            // arrange
            var response = await Client.GetAsync( "api/v1/values/42/abc" );
            var result1 = await response.EnsureSuccessStatusCode().Content.ReadAsStringAsync();

            // act
            response = await Client.GetAsync( "api/v1/values/42/children" );
            var result2 = await response.EnsureSuccessStatusCode().Content.ReadAsStringAsync();

            // assert
            result1.Should().Be( "{\"id\":42,\"childId\":\"abc\"}" );
            result2.Should().Be( "{\"id\":42}" );
        }

        [Fact]
        public async Task then_the_higher_precedence_route_should_result_in_500_during_the_second_request()
        {
            // arrange
            var response = await Client.GetAsync( "api/v1/values/42/abc" );
            var result1 = await response.EnsureSuccessStatusCode().Content.ReadAsStringAsync();

            // act
            response = await Client.GetAsync( "api/v1/values/42/ambiguous" );

            // assert
            result1.Should().Be( "{\"id\":42,\"childId\":\"abc\"}" );
            response.StatusCode.Should().Be( InternalServerError );
        }
    }
}