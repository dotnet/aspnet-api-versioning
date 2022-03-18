// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Controllers;

using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Metadata;

public class ApiVersionParameterBindingTest
{
    [Fact]
    public async Task execute_async_should_bind_parameter_value()
    {
        // arrange
        var apiVersion = new ApiVersion( 42, 0 );
        var metadataProvider = Mock.Of<ModelMetadataProvider>();
        var actionContext = NewActionContext( apiVersion );
        var parameter = NewParameter( nameof( apiVersion ) );
        var binding = new ApiVersionParameterBinding( parameter );

        // act
        await binding.ExecuteBindingAsync( metadataProvider, actionContext, CancellationToken.None );

        // assert
        actionContext.ActionArguments[nameof( apiVersion )].Should().Be( apiVersion );
    }

    [Fact]
    public async Task execute_async_should_bind_null_parameter_value()
    {
        // arrange
        var metadataProvider = Mock.Of<ModelMetadataProvider>();
        var actionContext = NewActionContext( default );
        var parameter = NewParameter( "requestedApiVersion" );
        var binding = new ApiVersionParameterBinding( parameter );

        // act
        await binding.ExecuteBindingAsync( metadataProvider, actionContext, CancellationToken.None );

        // assert
        actionContext.ActionArguments["requestedApiVersion"].Should().BeNull();
    }

    private static HttpActionContext NewActionContext( ApiVersion apiVersion )
    {
        var configuration = new HttpConfiguration();
        var request = new HttpRequestMessage();
        var controllerContext = new HttpControllerContext() { Configuration = configuration, Request = request };
        var actionContext = new HttpActionContext() { ControllerContext = controllerContext };

        request.SetConfiguration( configuration );
        request.ApiVersionProperties().RequestedApiVersion = apiVersion;

        return actionContext;
    }

    private static HttpParameterDescriptor NewParameter( string name )
    {
        var parameter = new Mock<HttpParameterDescriptor>();
        parameter.Setup( p => p.ParameterName ).Returns( name );
        return parameter.Object;
    }
}