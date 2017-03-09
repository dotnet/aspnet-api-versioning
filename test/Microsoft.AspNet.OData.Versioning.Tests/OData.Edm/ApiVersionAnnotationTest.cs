namespace Microsoft.OData.Edm
{
    using FluentAssertions;
    using System;
    using Web.Http;
    using Xunit;

    public class ApiVersionAnnotationTest
    {
        [Fact]
        public void new_api_version_annotation_should_set_expected_version()
        {
            // arrange
            var annotatedApiVersion = new ApiVersion( 1, 1 );
            var annotation = new ApiVersionAnnotation( annotatedApiVersion );

            // act
            var apiVersion = annotation.ApiVersion;

            // assert
            apiVersion.Should().Be( annotatedApiVersion );
        }
    }
}