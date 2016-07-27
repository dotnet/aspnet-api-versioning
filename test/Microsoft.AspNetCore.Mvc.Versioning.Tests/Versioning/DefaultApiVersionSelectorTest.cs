namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using FluentAssertions;
    using Http;
    using Moq;
    using System;
    using Xunit;

    public class DefaultApiVersionSelectorTest
    {
        [Fact]
        public void select_version_should_return_default_api_version()
        {
            // arrange
            var options = new ApiVersioningOptions();
            var selector = new DefaultApiVersionSelector( options );
            var request = new Mock<HttpRequest>().Object;
            var model = ApiVersionModel.Default;
            var version = new ApiVersion( 1, 0 );

            // act
            var selectedVersion = selector.SelectVersion( request, model );

            // assert
            selectedVersion.Should().Be( version );
        }

        [Fact]
        public void select_version_should_return_updated_default_api_version()
        {
            // arrange
            var options = new ApiVersioningOptions();
            var selector = new DefaultApiVersionSelector( options );
            var request = new Mock<HttpRequest>().Object;
            var model = ApiVersionModel.Default;
            var version = new ApiVersion( 42, 0 );

            options.DefaultApiVersion = version;

            // act
            var selectedVersion = selector.SelectVersion( request, model );

            // assert
            selectedVersion.Should().Be( version );
        }
    }
}
