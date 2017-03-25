namespace Microsoft.Web.Http.Description
{
    using FluentAssertions;
    using System;
    using System.Web.Http.Description;
    using Xunit;
    using static System.Net.Http.HttpMethod;

    public class VersionedApiDescriptionTest
    {
        [Fact]
        public void shadowed_ID_property_should_return_expected_value()
        {
            // arrange
            var apiDescription = new VersionedApiDescription()
            {
                HttpMethod = Get,
                RelativePath = "Values",
                Version = new ApiVersion( 1, 0 )
            };

            // act
            var id = apiDescription.ID;

            // assert
            id.Should().Be( "GETValues-1.0" );
        }

        [Fact]
        public void shadowed_ResponseDescription_property_should_set_internal_value()
        {
            // arrange
            var apiDescription = new VersionedApiDescription();
            var responseDescription = new ResponseDescription() { Documentation = "Test" };

            // act
            apiDescription.ResponseDescription = responseDescription;

            // assert
            apiDescription.ResponseDescription.Should().BeSameAs( responseDescription );
        }
    }
}