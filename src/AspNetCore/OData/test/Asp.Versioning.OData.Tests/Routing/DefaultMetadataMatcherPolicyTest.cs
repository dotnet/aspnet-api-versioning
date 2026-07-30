// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Routing;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.Options;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

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

    [Fact]
    public void get_edges_should_only_contain_metadata_endpoints()
    {
        // arrange
        var paramSource = Mock.Of<IApiVersionParameterSource>();
        var options = Options.Create( new ApiVersioningOptions() );
        var policy = new DefaultMetadataMatcherPolicy( paramSource, options );
        var serviceDocument = NewMetadataEndpoint( "api", new ApiVersion( 1.0 ) );
        var endpoints = new Endpoint[] { serviceDocument };

        // act
        var edges = policy.GetEdges( endpoints );

        // assert
        edges.Single().Endpoints.Should().Equal( serviceDocument );
    }

    // a node normally only contains the service document and/or $metadata, but it can also contain
    // unrelated endpoints when another route template has a non-literal segment in the same position.
    // an endpoint that is excluded from all edges is dropped from the matcher and becomes unreachable
    [Fact]
    public void get_edges_should_not_drop_unrelated_endpoints()
    {
        // arrange
        var paramSource = Mock.Of<IApiVersionParameterSource>();
        var options = Options.Create( new ApiVersioningOptions() );
        var policy = new DefaultMetadataMatcherPolicy( paramSource, options );
        var serviceDocument = NewMetadataEndpoint( "api/v{version:apiVersion}", new ApiVersion( 1.0 ) );
        var entity = NewEntityEndpoint( "api/Tests({key})", new ApiVersion( 1.0 ) );
        var endpoints = new Endpoint[] { serviceDocument, entity };

        // act
        var edges = policy.GetEdges( endpoints );

        // assert
        edges.Single().Endpoints.Should().Equal( serviceDocument, entity );
    }

    private static RouteEndpoint NewMetadataEndpoint( string template, ApiVersion apiVersion ) =>
        NewEndpoint( template, apiVersion, new ODataRoutingMetadata( string.Empty, EdmCoreModel.Instance, [] ) );

    private static RouteEndpoint NewEntityEndpoint( string template, ApiVersion apiVersion )
    {
        var builder = new ODataConventionModelBuilder();

        builder.EntitySet<TestEntity>( "Tests" );

        var edm = builder.GetEdmModel();
        var entitySet = edm.EntityContainer.FindEntitySet( "Tests" );

        return NewEndpoint(
            template,
            apiVersion,
            new ODataRoutingMetadata(
                string.Empty,
                edm,
                new ODataPathTemplate( new EntitySetSegmentTemplate( entitySet ) ) ) );
    }

    private static RouteEndpoint NewEndpoint( string template, ApiVersion apiVersion, IODataRoutingMetadata odata )
    {
        var model = new ApiVersionModel( apiVersion );
        var items = new object[] { odata, new ApiVersionMetadata( model, model ) };

        return new( Limbo, RoutePatternFactory.Parse( template ), 0, new( items ), template );
    }

    private static Task Limbo( HttpContext context ) => Task.CompletedTask;
}