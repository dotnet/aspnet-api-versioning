namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using FluentAssertions;
    using Http;
    using Moq;
    using System;
    using System.Collections.Generic;
    using Xunit;

    public class CurrentImplementationApiVersionSelectorTest
    {
        [Theory]
        [ClassData( typeof( MaxSelectVersionData ) )]
        public void select_version_should_return_max_api_version( IEnumerable<ApiVersion> supported, IEnumerable<ApiVersion> deprecated, ApiVersion version )
        {
            // arrange
            var selector = new CurrentImplementationApiVersionSelector();
            var request = new Mock<HttpRequest>().Object;
            var model = new ApiVersionModel( supported, deprecated );

            // act
            var selectedVersion = selector.SelectVersion( request, model );

            // assert
            selectedVersion.Should().Be( version );
        }
    }
}
