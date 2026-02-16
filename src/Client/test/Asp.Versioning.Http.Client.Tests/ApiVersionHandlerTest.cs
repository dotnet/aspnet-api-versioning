// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Http;

public class ApiVersionHandlerTest
{
    [Fact]
    public async Task send_async_should_write_api_version_to_request()
    {
        // arrange
        var writer = Mock.Of<IApiVersionWriter>();
        var request = new HttpRequestMessage( HttpMethod.Get, "http://tempuri.org" );
        var version = new ApiVersion( 1.0 );
        using var handler = new ApiVersionHandler( writer, version )
        {
            InnerHandler = new TestServer(),
        };
        using var invoker = new HttpMessageInvoker( handler );

        // act
        await invoker.SendAsync( request, TestContext.Current.CancellationToken );

        // assert
        Mock.Get( writer ).Verify( w => w.Write( request, version ) );
    }

    [Fact]
    public async Task send_async_should_not_notify_when_no_headers_are_set()
    {
        // arrange
        var writer = Mock.Of<IApiVersionWriter>();
        var notification = Mock.Of<IApiNotification>();
        var request = new HttpRequestMessage( HttpMethod.Get, "http://tempuri.org" );
        var version = new ApiVersion( 1.0 );
        using var handler = new ApiVersionHandler( writer, version, notification )
        {
            InnerHandler = new TestServer(),
        };
        using var invoker = new HttpMessageInvoker( handler );

        // act
        await invoker.SendAsync( request, TestContext.Current.CancellationToken );

        // assert
        Mock.Get( notification )
            .Verify( n => n.OnApiDeprecatedAsync( It.IsAny<ApiNotificationContext>(), It.IsAny<CancellationToken>() ), Times.Never );
        Mock.Get( notification )
            .Verify( n => n.OnNewApiAvailableAsync( It.IsAny<ApiNotificationContext>(), It.IsAny<CancellationToken>() ), Times.Never );
    }

    [Fact]
    public async Task send_async_should_signal_deprecated_api_versions_from_header()
    {
        // arrange
        var writer = Mock.Of<IApiVersionWriter>();
        var notification = Mock.Of<IApiNotification>();
        var request = new HttpRequestMessage( HttpMethod.Get, "http://tempuri.org" );
        var response = new HttpResponseMessage();
        var version = new ApiVersion( 1.0 );
        var cancellationToken = TestContext.Current.CancellationToken;
        using var handler = new ApiVersionHandler( writer, version, notification )
        {
            InnerHandler = new TestServer( response ),
        };
        using var invoker = new HttpMessageInvoker( handler );

        response.Headers.Add( "api-supported-versions", "2.0" );
        response.Headers.Add( "api-deprecated-versions", "1.0" );

        // act
        await invoker.SendAsync( request, cancellationToken );

        // assert
        Mock.Get( notification )
            .Verify( n => n.OnApiDeprecatedAsync( It.IsAny<ApiNotificationContext>(), cancellationToken ) );
    }

    [Fact]
    public async Task send_async_should_signal_deprecated_api_versions_from_deprecation_policy()
    {
        // arrange
        var writer = Mock.Of<IApiVersionWriter>();
        var notification = Mock.Of<IApiNotification>();
        var request = new HttpRequestMessage( HttpMethod.Get, "http://tempuri.org" );
        var response = new HttpResponseMessage();
        var version = new ApiVersion( 1.0 );
        var cancellationToken = TestContext.Current.CancellationToken;
        using var handler = new ApiVersionHandler( writer, version, notification )
        {
            InnerHandler = new TestServer( response ),
        };
        using var invoker = new HttpMessageInvoker( handler );

        response.Headers.Add( "api-supported-versions", "2.0" );
        response.Headers.Add( "deprecation", DateTimeOffset.UtcNow.ToDeprecationHeaderValue() );

        // act
        await invoker.SendAsync( request, cancellationToken );

        // assert
        Mock.Get( notification )
            .Verify( n => n.OnApiDeprecatedAsync( It.IsAny<ApiNotificationContext>(), cancellationToken ) );
    }

    [Fact]
    public async Task send_async_should_signal_new_api_version()
    {
        // arrange
        var writer = Mock.Of<IApiVersionWriter>();
        var notification = Mock.Of<IApiNotification>();
        var request = new HttpRequestMessage( HttpMethod.Get, "http://tempuri.org" );
        var response = new HttpResponseMessage();
        var version = new ApiVersion( 1.0 );
        var cancellationToken = TestContext.Current.CancellationToken;
        using var handler = new ApiVersionHandler( writer, version, notification )
        {
            InnerHandler = new TestServer( response ),
        };
        using var invoker = new HttpMessageInvoker( handler );

        response.Headers.Add( "api-supported-versions", ["1.0", "2.0"] );

        // act
        await invoker.SendAsync( request, cancellationToken );

        // assert
        Mock.Get( notification )
            .Verify( n => n.OnNewApiAvailableAsync( It.IsAny<ApiNotificationContext>(), cancellationToken ) );
    }
}