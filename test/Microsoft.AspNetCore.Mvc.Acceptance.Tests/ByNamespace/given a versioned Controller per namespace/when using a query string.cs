﻿namespace given_a_versioned_Controller_per_namespace
{
    using FluentAssertions;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.ByNamespace;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Xunit;
    using static System.Net.HttpStatusCode;

    public class when_using_a_query_string : ByNamespaceAcceptanceTest
    {
        [Theory]
        [InlineData( "Microsoft.AspNetCore.Mvc.ByNamespace.Controllers.V1.AgreementsController", "1.0" )]
        [InlineData( "Microsoft.AspNetCore.Mvc.ByNamespace.Controllers.V2.AgreementsController", "2.0" )]
        [InlineData( "Microsoft.AspNetCore.Mvc.ByNamespace.Controllers.V3.AgreementsController", "3.0" )]
        public async Task then_get_should_return_200( string controller, string apiVersion )
        {
            // arrange
            var example = new { controller = "", apiVersion = "", accountId = "" };

            // act
            var response = await GetAsync( $"api/agreements/42?api-version={apiVersion}" ).EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsExampleAsync( example );

            // assert
            response.Headers.GetValues( "api-supported-versions" ).Single().Should().Be( "1.0, 2.0, 3.0" );
            content.Should().BeEquivalentTo( new { controller = controller, apiVersion = apiVersion, accountId = "42" } );
        }

        [Fact]
        public async Task then_get_should_return_400_for_an_unsupported_version()
        {
            // arrange


            // act
            var response = await GetAsync( "api/agreements/42?api-version=4.0" );
            var content = await response.Content.ReadAsAsync<OneApiErrorResponse>();

            // assert
            response.StatusCode.Should().Be( BadRequest );
            content.Error.Code.Should().Be( "UnsupportedApiVersion" );
        }

        [Fact]
        public async Task then_get_should_return_400_for_an_unspecified_version()
        {
            // arrange


            // act
            var response = await GetAsync( "api/agreements/42" );
            var content = await response.Content.ReadAsAsync<OneApiErrorResponse>();

            // assert
            response.StatusCode.Should().Be( BadRequest );
            content.Error.Code.Should().Be( "ApiVersionUnspecified" );
        }
    }
}