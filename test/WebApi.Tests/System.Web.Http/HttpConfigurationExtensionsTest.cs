namespace System.Web.Http
{
    using FluentAssertions;
    using Linq;
    using Microsoft.Web.Http;
    using Microsoft.Web.Http.Controllers;
    using Microsoft.Web.Http.Dispatcher;
    using Microsoft.Web.Http.Versioning;
    using Xunit;

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
    }
}
