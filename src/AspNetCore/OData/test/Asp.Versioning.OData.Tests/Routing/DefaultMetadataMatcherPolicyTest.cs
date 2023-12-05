// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Routing;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.Extensions.Options;
using Microsoft.OData.Edm;

public class DefaultMetadataMatcherPolicyTest
{
    [Fact]
    public void applies_to_endpoints_should_return_true_for_service_document()
    {
        // arrange
        var paramSource = Mock.Of<IApiVersionParameterSource>();
        var options = Options.Create( new ApiVersioningOptions() );
        var policy = new DefaultMetadataMatcherPolicy( paramSource, options );
        var metadata = new ODataRoutingMetadata( string.Empty, EdmCoreModel.Instance, [] );
        var items = new object[] { metadata };
        var endpoints = new Endpoint[] { new( Limbo, new( items ), default ) };

        // act
        var result = policy.AppliesToEndpoints( endpoints );

        // assert
        result.Should().BeTrue();
    }

    [Fact]
    public void applies_to_endpoints_should_return_true_for_metadata()
    {
        // arrange
        var paramSource = Mock.Of<IApiVersionParameterSource>();
        var options = Options.Create( new ApiVersioningOptions() );
        var policy = new DefaultMetadataMatcherPolicy( paramSource, options );
        var metadata = new ODataRoutingMetadata(
            string.Empty,
            EdmCoreModel.Instance,
            new ODataPathTemplate( MetadataSegmentTemplate.Instance ) );
        var items = new object[] { metadata };
        var endpoints = new Endpoint[] { new( Limbo, new( items ), default ) };

        // act
        var result = policy.AppliesToEndpoints( endpoints );

        // assert
        result.Should().BeTrue();
    }

    [Fact]
    public void applies_to_endpoints_should_return_false_for_normal_endpoints()
    {
        // arrange
        var paramSource = Mock.Of<IApiVersionParameterSource>();
        var options = Options.Create( new ApiVersioningOptions() );
        var policy = new DefaultMetadataMatcherPolicy( paramSource, options );
        var endpoints = new Endpoint[] { new( Limbo, new(), default ) };

        // act
        var result = policy.AppliesToEndpoints( endpoints );

        // assert
        result.Should().BeFalse();
    }

    private static Task Limbo( HttpContext context ) => Task.CompletedTask;
}