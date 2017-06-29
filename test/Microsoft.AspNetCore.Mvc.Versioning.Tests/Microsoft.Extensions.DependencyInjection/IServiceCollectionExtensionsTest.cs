namespace Microsoft.Extensions.DependencyInjection
{
    using AspNetCore.Mvc;
    using AspNetCore.Mvc.Infrastructure;
    using AspNetCore.Mvc.Routing;
    using AspNetCore.Mvc.Versioning;
    using AspNetCore.Routing;
    using FluentAssertions;
    using Options;
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
            var mvcOptions = new MvcOptions();
            var routeOptions = new RouteOptions();

            services.AddMvc();
            services.AddApiVersioning();

            var serviceProvider = services.BuildServiceProvider();
            var mvcConfiguration = serviceProvider.GetRequiredService<IConfigureOptions<MvcOptions>>();
            var routeConfiguration = serviceProvider.GetRequiredService<IConfigureOptions<RouteOptions>>();

            // act
            mvcConfiguration.Configure( mvcOptions );
            routeConfiguration.Configure( routeOptions );

            // assert
            services.Single( sd => sd.ServiceType == typeof( IApiVersionReader ) ).ImplementationInstance.Should().NotBeNull();
            services.Single( sd => sd.ServiceType == typeof( IApiVersionSelector ) ).ImplementationInstance.Should().BeOfType<DefaultApiVersionSelector>();
            services.Single( sd => sd.ServiceType == typeof( IActionSelector ) ).ImplementationType.Should().Be( typeof( ApiVersionActionSelector ) );
            mvcOptions.Conventions.Single().Should().BeOfType<ApiVersionConvention>();
            routeOptions.ConstraintMap["apiVersion"].Should().Be( typeof( ApiVersionRouteConstraint ) );
        }

        [Fact]
        public void add_api_versioning_should_configure_mvc_with_custom_options()
        {
            // arrange
            var services = new ServiceCollection();
            var mvcOptions = new MvcOptions();
            var routeOptions = new RouteOptions();

            services.AddMvc();
            services.AddApiVersioning(
                 o =>
                 {
                     o.ReportApiVersions = true;
                     o.ApiVersionReader = ApiVersionReader.Combine( new QueryStringApiVersionReader(), new HeaderApiVersionReader( "api-version" ) );
                     o.ApiVersionSelector = new ConstantApiVersionSelector( new ApiVersion( DateTime.Today ) );
                 } );

            var serviceProvider = services.BuildServiceProvider();
            var mvcConfiguration = serviceProvider.GetRequiredService<IConfigureOptions<MvcOptions>>();
            var routeConfiguration = serviceProvider.GetRequiredService<IConfigureOptions<RouteOptions>>();

            // act
            mvcConfiguration.Configure( mvcOptions );
            routeConfiguration.Configure( routeOptions );

            // assert
            services.Single( sd => sd.ServiceType == typeof( IApiVersionReader ) ).ImplementationInstance.Should().NotBeNull();
            services.Single( sd => sd.ServiceType == typeof( IApiVersionSelector ) ).ImplementationInstance.Should().BeOfType<ConstantApiVersionSelector>();
            services.Single( sd => sd.ServiceType == typeof( IActionSelector ) ).ImplementationType.Should().Be( typeof( ApiVersionActionSelector ) );
            mvcOptions.Filters.Single().Should().BeOfType<ReportApiVersionsAttribute>();
            mvcOptions.Conventions.Single().Should().BeOfType<ApiVersionConvention>();
            routeOptions.ConstraintMap["apiVersion"].Should().Be( typeof( ApiVersionRouteConstraint ) );
        }
    }
}