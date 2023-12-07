// Copyright (c) .NET Foundation and contributors. All rights reserved.

//// Ignore Spelling: Dneutral

namespace Asp.Versioning;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;

public class ReportApiVersionsAttributeTest
{
    [Fact]
    public async Task on_action_executing_should_add_version_headers()
    {
        // arrange
        var supported = new[] { new ApiVersion( 1, 0 ), new ApiVersion( 2, 0 ) };
        var deprecated = new[] { new ApiVersion( 0, 5 ) };
        var model = new ApiVersionModel( supported, deprecated );
        var metadata = new ApiVersionMetadata( model, model );
        var onStartResponse = new List<(Func<object, Task>, object)>();
        var context = CreateContext( metadata, onStartResponse );
        var attribute = new ReportApiVersionsAttribute();

        // act
        attribute.OnActionExecuting( context );

        for ( var i = 0; i < onStartResponse.Count; i++ )
        {
            var (callback, state) = onStartResponse[i];
            await callback( state );
        }

        // assert
        var headers = context.HttpContext.Response.Headers;

        headers["api-supported-versions"].Single().Should().Be( "1.0, 2.0" );
        headers["api-deprecated-versions"].Single().Should().Be( "0.5" );
    }

    [Fact]
    public async Task on_action_executing_should_not_add_headers_for_versionX2Dneutral_controller()
    {
        // arrange
        var onStartResponse = new List<(Func<object, Task>, object)>();
        var context = CreateContext( ApiVersionMetadata.Neutral, onStartResponse );
        var attribute = new ReportApiVersionsAttribute();

        // act
        attribute.OnActionExecuting( context );

        for ( var i = 0; i < onStartResponse.Count; i++ )
        {
            var (callback, state) = onStartResponse[i];
            await callback( state );
        }

        // assert
        var headers = context.HttpContext.Response.Headers;

        headers.ContainsKey( "api-supported-versions" ).Should().BeFalse();
        headers.ContainsKey( "api-deprecated-versions" ).Should().BeFalse();
    }

    private static ActionExecutingContext CreateContext(
        ApiVersionMetadata metadata,
        List<(Func<object, Task> Callback, object State)> onStartResponse )
    {
        var headers = new HeaderDictionary();
        var response = new Mock<HttpResponse>();
        var serviceProvider = new Mock<IServiceProvider>();
        var features = new FeatureCollection();
        var endpointFeature = new Mock<IEndpointFeature>();
        var versioningFeature = new Mock<IApiVersioningFeature>();
        var httpContext = new Mock<HttpContext>();
        var action = new ActionDescriptor();
        var actionContext = new ActionContext( httpContext.Object, new RouteData(), action );
        var filters = Array.Empty<IFilterMetadata>();
        var actionArguments = new Dictionary<string, object>();
        var controller = default( object );
        var endpoint = new Endpoint( c => Task.CompletedTask, new( new[] { metadata } ), "Test" );
        var options = Options.Create( new ApiVersioningOptions() );
        var reporter = new DefaultApiVersionReporter( new SunsetPolicyManager( options ) );

        endpointFeature.SetupProperty( f => f.Endpoint, endpoint );
        versioningFeature.SetupProperty( f => f.RequestedApiVersion, new ApiVersion( 1.0 ) );
        features.Set( endpointFeature.Object );
        features.Set( versioningFeature.Object );
        serviceProvider.Setup( sp => sp.GetService( typeof( IReportApiVersions ) ) ).Returns( reporter );
        response.SetupGet( r => r.Headers ).Returns( headers );
        response.SetupGet( r => r.HttpContext ).Returns( () => httpContext.Object );
        response.Setup( r => r.OnStarting( It.IsAny<Func<object, Task>>(), It.IsAny<object>() ) )
                .Callback( ( Func<object, Task> callback, object state ) => onStartResponse.Add( (callback, state) ) );
        httpContext.SetupGet( c => c.Response ).Returns( response.Object );
        httpContext.SetupGet( c => c.Features ).Returns( features );
        httpContext.SetupProperty( c => c.RequestServices, serviceProvider.Object );
        action.EndpointMetadata = new[] { metadata };

        return new( actionContext, filters, actionArguments, controller );
    }
}