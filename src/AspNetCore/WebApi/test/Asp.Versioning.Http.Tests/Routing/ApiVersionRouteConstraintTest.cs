// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Routing;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using static Microsoft.AspNetCore.Routing.RouteDirection;
using static System.String;

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
        var httpContext = NewHttpContext();
        var route = new Mock<IRouter>().Object;
        var values = new RouteValueDictionary();
        var routeDirection = UrlGeneration;
        var constraint = new ApiVersionRouteConstraint();

        if ( !IsNullOrEmpty( key ) )
        {
            values[key] = value;
        }

        // act
        var matched = constraint.Match( httpContext, route, key, values, routeDirection );

        // assert
        matched.Should().Be( expected );
    }

    [Fact]
    public void match_should_return_false_when_route_key_is_missing()
    {
        // arrange
        var httpContext = NewHttpContext();
        var route = new Mock<IRouter>().Object;
        var values = new RouteValueDictionary();
        var routeDirection = IncomingRequest;
        var constraint = new ApiVersionRouteConstraint();

        // act
        var matched = constraint.Match( httpContext, route, "version", values, routeDirection );

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
        var httpContext = NewHttpContext();
        var route = new Mock<IRouter>().Object;
        var routeKey = nameof( version );
        var values = new RouteValueDictionary() { [routeKey] = version };
        var routeDirection = IncomingRequest;
        var constraint = new ApiVersionRouteConstraint();

        // act
        var matched = constraint.Match( httpContext, route, routeKey, values, routeDirection );

        // assert
        matched.Should().BeFalse();
    }

    [Fact]
    public void match_should_return_true_when_matched()
    {
        // arrange
        var httpContext = NewHttpContext();
        var route = new Mock<IRouter>().Object;
        var values = new RouteValueDictionary() { ["version"] = "2.0" };
        var routeDirection = IncomingRequest;
        var constraint = new ApiVersionRouteConstraint();

        // act
        var matched = constraint.Match( httpContext, route, "version", values, routeDirection );

        // assert
        matched.Should().BeTrue();
    }

    [Fact]
    public void url_helper_should_create_route_link_with_api_version_constraint()
    {
        // arrange
        var urlHelper = NewUrlHelper( controller: "Store", action: "Buy", version: "1" );

        // act
        var url = urlHelper.Link( "default", default );

        // assert
        url.Should().Be( "/v1/Store/Buy" );
    }

    [Fact]
    public void url_helper_should_create_route_url_with_api_version_constraint()
    {
        // arrange
        var urlHelper = NewUrlHelper( controller: "Movie", action: "Rate", version: "2" );

        // act
        var url = urlHelper.RouteUrl( "default" );

        // assert
        url.Should().Be( "/v2/Movie/Rate" );
    }

    [Fact]
    public void url_helper_should_create_action_with_api_version_constraint()
    {
        // arrange
        var urlHelper = NewUrlHelper( controller: "Order", action: "Place", version: "1.1" );

        // act
        var url = urlHelper.Action( action: "Place", controller: "Order" );

        // assert
        url.Should().Be( "/v1.1/Order/Place" );
    }

    private sealed class PassThroughRouter : IRouter
    {
        public VirtualPathData GetVirtualPath( VirtualPathContext context ) => null;

        public Task RouteAsync( RouteContext context )
        {
            context.Handler = c => Task.CompletedTask;
            return Task.CompletedTask;
        }
    }

    private static HttpContext NewHttpContext()
    {
        var featureCollection = new Mock<IFeatureCollection>();
        var serviceProvider = new Mock<IServiceProvider>();
        var httpContext = new Mock<HttpContext>();

        featureCollection.Setup( fc => fc.Get<IApiVersioningFeature>() ).Returns( () => new ApiVersioningFeature( httpContext.Object ) );
        serviceProvider.Setup( sp => sp.GetService( typeof( IApiVersionParser ) ) ).Returns( ApiVersionParser.Default );
        httpContext.SetupGet( hc => hc.Features ).Returns( featureCollection.Object );
        httpContext.SetupProperty( hc => hc.Items, new Dictionary<object, object>() );
        httpContext.SetupProperty( hc => hc.RequestServices, serviceProvider.Object );

        return httpContext.Object;
    }

    private static RouteBuilder CreateRouteBuilder( IServiceProvider services )
    {
        var app = new Mock<IApplicationBuilder>();
        app.SetupGet( a => a.ApplicationServices ).Returns( services );
        return new( app.Object ) { DefaultHandler = new PassThroughRouter() };
    }

    private static IUrlHelper NewUrlHelper( string controller, string action, string version )
    {
        var services = new ServiceCollection();

        services.AddOptions();
        services.AddLogging();
        services.AddRouting();
        services.AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>()
                .AddSingleton( UrlEncoder.Default );
        services.AddMvcCore();
        services.AddApiVersioning();

        var provider = services.BuildServiceProvider();
        var httpContext = new DefaultHttpContext() { RequestServices = provider };
        var routeBuilder = CreateRouteBuilder( provider );
        var actionContext = new ActionContext() { HttpContext = httpContext };
        var constraint = new ApiVersionRouteConstraint();

        httpContext.Features.Set<IApiVersioningFeature>( new ApiVersioningFeature( httpContext ) );
        routeBuilder.MapRoute( "default", "v{version:apiVersion}/{controller}/{action}" );

        var router = routeBuilder.Build();

        actionContext.RouteData = new()
        {
            Values =
            {
                [nameof(controller)] = controller,
                [nameof(action)] = action,
                [nameof(version)] = version,
            },
            Routers =
            {
                router,
            },
        };
        actionContext.RouteData.Routers.Add( router );
        constraint.Match( httpContext, router, nameof( version ), actionContext.RouteData.Values, IncomingRequest );

        var factory = provider.GetRequiredService<IUrlHelperFactory>();

        return factory.GetUrlHelper( actionContext );
    }
}