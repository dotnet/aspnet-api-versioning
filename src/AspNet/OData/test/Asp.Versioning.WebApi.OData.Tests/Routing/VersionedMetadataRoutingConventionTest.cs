// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Routing;

using Asp.Versioning.OData;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Microsoft.OData;
using Microsoft.OData.Edm;
using System.Net.Http;
using System.Web.Http.Controllers;
using static Microsoft.OData.ServiceLifetime;

public class VersionedMetadataRoutingConventionTest
{
    [Theory]
    [MemberData( nameof( SelectControllerData ) )]
    public void select_controller_should_return_expected_name( string requestUrl, string expected )
    {
        // arrange
        var odataPath = ParseUrl( requestUrl );
        var request = new HttpRequestMessage();
        var routingConvention = new VersionedMetadataRoutingConvention();

        SetRequestContainer( request );

        // act
        var controllerName = routingConvention.SelectController( odataPath, request );

        // assert
        controllerName.Should().Be( expected );
    }

    [Theory]
    [MemberData( nameof( SelectActionData ) )]
    public void select_action_should_return_expected_name( string requestUrl, string verb, string expected )
    {
        // arrange
        var odataPath = ParseUrl( requestUrl );
        var request = new HttpRequestMessage( new HttpMethod( verb ), "http://localhost/$metadata" );
        var controllerContext = new HttpControllerContext() { Request = request };
        var actionMap = new Mock<ILookup<string, HttpActionDescriptor>>().Object;
        var routingConvention = new VersionedMetadataRoutingConvention();

        // act
        var actionName = routingConvention.SelectAction( odataPath, controllerContext, actionMap );

        // assert
        actionName.Should().Be( expected );
    }

    private readonly IODataPathHandler pathHandler = new DefaultODataPathHandler();
    private readonly IServiceProvider serviceProvider;

    public VersionedMetadataRoutingConventionTest()
    {
        var builder = new DefaultContainerBuilder();

        builder.AddDefaultODataServices();
        builder.AddService( Singleton, typeof( IEdmModel ), sp => Test.Model );
        serviceProvider = builder.BuildContainer();
    }

    private ODataPath ParseUrl( string odataPath ) => pathHandler.Parse( "http://localhost", odataPath, serviceProvider );

    private static void SetRequestContainer( HttpRequestMessage request )
    {
        const string RequestContainerKey = "Microsoft.AspNet.OData.RequestContainer";
        var selector = new Mock<IEdmModelSelector>();
        var serviceProvider = new Mock<IServiceProvider>();

        selector.SetupGet( s => s.ApiVersions ).Returns( new[] { ApiVersion.Default } );
        serviceProvider.Setup( sp => sp.GetService( typeof( IEdmModelSelector ) ) ).Returns( selector.Object );
        request.Properties[RequestContainerKey] = serviceProvider.Object;
    }

    public static IEnumerable<object[]> SelectControllerData
    {
        get
        {
            yield return new object[] { "", "VersionedMetadata" };
            yield return new object[] { "$metadata", "VersionedMetadata" };
            yield return new object[] { "Tests", null };
            yield return new object[] { "Tests/42", null };
        }
    }

    public static IEnumerable<object[]> SelectActionData
    {
        get
        {
            yield return new object[] { "", "GET", "GetServiceDocument" };
            yield return new object[] { "$metadata", "GET", "GetMetadata" };
            yield return new object[] { "$metadata", "OPTIONS", "GetOptions" };
            yield return new object[] { "Tests", "GET", null };
            yield return new object[] { "Tests/42", "GET", null };
        }
    }
}