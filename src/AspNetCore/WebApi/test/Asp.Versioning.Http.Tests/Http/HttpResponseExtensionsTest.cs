// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Microsoft.AspNetCore.Http;

using Asp.Versioning;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Primitives;

public class HttpResponseExtensionsTest
{
    [Fact]
    public void add_api_version_to_content_type_should_ignore_unsuccessful_status_code()
    {
        // arrange
        var response = new Mock<HttpResponse>();
        var headers = new HeaderDictionary();

        response.SetupProperty( r => r.StatusCode, 400 );
        response.SetupGet( r => r.Headers ).Returns( headers );

        // act
        response.Object.AddApiVersionToContentType( "v" );

        // assert
        headers.Should().BeEmpty();
    }

    [Fact]
    public void add_api_version_to_content_type_should_ignore_missing_header()
    {
        // arrange
        var response = new Mock<HttpResponse>();
        var headers = new HeaderDictionary();

        response.SetupProperty( r => r.StatusCode, 200 );
        response.SetupGet( r => r.Headers ).Returns( headers );

        // act
        response.Object.AddApiVersionToContentType( "v" );

        // assert
        headers.Should().BeEmpty();
    }

    [Fact]
    public void add_api_version_to_content_type_should_ignore_missing_version()
    {
        // arrange
        var httpContext = new Mock<HttpContext>();
        var serviceProvider = new Mock<IServiceProvider>();
        var features = new Mock<IFeatureCollection>();
        var request = new Mock<HttpRequest>();
        var response = new Mock<HttpResponse>();
        var headers = new HeaderDictionary()
        {
            ["Content-Type"] = "application/json",
        };

        serviceProvider.Setup( sp => sp.GetService( typeof( IApiVersionParser ) ) ).Returns( ApiVersionParser.Default );
        serviceProvider.Setup( sp => sp.GetService( typeof( IApiVersionReader ) ) ).Returns( new QueryStringApiVersionReader() );
        features.Setup( f => f.Get<IApiVersioningFeature>() ).Returns( () => new ApiVersioningFeature( httpContext.Object ) );
        request.SetupGet( r => r.Query ).Returns( Mock.Of<IQueryCollection>() );
        request.SetupGet( r => r.HttpContext ).Returns( () => httpContext.Object );
        response.SetupProperty( r => r.StatusCode, 200 );
        response.SetupGet( r => r.Headers ).Returns( headers );
        response.SetupGet( r => r.HttpContext ).Returns( () => httpContext.Object );
        httpContext.SetupGet( c => c.Features ).Returns( features.Object );
        httpContext.SetupGet( c => c.Request ).Returns( request.Object );
        httpContext.SetupProperty( hc => hc.RequestServices, serviceProvider.Object );

        // act
        response.Object.AddApiVersionToContentType( "v" );

        // assert
        headers["Content-Type"].Single().Should().Be( "application/json" );
    }

    [Fact]
    public void add_api_version_to_content_type_should_ignore_existing_parameter()
    {
        // arrange
        var httpContext = new Mock<HttpContext>();
        var serviceProvider = new Mock<IServiceProvider>();
        var features = new Mock<IFeatureCollection>();
        var request = new Mock<HttpRequest>();
        var response = new Mock<HttpResponse>();
        var query = new Mock<IQueryCollection>();
        var headers = new HeaderDictionary()
        {
            ["Content-Type"] = "application/json;v=1.0",
        };

        serviceProvider.Setup( sp => sp.GetService( typeof( IApiVersionParser ) ) ).Returns( ApiVersionParser.Default );
        serviceProvider.Setup( sp => sp.GetService( typeof( IApiVersionReader ) ) ).Returns( new QueryStringApiVersionReader() );
        features.Setup( f => f.Get<IApiVersioningFeature>() ).Returns( () => new ApiVersioningFeature( httpContext.Object ) );
        query.SetupGet( q => q["api-version"] ).Returns( new StringValues( "42.0" ) );
        request.SetupGet( r => r.Query ).Returns( query.Object );
        request.SetupGet( r => r.HttpContext ).Returns( () => httpContext.Object );
        response.SetupProperty( r => r.StatusCode, 200 );
        response.SetupGet( r => r.Headers ).Returns( headers );
        response.SetupGet( r => r.HttpContext ).Returns( () => httpContext.Object );
        httpContext.SetupGet( c => c.Features ).Returns( features.Object );
        httpContext.SetupGet( c => c.Request ).Returns( request.Object );
        httpContext.SetupProperty( hc => hc.RequestServices, serviceProvider.Object );

        // act
        response.Object.AddApiVersionToContentType( "v" );

        // assert
        headers["Content-Type"].Single().Should().Be( "application/json;v=1.0" );
    }

    [Fact]
    public void add_api_version_to_content_type_should_set_parameter()
    {
        // arrange
        var httpContext = new Mock<HttpContext>();
        var serviceProvider = new Mock<IServiceProvider>();
        var features = new Mock<IFeatureCollection>();
        var request = new Mock<HttpRequest>();
        var response = new Mock<HttpResponse>();
        var query = new Mock<IQueryCollection>();
        var headers = new HeaderDictionary()
        {
            ["Content-Type"] = "application/json",
        };

        serviceProvider.Setup( sp => sp.GetService( typeof( IApiVersionParser ) ) ).Returns( ApiVersionParser.Default );
        serviceProvider.Setup( sp => sp.GetService( typeof( IApiVersionReader ) ) ).Returns( new QueryStringApiVersionReader() );
        features.Setup( f => f.Get<IApiVersioningFeature>() ).Returns( () => new ApiVersioningFeature( httpContext.Object ) );
        query.SetupGet( q => q["api-version"] ).Returns( new StringValues( "42.0" ) );
        request.SetupGet( r => r.Query ).Returns( query.Object );
        request.SetupGet( r => r.HttpContext ).Returns( () => httpContext.Object );
        response.SetupProperty( r => r.StatusCode, 200 );
        response.SetupGet( r => r.Headers ).Returns( headers );
        response.SetupGet( r => r.HttpContext ).Returns( () => httpContext.Object );
        httpContext.SetupGet( c => c.Features ).Returns( features.Object );
        httpContext.SetupGet( c => c.Request ).Returns( request.Object );
        httpContext.SetupProperty( hc => hc.RequestServices, serviceProvider.Object );

        // act
        response.Object.AddApiVersionToContentType( "v" );

        // assert
        headers["Content-Type"].Single().Should().Be( "application/json; v=42.0" );
    }
}