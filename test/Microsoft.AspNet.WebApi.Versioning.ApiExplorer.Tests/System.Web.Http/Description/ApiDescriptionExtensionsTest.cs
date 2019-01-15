namespace System.Web.Http.Description
{
    using FluentAssertions;
    using Microsoft.Web.Http;
    using Microsoft.Web.Http.Description;
    using Xunit;

    public class ApiDescriptionExtensionsTest
    {
        [Fact]
        public void get_api_version_should_return_null_by_default()
        {
            // arrange
            var description = new ApiDescription();

            // act
            var apiVersion = description.GetApiVersion();

            // assert
            apiVersion.Should().BeNull();
        }

        [Fact]
        public void get_api_version_should_return_property_value()
        {
            // arrange
            var apiVersion = new ApiVersion( 1, 0 );
            var description = new VersionedApiDescription() { ApiVersion = apiVersion };

            // act
            var result = description.GetApiVersion();

            // assert
            result.Should().Be( apiVersion );
        }

        [Fact]
        public void is_deprecated_should_return_false_by_default()
        {
            // arrange
            var description = new ApiDescription();

            // act
            var deprecated = description.IsDeprecated();

            // assert
            deprecated.Should().BeFalse();
        }

        [Fact]
        public void is_deprecated_should_return_property_value()
        {
            // arrange
            var description = new VersionedApiDescription() { IsDeprecated = true };

            // act
            var deprecated = description.IsDeprecated();

            // assert
            deprecated.Should().BeTrue();
        }

        [Fact]
        public void get_group_name_should_return_null_by_default()
        {
            // arrange
            var description = new ApiDescription();

            // act
            var groupName = description.GetGroupName();

            // assert
            groupName.Should().BeNull();
        }

        [Fact]
        public void get_group_name_should_return_property_value()
        {
            // arrange
            var description = new VersionedApiDescription() { GroupName = "v1" };

            // act
            var groupName = description.GetGroupName();

            // assert
            groupName.Should().Be( "v1" );
        }
    }
}