namespace Microsoft.AspNet.OData.Routing
{
    using FluentAssertions;
    using Microsoft.AspNet.OData.Interfaces;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Features;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.AspNetCore.Mvc.Versioning;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.OData;
    using Microsoft.OData.Edm;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;
    using static Microsoft.OData.ServiceLifetime;

    public class VersionedMetadataRoutingConventionTest
    {
        [Theory]
        [MemberData( nameof( SelectActionData ) )]
        public void select_action_should_return_expected_name( string requestUrl, string verb, string expected )
        {
            // arrange
            var odataPath = ParseUrl( requestUrl );
            var feature = new Mock<IODataFeature>();
            var features = new Mock<IFeatureCollection>();
            var actionDescriptorCollectionProvider = new Mock<IActionDescriptorCollectionProvider>();
            var serviceProvider = new Mock<IServiceProvider>();
            var request = new Mock<HttpRequest>();
            var httpContext = new Mock<HttpContext>();
            var routingConvention = new VersionedMetadataRoutingConvention();
            var items = new ActionDescriptor[]
            {
                new ControllerActionDescriptor(){ ControllerName = "VersionedMetadata", ActionName = "GetServiceDocument" },
                new ControllerActionDescriptor(){ ControllerName = "VersionedMetadata", ActionName = "GetMetadata" },
                new ControllerActionDescriptor(){ ControllerName = "VersionedMetadata", ActionName = "GetOptions" },
            };

            feature.SetupProperty( f => f.Path, odataPath );
            feature.SetupGet( f => f.RequestContainer ).Returns( () =>
            {
                var sp = new Mock<IServiceProvider>();
                var selector = new Mock<IEdmModelSelector>();

                selector.SetupGet( s => s.ApiVersions ).Returns( new[] { ApiVersion.Default } );
                sp.Setup( sp => sp.GetService( typeof( IEdmModelSelector ) ) ).Returns( selector.Object );

                return sp.Object;
            } );
            features.Setup( f => f.Get<IApiVersioningFeature>() ).Returns( () => new ApiVersioningFeature( httpContext.Object ) );
            features.Setup( f => f.Get<IODataFeature>() ).Returns( feature.Object );
            actionDescriptorCollectionProvider.SetupGet( p => p.ActionDescriptors ).Returns( new ActionDescriptorCollection( items, 0 ) );
            serviceProvider.Setup( sp => sp.GetService( typeof( IActionDescriptorCollectionProvider ) ) ).Returns( actionDescriptorCollectionProvider.Object );
            serviceProvider.Setup( sp => sp.GetService( typeof( IApiVersionSelector ) ) ).Returns( Mock.Of<IApiVersionSelector> );
            serviceProvider.Setup( sp => sp.GetService( typeof( IODataRouteCollectionProvider ) ) )
                           .Returns( () =>
                           {
                               var provider = new Mock<IODataRouteCollectionProvider>();
                               provider.SetupGet( p => p.Items ).Returns( Mock.Of<IODataRouteCollection> );
                               return provider.Object;
                           } );
            request.SetupProperty( r => r.Method, verb );
            request.SetupGet( r => r.HttpContext ).Returns( () => httpContext.Object );
            request.SetupProperty( r => r.Query, Mock.Of<IQueryCollection>() );
            httpContext.SetupGet( c => c.Features ).Returns( features.Object );
            httpContext.SetupProperty( c => c.RequestServices, serviceProvider.Object );
            httpContext.SetupGet( c => c.Request ).Returns( request.Object );

            // act
            var actionName = routingConvention.SelectAction( new RouteContext( httpContext.Object ) )?.SingleOrDefault()?.ActionName;

            // assert
            actionName.Should().Be( expected );
        }

        readonly IODataPathHandler pathHandler = new DefaultODataPathHandler();
        readonly IServiceProvider serviceProvider;

        public VersionedMetadataRoutingConventionTest()
        {
            var builder = new DefaultContainerBuilder();

            builder.AddDefaultODataServices();
            builder.AddService( Singleton, typeof( IEdmModel ), sp => Test.Model );
            serviceProvider = builder.BuildContainer();
        }

        ODataPath ParseUrl( string odataPath ) => pathHandler.Parse( "http://localhost", odataPath, serviceProvider );

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
}