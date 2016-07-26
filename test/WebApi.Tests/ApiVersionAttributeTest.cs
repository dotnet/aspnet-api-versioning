namespace Microsoft.Web.Http
{
    using FluentAssertions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Xunit;

    public class ApiVersionAttributeTest
    {
        [Fact]
        public void api_versions_attribute_should_sort_specified_versions()
        {
            // arrange
            var expected = new[] { new ApiVersion( 3, 0 ) };
            var attribute = new ApiVersionAttribute( "3.0" );

            // act
            var versions = attribute.Versions;

            // assert
            versions.Should().BeEquivalentTo( expected );
        }
    }
}
