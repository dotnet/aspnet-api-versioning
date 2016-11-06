namespace given
{
    using FluentAssertions;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Conventions;
    using Microsoft.AspNetCore.Mvc.Conventions.Controllers;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Xunit;
    using static System.Net.HttpStatusCode;

    public class _a_query_string_versioned_Controller_split_into_two_types_using_conventions : ConventionsAcceptanceTest
    {
        [Fact]
        public async Task _get_should_return_200()
        {
            // REMARKS: this test should be a theory, but when it is, it becomes flaky. any failure succeeds when run again.
            // the exact cause is unknown, but seems to be related to some form of caching. running a loop in a single test
            // case seems to resolve the problem.

            // arrange
            var iterations = new[]
            {
                new { Controller = nameof( ValuesController),  ApiVersion = "1.0" },
                new { Controller = nameof( Values2Controller), ApiVersion = "2.0" },
                new { Controller = nameof( Values2Controller), ApiVersion = "3.0" }
            };
            var example = new { controller = "", version = "" };

            foreach ( var iteration in iterations )
            {
                // act
                var response = await GetAsync( $"api/values?api-version={iteration.ApiVersion}" ).EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsExampleAsync( example );

                // assert
                response.Headers.GetValues( "api-supported-versions" ).Single().Should().Be( "1.0, 2.0, 3.0" );
                content.ShouldBeEquivalentTo( new { controller = iteration.Controller, version = iteration.ApiVersion } );
            }
        }

        [Fact]
        public async Task _get_should_return_400_when_version_is_unsupported()
        {
            // arrange


            // act
            var response = await GetAsync( "api/values?api-version=4.0" );
            var content = await response.Content.ReadAsAsync<OneApiErrorResponse>();

            // assert
            response.StatusCode.Should().Be( BadRequest );
            content.Error.Code.Should().Be( "UnsupportedApiVersion" );
        }
    }
}