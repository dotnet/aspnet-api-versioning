// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Http;

using MELT;
using Microsoft.Extensions.Logging;

public class ApiVersionHandlerLoggerTTest
{
    [Fact]
    public async Task on_api_deprecated_should_log_message()
    {
        // arrange
        using var factory = TestLoggerFactory.Create();
        var logger = factory.CreateLogger<ApiVersionHandler>();
        var parser = ApiVersionParser.Default;
        var notification = new ApiVersionHandlerLogger<ApiVersionHandler>( logger, parser, new() );
        var response = new HttpResponseMessage()
        {
            RequestMessage = new HttpRequestMessage( HttpMethod.Get, "http://tempuri.org" ),
        };
        var context = new ApiNotificationContext( response, new ApiVersion( 1.0 ) );
        var date = DateTimeOffset.Now;
        var expected = "API version 1.0 for http://tempuri.org has been deprecated and will " +
                      $"sunset on {date.ToUniversalTime()}. Additional information: " +
                      "API Policy (en): http://tempuri.org/policy/en, " +
                      "API Política (es): http://tempuri.org/policy/es";

        response.Headers.Add( "sunset", date.ToString( "r" ) );
        response.Headers.Add( "link", "<policy/en>; rel=\"sunset\"; type=\"text/html\"; title=\"API Policy\"; hreflang=\"en\"" );
        response.Headers.Add( "link", "<policy/es>; rel=\"sunset\"; type=\"text/html\"; title=\"API Política\"; hreflang=\"es\"" );

        // act
        await notification.OnApiDeprecatedAsync( context, default );

        // assert
        var entry = factory.Sink.LogEntries.Single();

        entry.EventId.Should().Be( new EventId( 1, "ApiVersionDeprecated" ) );
        entry.Exception.Should().BeNull();
        entry.LoggerName.Should().Be( typeof( ApiVersionHandler ).FullName );
        entry.LogLevel.Should().Be( LogLevel.Warning );
        entry.Message.Should().Be( expected );
    }

    [Fact]
    public async Task on_new_api_available_should_log_message()
    {
        // arrange
        using var factory = TestLoggerFactory.Create();
        var logger = factory.CreateLogger<ApiVersionHandler>();
        var parser = ApiVersionParser.Default;
        var notification = new ApiVersionHandlerLogger<ApiVersionHandler>( logger, parser, new() );
        var response = new HttpResponseMessage()
        {
            RequestMessage = new HttpRequestMessage( HttpMethod.Get, "http://tempuri.org" ),
        };
        var context = new ApiNotificationContext( response, new ApiVersion( 1.0 ) );
        var date = DateTimeOffset.Now;
        var expected = "API version 2.0 is now available for http://tempuri.org (1.0) " +
                      $"until <unspecified>. Additional information: " +
                      "http://tempuri.org/policy";

        response.Headers.Add( "api-supported-versions", new[] { "1.0", "2.0" } );
        response.Headers.Add( "link", "<policy>; rel=\"sunset\"; type=\"text/html\"" );

        // act
        await notification.OnNewApiAvailableAsync( context, default );

        // assert
        var entry = factory.Sink.LogEntries.Single();

        entry.EventId.Should().Be( new EventId( 2, "NewApiAvailable" ) );
        entry.Exception.Should().BeNull();
        entry.LoggerName.Should().Be( typeof( ApiVersionHandler ).FullName );
        entry.LogLevel.Should().Be( LogLevel.Information );
        entry.Message.Should().Be( expected );
    }
}