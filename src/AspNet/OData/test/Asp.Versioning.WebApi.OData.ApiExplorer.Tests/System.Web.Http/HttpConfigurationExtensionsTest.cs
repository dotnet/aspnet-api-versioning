// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace System.Web.Http;

using Asp.Versioning.ApiExplorer;

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
        var result = default( ODataApiExplorerOptions );

        // act
        configuration.AddODataApiExplorer(
            options =>
            {
                options.UseApiExplorerSettings = true;
                result = options;
            } );

        // assert
        result.UseApiExplorerSettings.Should().BeTrue();
    }
}