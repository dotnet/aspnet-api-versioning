namespace Microsoft.Web.Http.Versioning
{
    using FluentAssertions;
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
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
            var request = new HttpRequestMessage();
            var model = new ApiVersionModel( supportedVersions, deprecatedVersions );

            // act
            var selectedVersion = selector.SelectVersion( request, model );

            // assert
            selectedVersion.Should().Be( expectedVersion );
        }
    }
}