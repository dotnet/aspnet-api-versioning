// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.ApiExplorer;

using Asp.Versioning.Grpc.Tests;
using Asp.Versioning.Routing;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;

public class GrpcJsonTranscodingDescriptionProviderTest
{
    [Fact]
    public async Task provider_should_describe_all_transcoded_methods()
    {
        // arrange

        // act
        var descriptions = await TestApplication.DescribeApisAsync();

        // assert
        descriptions.Select( description => $"{description.HttpMethod} {description.RelativePath}" )
                    .Should()
                    .BeEquivalentTo(
                        "GET api/orders/{id}",
                        "POST api/orders",
                        "PUT api/orders/{id}",
                        "GET api/v{api-version}/orders/{id}" );
    }

    [Fact]
    public async Task provider_should_describe_action_from_service_and_method()
    {
        // arrange

        // act
        var description = await TestApplication.DescribeApiAsync( "GET", "api/orders/{id}" );

        // assert
        var action = description.ActionDescriptor.Should().BeOfType<ControllerActionDescriptor>().Subject;

        action.ControllerName.Should().Be( "Orders" );
        action.ActionName.Should().Be( "GetOrder" );
        action.ControllerTypeInfo.AsType().Should().Be<TestOrdersService>();
        action.RouteValues["controller"].Should().Be( "Orders" );
        action.RouteValues["action"].Should().Be( "GetOrder" );
    }

    [Fact]
    public async Task provider_should_describe_route_parameter()
    {
        // arrange

        // act
        var description = await TestApplication.DescribeApiAsync( "GET", "api/orders/{id}" );

        // assert
        var parameter = description.ParameterDescriptions.Single( p => p.Source == BindingSource.Path );

        parameter.Name.Should().Be( "id" );
        parameter.Type.Should().Be<int>();
        parameter.IsRequired.Should().BeTrue();
        parameter.ModelMetadata.ModelType.Should().Be<int>();
        parameter.ParameterDescriptor.ParameterType.Should().Be<int>();
    }

    [Fact]
    public async Task provider_should_describe_query_parameters()
    {
        // arrange

        // act
        var description = await TestApplication.DescribeApiAsync( "GET", "api/orders/{id}" );

        // assert
        var parameters = description.ParameterDescriptions.Where( p => p.Source == BindingSource.Query );

        parameters.Select( p => p.Name ).Should().BeEquivalentTo( "api-version", "includeLineItems" );
        parameters.Should().AllSatisfy( p => p.Type.Should().NotBeNull() );
    }

    // a request body schema is generated from ApiParameterDescription.Type verbatim; unlike a path or query
    // parameter, there is no fall back to ModelMetadata.ModelType. a null value fails schema generation
    [Fact]
    public async Task provider_should_describe_body_parameter_with_type()
    {
        // arrange

        // act
        var description = await TestApplication.DescribeApiAsync( "POST", "api/orders" );

        // assert
        var parameter = description.ParameterDescriptions.Single( p => p.Source == BindingSource.Body );

        parameter.Type.Should().Be<Order>();
        parameter.ModelMetadata.ModelType.Should().Be<Order>();
    }

    [Fact]
    public async Task provider_should_describe_wildcard_body_parameter()
    {
        // arrange

        // act
        var description = await TestApplication.DescribeApiAsync( "PUT", "api/orders/{id}" );

        // assert
        var parameter = description.ParameterDescriptions.Single( p => p.Source == BindingSource.Body );

        parameter.Type.Should().Be<OrderRequest>();
        parameter.ParameterDescriptor.Should()
                 .BeOfType<ControllerParameterDescriptor>()
                 .Which.ParameterInfo.Should().NotBeNull();
    }

    [Fact]
    public async Task provider_should_not_describe_query_parameters_for_wildcard_body()
    {
        // arrange

        // act
        var description = await TestApplication.DescribeApiAsync( "PUT", "api/orders/{id}" );

        // assert
        description.ParameterDescriptions.Should().NotContain( p => p.Source == BindingSource.Query );
    }

    [Fact]
    public async Task provider_should_describe_api_version_route_parameter()
    {
        // arrange

        // act
        var description = await TestApplication.DescribeApiAsync( "GET", "api/v{api-version}/orders/{id}" );

        // assert
        var parameter = description.ParameterDescriptions.Single( p => p.Name == "api-version" );

        parameter.Source.Should().Be( BindingSource.Path );
        parameter.ModelMetadata.DataTypeName.Should().Be( "ApiVersion" );
        parameter.RouteInfo.Constraints.Should().ContainItemsAssignableTo<ApiVersionRouteConstraint>();
    }

    [Fact]
    public async Task provider_should_describe_unwrapped_response_body()
    {
        // arrange

        // act
        var description = await TestApplication.DescribeApiAsync( "GET", "api/orders/{id}" );

        // assert
        var response = description.SupportedResponseTypes.Single( r => r.StatusCode == 200 );

        response.Type.Should().Be<Order>();
        response.ApiResponseFormats.Single().MediaType.Should().Be( "application/json" );
    }

    [Fact]
    public async Task provider_should_describe_message_response_when_not_unwrapped()
    {
        // arrange

        // act
        var description = await TestApplication.DescribeApiAsync( "POST", "api/orders" );

        // assert
        description.SupportedResponseTypes
                   .Single( r => r.StatusCode == 200 )
                   .Type.Should().Be<OrderReply>();
    }

    [Fact]
    public async Task provider_should_describe_default_response_as_status()
    {
        // arrange

        // act
        var description = await TestApplication.DescribeApiAsync( "GET", "api/orders/{id}" );

        // assert
        var response = description.SupportedResponseTypes.Single( r => r.IsDefaultResponse );

        response.Type.Should().Be<Google.Rpc.Status>();
        response.ModelMetadata.ModelType.Should().Be<Google.Rpc.Status>();
    }

    [Fact]
    public async Task provider_should_describe_empty_response()
    {
        // arrange

        // act
        var description = await TestApplication.DescribeApiAsync( "PUT", "api/orders/{id}" );

        // assert
        description.SupportedResponseTypes
                   .Single( r => r.StatusCode == 200 )
                   .Type.Should().Be<Empty>();
    }
}