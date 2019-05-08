namespace Microsoft.AspNet.OData.Routing
{
    using FluentAssertions;
    using Microsoft.AspNet.OData.Routing.Conventions;
    using Microsoft.OData.Edm;
    using Microsoft.Web.Http;
    using Microsoft.Web.Http.Versioning;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Web.Http;
    using System.Web.Http.Routing;
    using Xunit;
    using static Microsoft.Web.Http.ApiVersion;
    using static System.Net.Http.HttpMethod;
    using static System.Net.HttpStatusCode;
    using static System.Web.Http.Routing.HttpRouteDirection;

    public class VersionedODataPathRouteConstraintTest
    {
        [Fact]
        public void match_should_always_return_true_for_uri_resolution()
        {
            // arrange
            var request = new HttpRequestMessage();
            var route = new Mock<IHttpRoute>().Object;
            var parameterName = (string) null;
            var values = new Dictionary<string, object>();
            var routeDirection = UriGeneration;
            var constraint = new VersionedODataPathRouteConstraint( "odata", Default );

            // act
            var result = constraint.Match( request, route, parameterName, values, routeDirection );

            // assert
            result.Should().BeTrue();
        }

        [Theory]
        [InlineData( "2.0" )]
        [InlineData( "3.0" )]
        public void match_should_be_true_when_api_version_is_requested_in_query_string( string apiVersion )
        {
            // arrange
            var request = new HttpRequestMessage( Get, $"http://localhost/Tests(1)?api-version={apiVersion}" );
            var values = new Dictionary<string, object>() { ["odataPath"] = "Tests(1)" };
            var constraint = NewVersionedODataPathRouteConstraint( request, Test.Model, Parse( apiVersion ) );

            // act
            var result = constraint.Match( request, null, null, values, UriResolution );

            // assert
            result.Should().BeTrue();
        }

        [Theory]
        [InlineData( "http://localhost?api-version=2.0", null )]
        [InlineData( "http://localhost/$metadata?api-version=2.0", "$metadata" )]
        public void match_should_return_expected_result_for_service_and_metadata_document( string requestUri, string odataPath )
        {
            // arrange
            var apiVersion = new ApiVersion( 2, 0 );
            var request = new HttpRequestMessage( Get, requestUri );
            var values = new Dictionary<string, object>() { [nameof( odataPath )] = odataPath };
            var constraint = NewVersionedODataPathRouteConstraint( request, Test.EmptyModel, apiVersion );

            // act
            var result = constraint.Match( request, null, null, values, UriResolution );

            // assert
            result.Should().BeTrue();
        }

        [Theory]
        [InlineData( "http://localhost/", null )]
        [InlineData( "http://localhost/$metadata", "$metadata" )]
        [InlineData( "http://localhost/Tests(1)", "Tests(1)" )]
        public void match_should_return_expected_result_when_controller_is_implicitly_versioned( string requestUri, string odataPath )
        {
            // arrange
            var apiVersion = new ApiVersion( 2, 0 );
            var request = new HttpRequestMessage( Get, requestUri );
            var values = new Dictionary<string, object>() { [nameof( odataPath )] = odataPath };
            var constraint = NewVersionedODataPathRouteConstraint(
                request,
                Test.Model,
                apiVersion,
                options => options.AssumeDefaultVersionWhenUnspecified = true );

            // act
            var result = constraint.Match( request, null, null, values, UriResolution );

            // assert
            result.Should().BeFalse();
            request.ODataApiVersionProperties().MatchingRoutes.Should().Equal( new Dictionary<ApiVersion, string>() { [apiVersion] = "odata" } );
        }

        [Fact]
        public void match_should_return_400_when_requested_api_version_is_ambiguous()
        {
            // arrange
            var request = new HttpRequestMessage( Get, $"http://localhost/Tests(1)?api-version=1.0&api-version=2.0" );
            var values = new Dictionary<string, object>() { ["odataPath"] = "Tests(1)" };
            var constraint = NewVersionedODataPathRouteConstraint( request, Test.Model, new ApiVersion( 1, 0 ) );

            // act
            Action match = () => constraint.Match( request, null, null, values, UriResolution );

            // assert
            match.Should().Throw<HttpResponseException>().And.Response.StatusCode.Should().Be( BadRequest );
        }

        static VersionedODataPathRouteConstraint NewVersionedODataPathRouteConstraint(
            HttpRequestMessage request,
            IEdmModel model,
            ApiVersion apiVersion,
            Action<ApiVersioningOptions> configure = default,
            string routePrefix = default )
        {
            var pathHandler = new DefaultODataPathHandler();
            var conventions = ODataRoutingConventions.CreateDefault();
            var configuration = new HttpConfiguration();
            var routingConventions = Enumerable.Empty<IODataRoutingConvention>();
            var constraint = new VersionedODataPathRouteConstraint( "odata", apiVersion );

            configuration.AddApiVersioning( configure ?? new Action<ApiVersioningOptions>( _ => { } ) );
            configuration.MapVersionedODataRoute( "odata", routePrefix, model, apiVersion );
            request.SetConfiguration( configuration );
            configuration.EnsureInitialized();

            return constraint;
        }
    }
}