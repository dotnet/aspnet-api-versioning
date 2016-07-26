namespace Microsoft.Web.Http.Versioning
{
    using FluentAssertions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using Xunit;

    public class CurrentImplementationApiVersionSelectorTest
    {
        [Theory]
        [ClassData( typeof( MaxSelectVersionData ) )]
        public void select_version_should_return_max_api_version( IEnumerable<ApiVersion> supported, IEnumerable<ApiVersion> deprecated, ApiVersion version )
        {
            // arrange
            var selector = new CurrentImplementationApiVersionSelector();
            var request = new HttpRequestMessage();
            var model = new ApiVersionModel( supported, deprecated );

            // act
            var selectedVersion = selector.SelectVersion( request, model );

            // assert
            selectedVersion.Should().Be( version );
        }
    }
}
