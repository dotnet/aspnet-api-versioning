namespace Microsoft.Web.Http.Versioning
{
    using FluentAssertions;
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using Xunit;

    public class LowestImplementedApiVersionSelectorTest
    {
        [Theory]
        [ClassData( typeof( MinSelectVersionData ) )]
        public void select_version_should_return_min_api_version( IEnumerable<ApiVersion> supportedVersions, IEnumerable<ApiVersion> deprecatedVersions, ApiVersion expectedVersion )
        {
            // arrange
            var options = new ApiVersioningOptions() { DefaultApiVersion = new ApiVersion( 42, 0 ) };
            var selector = new LowestImplementedApiVersionSelector( options );
            var request = new HttpRequestMessage();
            var versionInfo = new ApiVersionModel( supportedVersions, deprecatedVersions );

            // act
            var selectedVersion = selector.SelectVersion( request, versionInfo );

            // assert
            selectedVersion.Should().Be( expectedVersion );
        }
    }
}
