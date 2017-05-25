﻿namespace given_a_versioned_Controller
{
    using FluentAssertions;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Basic;
    using Microsoft.AspNetCore.Mvc.Basic.Controllers;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Xunit;
    using static System.Net.HttpStatusCode;

    public class when_using_a_url_segment : BasicAcceptanceTest
    {
        [Theory]
        [InlineData( "api/v1/helloworld", nameof( HelloWorldController ), "1" )]
        [InlineData( "api/v2/helloworld", nameof( HelloWorld2Controller ), "2" )]
        public async Task then_get_should_return_200( string requestUrl, string controller, string apiVersion )
        {
            // arrange
            var example = new { controller = "", version = "" };

            // act
            var response = await GetAsync( requestUrl ).EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsExampleAsync( example );

            // assert
            response.Headers.GetValues( "api-supported-versions" ).Single().Should().Be( "1.0, 2.0" );
            content.ShouldBeEquivalentTo( new { controller = controller, version = apiVersion } );
        }

        [Theory]
        [InlineData( "api/v1/helloworld/42", nameof( HelloWorldController ), "1", "42" )]
        [InlineData( "api/v2/helloworld/42", nameof( HelloWorld2Controller ), "2", 42 )]
        public async Task then_get_by_id_should_return_200( string requestUrl, string controller, string apiVersion, object id )
        {
            // arrange
            var example = new { controller = "", version = "", id = default( object ) };

            // act
            var response = await GetAsync( requestUrl ).EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsExampleAsync( example );

            // assert
            response.Headers.GetValues( "api-supported-versions" ).Single().Should().Be( "1.0, 2.0" );
            content.ShouldBeEquivalentTo( new { controller = controller, version = apiVersion, id = id } );
        }

        [Theory]
        [InlineData( "v1" )]
        [InlineData( "v2" )]
        public async Task then_post_should_return_201( string version )
        {
            // arrange
            var entity = default( object );

            // act
            var response = await PostAsync( $"api/{version}/helloworld", entity ).EnsureSuccessStatusCode();

            // assert
            response.Headers.Location.Should().Be( new Uri( $"http://localhost/api/{version}/HelloWorld/42" ) );
        }

        [Fact]
        public async Task then_get_should_return_400_for_an_unsupported_version()
        {
            // arrange


            // act
            var response = await GetAsync( "api/v3/helloworld" );
            var content = await response.Content.ReadAsAsync<OneApiErrorResponse>();

            // assert
            response.StatusCode.Should().Be( BadRequest );
            content.Error.Code.Should().Be( "UnsupportedApiVersion" );
        }

        [Theory]
        [InlineData( nameof( HelloWorldController ), "1" )]
        [InlineData( nameof( HelloWorld2Controller ), "2" )]
        public async Task then_action_segment_should_not_be_ambiguous_with_route_parameter( string controller, string apiVersion )
        {
            // arrange
            var example = new { controller = "", query = "", version = "" };

            // act
            var response = await GetAsync( $"api/v{apiVersion}/helloworld/search?query=Foo" ).EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsExampleAsync( example );

            // assert
            response.Headers.GetValues( "api-supported-versions" ).Single().Should().Be( "1.0, 2.0" );
            content.ShouldBeEquivalentTo( new { controller = controller, query = "Foo", version = apiVersion } );
        }
    }
}