namespace given
{
    using FluentAssertions;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Conventions;
    using Microsoft.AspNetCore.Mvc.Conventions.Controllers;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Xunit;
    using static System.Net.HttpStatusCode;

    public class _a_url_versioned_Controller_using_conventions : ConventionsAcceptanceTest
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
                new { RequestUrl = "api/v1/helloworld", ControllerName = nameof( HelloWorldController ), ApiVersion = "1" },
                new { RequestUrl = "api/v2/helloworld", ControllerName = nameof( HelloWorld2Controller ), ApiVersion = "2" },
                new { RequestUrl = "api/v3/helloworld", ControllerName = nameof( HelloWorld2Controller ), ApiVersion = "3" },
            };
            var example = new { controller = "", version = "" };

            foreach ( var iteration in iterations )
            {
                // act
                var response = await GetAsync( iteration.RequestUrl ).EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsExampleAsync( example );

                // assert
                response.Headers.GetValues( "api-supported-versions" ).Single().Should().Be( "2.0, 3.0, 4.0" );
                response.Headers.GetValues( "api-deprecated-versions" ).Single().Should().Be( "1.0" );
                content.ShouldBeEquivalentTo( new { controller = iteration.ControllerName, version = iteration.ApiVersion } );
            }
        }

        [Fact]
        public async Task _get_with_id_should_return_200()
        {
            // REMARKS: this test should be a theory, but when it is, it becomes flaky. any failure succeeds when run again.
            // the exact cause is unknown, but seems to be related to some form of caching. running a loop in a single test
            // case seems to resolve the problem.

            // arrange
            var iterations = new[]
            {
                new { RequestUrl = "api/v1/helloworld/42", ControllerName = nameof( HelloWorldController ), ApiVersion = "1" },
                new { RequestUrl = "api/v2/helloworld/42", ControllerName = nameof( HelloWorld2Controller ), ApiVersion = "2" },
                new { RequestUrl = "api/v3/helloworld/42", ControllerName = nameof( HelloWorld2Controller ), ApiVersion = "3" },
            };

            var example = new { controller = "", version = "", id = "" };

            foreach ( var iteration in iterations )
            {
                // act
                var response = await GetAsync( iteration.RequestUrl ).EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsExampleAsync( example );

                // assert
                response.Headers.GetValues( "api-supported-versions" ).Single().Should().Be( "2.0, 3.0, 4.0" );
                response.Headers.GetValues( "api-deprecated-versions" ).Single().Should().Be( "1.0" );
                content.ShouldBeEquivalentTo( new { controller = iteration.ControllerName, version = iteration.ApiVersion, id = "42" } );
            }
        }

        [Fact]
        public async Task _get_should_return_400_when_version_is_unsupported()
        {
            // arrange


            // act
            var response = await GetAsync( "api/v4/helloworld" );
            var content = await response.Content.ReadAsAsync<OneApiErrorResponse>();

            // assert
            response.StatusCode.Should().Be( BadRequest );
            content.Error.Code.Should().Be( "UnsupportedApiVersion" );
        }
    }
}