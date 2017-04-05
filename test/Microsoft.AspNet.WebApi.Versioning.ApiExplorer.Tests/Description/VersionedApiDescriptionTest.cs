namespace Microsoft.Web.Http.Description
{
    using FluentAssertions;
    using System.Web.Http.Description;
    using Xunit;

    public class VersionedApiDescriptionTest
    {
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