namespace Microsoft.AspNetCore.Mvc.Routing
{
    using AspNetCore.Routing;
    using Builder;
    using Extensions.DependencyInjection;
    using Extensions.ObjectPool;
    using FluentAssertions;
    using Http;
    using Microsoft.AspNetCore.Http.Features;
    using Microsoft.AspNetCore.Mvc.Versioning;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Text.Encodings.Web;
    using System.Threading.Tasks;
    using Xunit;
    using static AspNetCore.Routing.RouteDirection;
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
        public void url_helper_should_create_route_link_with_api_version_constriant()
        {
            // arrange
            var services = CreateServices().AddApiVersioning();
            var provider = services.BuildServiceProvider();
            var httpContext = new DefaultHttpContext() { RequestServices = provider };
            var routeBuilder = CreateRouteBuilder( provider );
            var actionContext = new ActionContext() { HttpContext = httpContext };

            httpContext.Features.Set<IApiVersioningFeature>( new ApiVersioningFeature( httpContext ) );
            routeBuilder.MapRoute( "default", "v{version:apiVersion}/{controller}/{action}" );
            actionContext.RouteData = new RouteData();
            actionContext.RouteData.Routers.Add( routeBuilder.Build() );

            var urlHelper = new UrlHelper( actionContext );

            // act
            var url = urlHelper.Link( "default", new { version = "1", controller = "Store", action = "Buy" } );

            // assert
            url.Should().Be( "/v1/Store/Buy" );
        }

        class PassThroughRouter : IRouter
        {
            public VirtualPathData GetVirtualPath( VirtualPathContext context ) => null;

            public Task RouteAsync( RouteContext context )
            {
                context.Handler = c => Task.CompletedTask;
                return Task.CompletedTask;
            }
        }

        static HttpContext NewHttpContext()
        {
            var featureCollection = new Mock<IFeatureCollection>();
            var httpContext = new Mock<HttpContext>();

            featureCollection.Setup( fc => fc.Get<IApiVersioningFeature>() ).Returns( () => new ApiVersioningFeature( httpContext.Object ) );
            httpContext.SetupGet( hc => hc.Features ).Returns( featureCollection.Object );
            httpContext.SetupProperty( hc => hc.Items, new Dictionary<object, object>() );

            return httpContext.Object;
        }

        static ServiceCollection CreateServices()
        {
            var services = new ServiceCollection();

            services.AddOptions();
            services.AddLogging();
            services.AddRouting();
            services.AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>()
                    .AddSingleton( UrlEncoder.Default );

            return services;
        }

        static IRouteBuilder CreateRouteBuilder( IServiceProvider services )
        {
            var app = new Mock<IApplicationBuilder>();
            app.SetupGet( a => a.ApplicationServices ).Returns( services );
            return new RouteBuilder( app.Object ) { DefaultHandler = new PassThroughRouter() };
        }
    }
}