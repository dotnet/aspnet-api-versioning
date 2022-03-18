// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Routing;

using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;
using static System.String;
using static System.Web.Http.Routing.HttpRouteDirection;

public class ApiVersionRouteConstraintTest
{
    [Theory]
    [InlineData( "apiVersion", "1", true )]
    [InlineData( "apiVersion", null, false )]
    [InlineData( "apiVersion", "", false )]
    [InlineData( null, "", false )]
    public void match_should_return_expected_result_for_url_generation( string key, string value, bool expected )
    {
        // arrange
        var request = new HttpRequestMessage();
        var route = new Mock<IHttpRoute>().Object;
        var values = new Dictionary<string, object>();
        var routeDirection = UriGeneration;
        var constraint = new ApiVersionRouteConstraint();

        if ( !IsNullOrEmpty( key ) )
        {
            values[key] = value;
        }

        // act
        var matched = constraint.Match( request, route, key, values, routeDirection );

        // assert
        matched.Should().Be( expected );
    }

    [Fact]
    public void match_should_return_false_when_route_parameter_is_missing()
    {
        // arrange
        var request = new HttpRequestMessage();
        var route = new Mock<IHttpRoute>().Object;
        var values = new Dictionary<string, object>();
        var routeDirection = UriResolution;
        var constraint = new ApiVersionRouteConstraint();

        // act
        var matched = constraint.Match( request, route, "version", values, routeDirection );

        // assert
        matched.Should().BeFalse();
    }

    [Theory]
    [InlineData( null )]
    [InlineData( "" )]
    [InlineData( "abc" )]
    public void match_should_return_false_when_route_parameter_is_invalid( string version )
    {
        // arrange
        var request = new HttpRequestMessage();
        var route = new Mock<IHttpRoute>().Object;
        var parameterName = nameof( version );
        var values = new Dictionary<string, object>() { [parameterName] = version };
        var routeDirection = UriResolution;
        var constraint = new ApiVersionRouteConstraint();

        request.SetConfiguration( new() );

        // act
        var matched = constraint.Match( request, route, parameterName, values, routeDirection );

        // assert
        matched.Should().BeFalse();
    }

    [Fact]
    public void match_should_return_true_when_matched()
    {
        // arrange
        var request = new HttpRequestMessage();
        var route = new Mock<IHttpRoute>().Object;
        var values = new Dictionary<string, object>() { ["version"] = "2.0" };
        var routeDirection = UriResolution;
        var constraint = new ApiVersionRouteConstraint();

        request.SetConfiguration( new() );

        // act
        var matched = constraint.Match( request, route, "version", values, routeDirection );

        // assert
        matched.Should().BeTrue();
    }

    [Fact]
    public void url_helper_should_create_route_link_with_api_version_constraint()
    {
        // arrange
        var request = new HttpRequestMessage();
        var routes = new HttpRouteCollection( "/" );
        var route = routes.MapHttpRoute(
            "Default",
            "v{apiVersion}/{controller}/{id}",
            defaults: null,
            constraints: new { apiVersion = new ApiVersionRouteConstraint() } );
        var values = new HttpRouteValueDictionary( new { apiVersion = "1", controller = "people", id = "123" } );
        var urlHelper = new UrlHelper( request );
        var routeValues = new { apiVersion = "1", controller = "people", id = "123" };

        request.Properties[HttpPropertyKeys.HttpConfigurationKey] = new HttpConfiguration( routes );
        request.Properties[HttpPropertyKeys.HttpRouteDataKey] = new HttpRouteData( route, values );

        // act
        var url = urlHelper.Route( "Default", routeValues );

        // assert
        url.Should().Be( "/v1/people/123" );
    }
}