// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace System.Web.Http;

using Asp.Versioning;
using Asp.Versioning.Controllers;
using Asp.Versioning.Dispatcher;

public class HttpConfigurationExtensionsTest
{
    [Fact]
    public void add_api_versioning_should_setup_configuration_with_default_options()
    {
        // arrange
        var configuration = new HttpConfiguration();

        // act
        configuration.AddApiVersioning();

        // assert
        configuration.Services.GetHttpControllerSelector().Should().BeOfType<ApiVersionControllerSelector>();
        configuration.Services.GetActionSelector().Should().BeOfType<ApiVersionActionSelector>();
        configuration.Filters.Should().HaveCount( 0 );
    }

    [Fact]
    public void add_api_versioning_should_report_api_versions_when_option_is_enabled()
    {
        // arrange
        var configuration = new HttpConfiguration();

        // act
        configuration.AddApiVersioning( o => o.ReportApiVersions = true );

        // assert
        configuration.Services.GetHttpControllerSelector().Should().BeOfType<ApiVersionControllerSelector>();
        configuration.Services.GetActionSelector().Should().BeOfType<ApiVersionActionSelector>();
        configuration.Filters.Single().Instance.Should().BeOfType<ReportApiVersionsAttribute>();
    }

    [Fact]
    public void add_api_versioning_should_not_allow_default_neutral_api_version()
    {
        // arrange
        var configuration = new HttpConfiguration();

        // act
        Action options = () => configuration.AddApiVersioning( options => options.DefaultApiVersion = ApiVersion.Neutral );

        // assert
        options.Should().Throw<InvalidOperationException>();
    }
}