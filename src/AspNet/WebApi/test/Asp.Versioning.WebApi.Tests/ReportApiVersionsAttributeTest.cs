// Copyright (c) .NET Foundation and contributors. All rights reserved.

//// Ignore Spelling: Dneutral

namespace Asp.Versioning;

using Asp.Versioning.Simulators;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Http.Routing;
using static System.Linq.Enumerable;

public class ReportApiVersionsAttributeTest
{
    [Fact]
    public void on_action_executed_should_add_version_headers()
    {
        // arrange
        var attribute = new ReportApiVersionsAttribute();
        var configuration = new HttpConfiguration();
        var attributes = new Collection<IApiVersionProvider>()
        {
            new ApiVersionAttribute( "1.0" ),
            new ApiVersionAttribute( "2.0" ),
            new ApiVersionAttribute( "0.5" ) { Deprecated = true },
        };
        var controller = new TestController();
        var method = controller.GetType().GetMethod( nameof( TestController.Get ) );
        var controllerDescriptor = new Mock<HttpControllerDescriptor>( configuration, "Test", controller.GetType() ) { CallBase = true };
        var routeData = new HttpRouteData( new HttpRoute( "api/tests" ) );
        var controllerContext = new HttpControllerContext( configuration, routeData, new HttpRequestMessage() ) { Controller = controller };
        var actionDescriptor = new ReflectedHttpActionDescriptor( controllerDescriptor.Object, method );
        var actionContext = new HttpActionContext( controllerContext, actionDescriptor ) { Response = new HttpResponseMessage() };
        var context = new HttpActionExecutedContext( actionContext, null );

        configuration.AddApiVersioning( options => options.ReportApiVersions = true );
        controllerContext.Request.SetConfiguration( new() );
        controllerContext.Request.Properties["MS_HttpActionDescriptor"] = actionDescriptor;
        controllerDescriptor.Setup( cd => cd.GetCustomAttributes<IApiVersionProvider>( It.IsAny<bool>() ) ).Returns( attributes );
        actionDescriptor.Properties[typeof( ApiVersionMetadata )] = new ApiVersionMetadata(
            new ApiVersionModel(
                declaredVersions: new ApiVersion[] { new( 0, 5 ), new( 1, 0 ), new( 2, 0 ) },
                supportedVersions: new ApiVersion[] { new( 1, 0 ), new( 2, 0 ) },
                deprecatedVersions: new ApiVersion[] { new( 0, 5 ), new( 1, 0 ), new( 2, 0 ) },
                advertisedVersions: Empty<ApiVersion>(),
                deprecatedAdvertisedVersions: Empty<ApiVersion>() ),
            new ApiVersionModel(
                supportedVersions: new[] { new ApiVersion( 1, 0 ), new ApiVersion( 2, 0 ) },
                deprecatedVersions: new[] { new ApiVersion( 0, 5 ) },
                advertisedVersions: Empty<ApiVersion>(),
                deprecatedAdvertisedVersions: Empty<ApiVersion>() ) );

        // act
        attribute.OnActionExecuted( context );

        // assert
        context.Response.Headers.GetValues( "api-supported-versions" ).Single().Should().Be( "1.0, 2.0" );
        context.Response.Headers.GetValues( "api-deprecated-versions" ).Single().Should().Be( "0.5" );
    }

    [Fact]
    public void on_action_executing_should_not_add_headers_for_versionX2Dneutral_controller()
    {
        // arrange
        var attribute = new ReportApiVersionsAttribute();
        var configuration = new HttpConfiguration();
        var attributes = new Collection<IApiVersionNeutral>() { new ApiVersionNeutralAttribute() };
        var controller = new TestController();
        var method = controller.GetType().GetMethod( nameof( TestVersionNeutralController.Get ) );
        var controllerDescriptor = new Mock<HttpControllerDescriptor>( configuration, "Test", controller.GetType() ) { CallBase = true };
        var routeData = new HttpRouteData( new HttpRoute( "api/tests" ) );
        var controllerContext = new HttpControllerContext( configuration, routeData, new HttpRequestMessage() ) { Controller = new TestVersionNeutralController() };
        var actionDescriptor = new ReflectedHttpActionDescriptor( controllerDescriptor.Object, method );
        var actionContext = new HttpActionContext( controllerContext, actionDescriptor ) { Response = new HttpResponseMessage() };
        var context = new HttpActionExecutedContext( actionContext, null );

        configuration.AddApiVersioning();
        controllerDescriptor.Setup( cd => cd.GetCustomAttributes<IApiVersionNeutral>( It.IsAny<bool>() ) ).Returns( attributes );
        controllerDescriptor.Object.Properties[typeof( ApiVersionModel )] = ApiVersionModel.Neutral;
        actionDescriptor.Properties[typeof( ApiVersionMetadata )] = ApiVersionMetadata.Neutral;

        // act
        attribute.OnActionExecuted( context );

        // assert
        context.Response.Headers.Contains( "api-supported-versions" ).Should().BeFalse();
        context.Response.Headers.Contains( "api-deprecated-versions" ).Should().BeFalse();
    }
}