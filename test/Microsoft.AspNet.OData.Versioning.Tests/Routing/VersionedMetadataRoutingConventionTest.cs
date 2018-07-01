namespace Microsoft.AspNet.OData.Routing
{
    using FluentAssertions;
    using Microsoft.AspNet.OData;
    using Microsoft.OData;
    using Microsoft.OData.Edm;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Web.Http.Controllers;
    using Xunit;
    using static Microsoft.OData.ServiceLifetime;

    public class VersionedMetadataRoutingConventionTest
    {
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

        public static IEnumerable<object[]> SelectControllerData
        {
            get
            {
                yield return new object[] { "", "VersionedMetadata" };
                yield return new object[] { "$metadata", "VersionedMetadata" };
                yield return new object[] { "Tests", null };
                yield return new object[] { "Tests(42)", null };
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
                yield return new object[] { "Tests(42)", "GET", null };
            }
        }

        [Theory]
        [MemberData( nameof( SelectControllerData ) )]
        public void select_controller_should_return_expected_name( string requestUrl, string expected )
        {
            // arrange
            var odataPath = ParseUrl( requestUrl );
            var request = new HttpRequestMessage();
            var routingConvention = new VersionedMetadataRoutingConvention();

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
    }
}