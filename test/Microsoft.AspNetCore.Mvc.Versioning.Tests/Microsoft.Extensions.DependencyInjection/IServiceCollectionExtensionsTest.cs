namespace Microsoft.Extensions.DependencyInjection
{
    using FluentAssertions;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.ApplicationModels;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.AspNetCore.Mvc.Routing;
    using Microsoft.AspNetCore.Mvc.Versioning;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.Options;
    using System;
    using System.Linq;
    using Xunit;

    public class IServiceCollectionExtensionsTest
    {
        [Fact]
        public void add_api_versioning_should_configure_mvc_with_default_options()
        {
            // arrange
            var services = new ServiceCollection();

            // act
            services.AddApiVersioning();

            // assert
            services.Single( sd => sd.ServiceType == typeof( IApiVersionReader ) ).ImplementationFactory.Should().NotBeNull();
            services.Single( sd => sd.ServiceType == typeof( IApiVersionSelector ) ).ImplementationFactory.Should().NotBeNull();
            services.Single( sd => sd.ServiceType == typeof( IErrorResponseProvider ) ).ImplementationFactory.Should().NotBeNull();
            services.Single( sd => sd.ServiceType == typeof( IActionSelector ) ).ImplementationType.Should().Be( typeof( ApiVersionActionSelector ) );
            services.Single( sd => sd.ServiceType == typeof( IApiVersionRoutePolicy ) ).ImplementationType.Should().Be( typeof( DefaultApiVersionRoutePolicy ) );
            services.Single( sd => sd.ServiceType == typeof( IApiControllerFilter ) ).ImplementationType.Should().Be( typeof( DefaultApiControllerFilter ) );
            services.Single( sd => sd.ServiceType == typeof( ReportApiVersionsAttribute ) ).ImplementationType.Should().Be( typeof( ReportApiVersionsAttribute ) );
            services.Single( sd => sd.ServiceType == typeof( IReportApiVersions ) ).ImplementationFactory.Should().NotBeNull();
            services.Any( sd => sd.ServiceType == typeof( IPostConfigureOptions<MvcOptions> ) && sd.ImplementationType == typeof( ApiVersioningMvcOptionsSetup ) ).Should().BeTrue();
            services.Any( sd => sd.ServiceType == typeof( IPostConfigureOptions<RouteOptions> ) && sd.ImplementationType == typeof( ApiVersioningRouteOptionsSetup ) ).Should().BeTrue();
            services.Any( sd => sd.ServiceType == typeof( IApplicationModelProvider ) && sd.ImplementationType == typeof( ApiVersioningApplicationModelProvider ) ).Should().BeTrue();
            services.Any( sd => sd.ServiceType == typeof( IActionDescriptorProvider ) && sd.ImplementationType == typeof( ApiVersionCollator ) ).Should().BeTrue();
            services.Any( sd => sd.ServiceType == typeof( IApiControllerSpecification ) && sd.ImplementationType == typeof( ApiBehaviorSpecification ) ).Should().BeTrue();
            services.Any( sd => sd.ServiceType == typeof( MatcherPolicy ) && sd.ImplementationType == typeof( ApiVersionMatcherPolicy ) ).Should().BeTrue();
        }

        [Fact]
        public void add_api_versioning_should_configure_mvc_with_custom_options()
        {
            // arrange
            var services = new ServiceCollection();

            // act
            services.AddApiVersioning(
                 options =>
                 {
                     options.ReportApiVersions = true;
                     options.ApiVersionReader = ApiVersionReader.Combine( new QueryStringApiVersionReader(), new HeaderApiVersionReader( "api-version" ) );
                     options.ApiVersionSelector = new ConstantApiVersionSelector( new ApiVersion( DateTime.Today ) );
                 } );

            // assert
            services.Single( sd => sd.ServiceType == typeof( IApiVersionReader ) ).ImplementationFactory.Should().NotBeNull();
            services.Single( sd => sd.ServiceType == typeof( IApiVersionSelector ) ).ImplementationFactory.Should().NotBeNull();
            services.Single( sd => sd.ServiceType == typeof( IErrorResponseProvider ) ).ImplementationFactory.Should().NotBeNull();
            services.Single( sd => sd.ServiceType == typeof( IActionSelector ) ).ImplementationType.Should().Be( typeof( ApiVersionActionSelector ) );
            services.Single( sd => sd.ServiceType == typeof( IApiVersionRoutePolicy ) ).ImplementationType.Should().Be( typeof( DefaultApiVersionRoutePolicy ) );
            services.Single( sd => sd.ServiceType == typeof( IApiControllerFilter ) ).ImplementationType.Should().Be( typeof( DefaultApiControllerFilter ) );
            services.Single( sd => sd.ServiceType == typeof( ReportApiVersionsAttribute ) ).ImplementationType.Should().Be( typeof( ReportApiVersionsAttribute ) );
            services.Single( sd => sd.ServiceType == typeof( IReportApiVersions ) ).ImplementationFactory.Should().NotBeNull();
            services.Any( sd => sd.ServiceType == typeof( IPostConfigureOptions<MvcOptions> ) && sd.ImplementationType == typeof( ApiVersioningMvcOptionsSetup ) ).Should().BeTrue();
            services.Any( sd => sd.ServiceType == typeof( IPostConfigureOptions<RouteOptions> ) && sd.ImplementationType == typeof( ApiVersioningRouteOptionsSetup ) ).Should().BeTrue();
            services.Any( sd => sd.ServiceType == typeof( IApplicationModelProvider ) && sd.ImplementationType == typeof( ApiVersioningApplicationModelProvider ) ).Should().BeTrue();
            services.Any( sd => sd.ServiceType == typeof( IActionDescriptorProvider ) && sd.ImplementationType == typeof( ApiVersionCollator ) ).Should().BeTrue();
            services.Any( sd => sd.ServiceType == typeof( IApiControllerSpecification ) && sd.ImplementationType == typeof( ApiBehaviorSpecification ) ).Should().BeTrue();
            services.Any( sd => sd.ServiceType == typeof( MatcherPolicy ) && sd.ImplementationType == typeof( ApiVersionMatcherPolicy ) ).Should().BeTrue();
        }
    }
}