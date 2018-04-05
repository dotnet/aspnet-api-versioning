namespace Microsoft.AspNet.OData.Routing
{
    using FluentAssertions;
    using Microsoft.AspNet.OData.Interfaces;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Features;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
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
            var request = Mock.Of<HttpRequest>();
            var httpContext = new Mock<HttpContext>();
            var routingConvention = new VersionedMetadataRoutingConvention();
            var items = new ActionDescriptor[]
            {
                new ControllerActionDescriptor(){ ControllerName = "VersionedMetadata", ActionName = "GetServiceDocument" },
                new ControllerActionDescriptor(){ ControllerName = "VersionedMetadata", ActionName = "GetMetadata" },
                new ControllerActionDescriptor(){ ControllerName = "VersionedMetadata", ActionName = "GetOptions" },
            };

            feature.SetupProperty( f => f.Path, odataPath );
            features.SetupGet( f => f.Get<IODataFeature>() ).Returns( feature.Object );
            actionDescriptorCollectionProvider.SetupGet( p => p.ActionDescriptors ).Returns( new ActionDescriptorCollection( items, 0 ) );
            serviceProvider.Setup( sp => sp.GetService( typeof( IActionDescriptorCollectionProvider ) ) ).Returns( actionDescriptorCollectionProvider.Object );
            request.Method = verb;
            httpContext.SetupGet( c => c.Features ).Returns( features.Object );
            httpContext.SetupGet( c => c.Request ).Returns( request );

            // act
            var actions = routingConvention.SelectAction( new RouteContext( httpContext.Object ) );

            // assert
            actions.Single().ActionName.Should().Be( expected );
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
                yield return new object[] { "Tests(42)", "GET", null };
            }
        }
    }
}