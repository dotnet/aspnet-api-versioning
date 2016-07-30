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
        public void select_version_should_return_max_api_version( IEnumerable<ApiVersion> supportedVersions, IEnumerable<ApiVersion> deprecatedVersions, ApiVersion expectedVersion )
        {
            // arrange
            var options = new ApiVersioningOptions() { DefaultApiVersion = new ApiVersion( 42, 0 ) };
            var selector = new CurrentImplementationApiVersionSelector( options );
            var request = new Mock<HttpRequest>().Object;
            var model = new ApiVersionModel( supportedVersions, deprecatedVersions );

            // act
            var selectedVersion = selector.SelectVersion( request, model );

            // assert
            selectedVersion.Should().Be( expectedVersion );
        }
    }
}
