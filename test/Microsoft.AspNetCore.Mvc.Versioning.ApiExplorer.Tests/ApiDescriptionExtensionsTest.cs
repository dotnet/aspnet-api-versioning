namespace Microsoft.AspNetCore.Mvc.ApiExplorer
{
    using FluentAssertions;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.Formatters;
    using Microsoft.AspNetCore.Mvc.Versioning;
    using Moq;
    using Xunit;

    public class ApiDescriptionExtensionsTest
    {
        [Fact]
        public void get_api_version_should_return_associated_value()
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

        [Theory]
        [InlineData( 0, 9, true )]
        [InlineData( 1, 0, false )]
        public void is_deprecated_should_match_model( int majorVersion, int minorVersion, bool expected )
        {
            // arrange
            var apiVersion = new ApiVersion( majorVersion, minorVersion );
            var model = new ApiVersionModel(
                supportedVersions: new[] { new ApiVersion( 1, 0 ) },
                deprecatedVersions: new[] { new ApiVersion( 0, 9 ) } );
            var description = new ApiDescription
            {
                ActionDescriptor = new ActionDescriptor()
                {
                    Properties = { [typeof( ApiVersionModel )] = model },
                },
                Properties =
                {
                    [typeof(ApiVersion)] = apiVersion,
                }
            };

            // act
            var deprecated = description.IsDeprecated();

            // assert
            deprecated.Should().Be( expected );
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