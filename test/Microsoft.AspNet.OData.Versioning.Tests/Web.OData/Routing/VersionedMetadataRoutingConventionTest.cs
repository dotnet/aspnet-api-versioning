namespace Microsoft.Web.OData.Routing
{
    using FluentAssertions;
    using Moq;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Web.Http.Controllers;
    using System.Web.OData.Routing;
    using Xunit;

    public class VersionedMetadataRoutingConventionTest
    {
        public static IEnumerable<object[]> SelectControllerData
        {
            get
            {
                yield return new object[] { new ODataPath(), "VersionedMetadata" };
                yield return new object[] { new ODataPath( new MetadataPathSegment() ), "VersionedMetadata" };
                yield return new object[] { new ODataPath( new EntitySetPathSegment( "Tests" ) ), null };
                yield return new object[] { new ODataPath( new EntitySetPathSegment( "Tests" ), new KeyValuePathSegment( "42" ) ), null };
            }
        }

        public static IEnumerable<object[]> SelectActionData
        {
            get
            {
                yield return new object[] { new ODataPath(), "GET", "GetServiceDocument" };
                yield return new object[] { new ODataPath( new MetadataPathSegment() ), "GET", "GetMetadata" };
                yield return new object[] { new ODataPath( new MetadataPathSegment() ), "OPTIONS", "GetOptions" };
                yield return new object[] { new ODataPath( new EntitySetPathSegment( "Tests" ) ), "GET", null };
                yield return new object[] { new ODataPath( new EntitySetPathSegment( "Tests" ), new KeyValuePathSegment( "42" ) ), "GET", null };
            }
        }

        [Theory]
        [MemberData( nameof( SelectControllerData ) )]
        public void select_controller_should_return_expected_name( ODataPath odataPath, string expected )
        {
            // arrange
            var request = new HttpRequestMessage();
            var routingConvention = new VersionedMetadataRoutingConvention();

            // act
            var controllerName = routingConvention.SelectController( odataPath, request );

            // assert
            controllerName.Should().Be( expected );
        }

        [Theory]
        [MemberData( nameof( SelectActionData ) )]
        public void select_action_should_return_expected_name( ODataPath odataPath, string verb, string expected )
        {
            // arrange
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
