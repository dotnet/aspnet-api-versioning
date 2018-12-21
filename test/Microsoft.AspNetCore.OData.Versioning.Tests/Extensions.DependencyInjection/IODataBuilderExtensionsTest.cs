namespace Microsoft.Extensions.DependencyInjection
{
    using FluentAssertions;
    using Microsoft.AspNet.OData.Builder;
    using Microsoft.AspNet.OData.Interfaces;
    using Microsoft.AspNet.OData.Routing;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.ApplicationModels;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.AspNetCore.Mvc.Versioning;
    using Microsoft.AspNetCore.Mvc.Versioning.Conventions;
    using Moq;
    using System.Linq;
    using Xunit;

    public class IODataBuilderExtensionsTest
    {
        [Fact]
        public void enable_api_versioning_should_register_expected_services()
        {
            // arrange
            var services = new ServiceCollection();
            var mock = new Mock<IODataBuilder>();

            mock.SetupGet( b => b.Services ).Returns( services );

            var builder = mock.Object;

            // act
            builder.EnableApiVersioning();

            // assert
            services.Single( sd => sd.ServiceType == typeof( IActionSelector ) ).ImplementationType.Should().Be( typeof( ODataApiVersionActionSelector ) );
            services.Single( sd => sd.ServiceType == typeof( VersionedODataModelBuilder ) ).ImplementationType.Should().Be( typeof( VersionedODataModelBuilder ) );
            services.Single( sd => sd.ServiceType == typeof( IODataRouteCollectionProvider ) ).ImplementationType.Should().Be( typeof( ODataRouteCollectionProvider ) );
            services.Any( sd => sd.ServiceType == typeof( IApplicationModelProvider ) && sd.ImplementationType.Name == "ODataApplicationModelProvider" ).Should().BeTrue();
            services.Any( sd => sd.ServiceType == typeof( IActionDescriptorProvider ) && sd.ImplementationType.Name == "ODataActionDescriptorProvider" ).Should().BeTrue();
            services.Any( sd => sd.ServiceType == typeof( IActionDescriptorChangeProvider ) && sd.ImplementationInstance.GetType().Name == "ODataActionDescriptorChangeProvider" ).Should().BeTrue();
            services.Any( sd => sd.ServiceType == typeof( IApiControllerSpecification ) && sd.ImplementationType == typeof( ODataControllerSpecification ) ).Should().BeTrue();
        }

        [Fact]
        public void enable_api_versioning_should_configure_custom_options()
        {
            // arrange
            var services = new ServiceCollection();
            var customConventions = Mock.Of<ApiVersionConventionBuilder>();
            var mock = new Mock<IODataBuilder>();

            mock.SetupGet( b => b.Services ).Returns( services );

            var builder = mock.Object;

            // act
            builder.EnableApiVersioning( options => options.Conventions = customConventions );

            // assert
            services.Single( sd => sd.ServiceType == typeof( IActionSelector ) ).ImplementationType.Should().Be( typeof( ODataApiVersionActionSelector ) );
            services.Single( sd => sd.ServiceType == typeof( VersionedODataModelBuilder ) ).ImplementationType.Should().Be( typeof( VersionedODataModelBuilder ) );
            services.Single( sd => sd.ServiceType == typeof( IODataRouteCollectionProvider ) ).ImplementationType.Should().Be( typeof( ODataRouteCollectionProvider ) );
            services.Any( sd => sd.ServiceType == typeof( IApplicationModelProvider ) && sd.ImplementationType.Name == "ODataApplicationModelProvider" ).Should().BeTrue();
            services.Any( sd => sd.ServiceType == typeof( IActionDescriptorProvider ) && sd.ImplementationType.Name == "ODataActionDescriptorProvider" ).Should().BeTrue();
            services.Any( sd => sd.ServiceType == typeof( IActionDescriptorChangeProvider ) && sd.ImplementationInstance.GetType().Name == "ODataActionDescriptorChangeProvider" ).Should().BeTrue();
            services.Any( sd => sd.ServiceType == typeof( IApiControllerSpecification ) && sd.ImplementationType == typeof( ODataControllerSpecification ) ).Should().BeTrue();
        }
    }
}