namespace System.Web.Http
{
    using FluentAssertions;
    using Microsoft.Web.Http.Description;
    using System;
    using Xunit;

    public class HttpConfigurationExtensionsTest
    {
        [Fact]
        public void add_odata_api_explorer_should_use_default_settings()
        {
            // arrange
            var configuration = new HttpConfiguration();
            var options = default( ODataApiExplorerOptions );

            // act
            configuration.AddODataApiExplorer( o => options = o );

            // assert
            options.UseApiExplorerSettings.Should().BeFalse();
        }

        [Fact]
        public void add_odata_api_explorer_should_use_api_explorer_settings_when_enabled()
        {
            // arrange
            var configuration = new HttpConfiguration();
            var options = default( ODataApiExplorerOptions );

            // act
            configuration.AddODataApiExplorer( o => { o.UseApiExplorerSettings = true; options = o; } );

            // assert
            options.UseApiExplorerSettings.Should().BeTrue();
        }
    }
}