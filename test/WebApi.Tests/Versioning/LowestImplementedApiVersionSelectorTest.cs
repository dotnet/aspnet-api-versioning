namespace Microsoft.Web.Http.Versioning
{
    using FluentAssertions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using Xunit;

    public class LowestImplementedApiVersionSelectorTest
    {
        [Theory]
        [ClassData( typeof( MinSelectVersionData ) )]
        public void select_version_should_return_min_api_version( IEnumerable<ApiVersion> supported, IEnumerable<ApiVersion> deprecated, ApiVersion version )
        {
            // arrange
            var selector = new LowestImplementedApiVersionSelector();
            var request = new HttpRequestMessage();
            var versionInfo = new ApiVersionModel( supported, deprecated );

            // act
            var selectedVersion = selector.SelectVersion( request, versionInfo );

            // assert
            selectedVersion.Should().Be( version );
        }
    }
}
