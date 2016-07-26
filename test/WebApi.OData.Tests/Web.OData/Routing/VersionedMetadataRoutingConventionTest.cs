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

    /// <summary>
    /// Provides unit tests for <see cref="VersionedMetadataRoutingConvention"/>.
    /// </summary>
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
                yield return new object[] { new ODataPath(), "GetServiceDocument" };
                yield return new object[] { new ODataPath( new MetadataPathSegment() ), "GetMetadata" };
                yield return new object[] { new ODataPath( new EntitySetPathSegment( "Tests" ) ), null };
                yield return new object[] { new ODataPath( new EntitySetPathSegment( "Tests" ), new KeyValuePathSegment( "42" ) ), null };
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
        public void select_action_should_return_expected_name( ODataPath odataPath, string expected )
        {
            // arrange
            var controllerContext = new HttpControllerContext();
            var actionMap = new Mock<ILookup<string, HttpActionDescriptor>>().Object;
            var routingConvention = new VersionedMetadataRoutingConvention();

            // act
            var actionName = routingConvention.SelectAction( odataPath, controllerContext, actionMap );

            // assert
            actionName.Should().Be( expected );
        }
    }
}
