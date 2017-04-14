namespace System.Web.Http
{
    using FluentAssertions;
    using System;
    using Xunit;

    public class HttpConfigurationExtensionsTest
    {
        [Fact]
        public void add_odata_api_explorer_should_use_default_settings()
        {
            // arrange
            var configuration = new HttpConfiguration();

            // act
            var apiExplorer = configuration.AddODataApiExplorer();

            // assert
            apiExplorer.UseApiExplorerSettings.Should().BeFalse();
        }

        [Fact]
        public void add_odata_api_explorer_should_use_api_explorer_settings_when_enabled()
        {
            // arrange
            var configuration = new HttpConfiguration();

            // act
            var apiExplorer = configuration.AddODataApiExplorer( useApiExplorerSettings: true );

            // assert
            apiExplorer.UseApiExplorerSettings.Should().BeTrue();
        }
    }
}