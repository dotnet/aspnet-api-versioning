// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OData;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.OData;
using System.Collections.Generic;

public class VersionedODataOptionsTest
{
    [Fact]
    public void value_should_return_options_for_current_request()
    {
        // arrange
        var context = Mock.Of<HttpContext>();
        var feature = new ApiVersioningFeature( context )
        {
            RequestedApiVersion = new( 2.0 ),
        };
        var features = new FeatureCollection();
        var serviceProvider = Mock.Of<IServiceProvider>();
        var reader = Mock.Of<IApiVersionReader>();
        var selector = new DefaultApiVersionSelector( new() );
        var accessor = Mock.Of<IHttpContextAccessor>();

        features.Set<IApiVersioningFeature>( feature );
        Mock.Get( reader ).Setup( r => r.Read( It.IsAny<HttpRequest>() ) ).Returns( new[] { "2.0" } );
        Mock.Get( serviceProvider ).Setup( sp => sp.GetService( typeof( IApiVersionReader ) ) ).Returns( reader );
        Mock.Get( serviceProvider ).Setup( sp => sp.GetService( typeof( IApiVersionParser ) ) ).Returns( ApiVersionParser.Default );
        Mock.Get( context ).SetupGet( c => c.Features ).Returns( features );
        Mock.Get( context ).SetupGet( c => c.Request ).Returns( Mock.Of<HttpRequest> );
        context.RequestServices = serviceProvider;
        accessor.HttpContext = context;

        var value = new ODataOptions();
        var options = new VersionedODataOptions( accessor, selector )
        {
            Mapping = new Dictionary<ApiVersion, ODataOptions>()
            {
                [new( 1.0 )] = new(),
                [new( 2.0 )] = value,
                [new( 3.0 )] = new(),
            },
        };

        // act
        var result = options.Value;

        // assert
        value.Should().BeSameAs( result );
    }

    [Fact]
    public void value_should_return_select_version_and_options_for_current_request()
    {
        // arrange
        var context = Mock.Of<HttpContext>();
        var feature = new ApiVersioningFeature( context );
        var features = new FeatureCollection();
        var serviceProvider = Mock.Of<IServiceProvider>();
        var reader = Mock.Of<IApiVersionReader>();
        var selector = new ConstantApiVersionSelector( new( 2.0 ) );
        var accessor = Mock.Of<IHttpContextAccessor>();

        features.Set<IApiVersioningFeature>( feature );
        Mock.Get( reader ).Setup( r => r.Read( It.IsAny<HttpRequest>() ) ).Returns( new[] { "2.0" } );
        Mock.Get( serviceProvider ).Setup( sp => sp.GetService( typeof( IApiVersionReader ) ) ).Returns( reader );
        Mock.Get( serviceProvider ).Setup( sp => sp.GetService( typeof( IApiVersionParser ) ) ).Returns( ApiVersionParser.Default );
        Mock.Get( context ).SetupGet( c => c.Features ).Returns( features );
        Mock.Get( context ).SetupGet( c => c.Request ).Returns( Mock.Of<HttpRequest> );
        context.RequestServices = serviceProvider;
        accessor.HttpContext = context;

        var value = new ODataOptions();
        var options = new VersionedODataOptions( accessor, selector )
        {
            Mapping = new Dictionary<ApiVersion, ODataOptions>()
            {
                [new( 1.0 )] = new(),
                [new( 2.0 )] = value,
                [new( 3.0 )] = new(),
            },
        };

        // act
        var result = options.Value;

        // assert
        value.Should().BeSameAs( result );
    }
}