namespace Microsoft.AspNetCore.Mvc.ApiExplorer
{
    using FluentAssertions;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.Formatters;
    using Moq;
    using Xunit;

    public class ApiDescriptionExtensionsTest
    {
        [Fact]
        public void get_api_version_should_associated_value()
        {
            // arrange
            var version = new ApiVersion( 42, 0 );
            var description = new ApiDescription();

            description.Properties[typeof( ApiVersion )] = version;

            // act
            var value = description.GetApiVersion();

            // assert
            value.Should().Be( version );
        }

        [Fact]
        public void set_api_version_should_associate_value()
        {
            // arrange
            var version = new ApiVersion( 42, 0 );
            var description = new ApiDescription();

            description.SetApiVersion( version );

            // act
            var value = (ApiVersion) description.Properties[typeof( ApiVersion )];

            // assert
            value.Should().Be( version );
        }

        [Fact]
        public void clone_api_description_should_create_a_shallow_copy()
        {
            // arrange
            var original = new ApiDescription()
            {
                GroupName = "Test",
                HttpMethod = "GET",
                RelativePath = "test",
                ActionDescriptor = new ActionDescriptor(),
                Properties = { ["key"] = new object() },
                ParameterDescriptions = { new ApiParameterDescription() },
                SupportedRequestFormats =
                {
                    new ApiRequestFormat()
                    {
                        Formatter = new Mock<IInputFormatter>().Object,
                        MediaType = "application/json"
                    }
                },
                SupportedResponseTypes =
                {
                    new ApiResponseType()
                    {
                        ApiResponseFormats =
                        {
                            new ApiResponseFormat()
                            {
                                Formatter = new Mock<IOutputFormatter>().Object,
                                MediaType = "application/json"
                            }
                        },
                        StatusCode = 200,
                        Type = typeof( object )
                    }
                }
            };

            // act
            var clone = original.Clone();

            // assert
            clone.Should().BeEquivalentTo( original );
        }
    }
}