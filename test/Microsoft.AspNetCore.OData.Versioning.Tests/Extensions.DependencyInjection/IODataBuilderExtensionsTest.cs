namespace Microsoft.Extensions.DependencyInjection
{
    using FluentAssertions;
    using Microsoft.AspNet.OData.Builder;
    using Microsoft.AspNet.OData.Interfaces;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.AspNetCore.Mvc.Versioning;
    using Microsoft.AspNetCore.Mvc.Versioning.Conventions;
    using Microsoft.Extensions.Options;
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
            var mvcOptions = new MvcOptions();
            var mock = new Mock<IODataBuilder>();

            mock.SetupGet( b => b.Services ).Returns( services );

            var builder = mock.Object;

            // act
            builder.EnableApiVersioning();

            var serviceProvider = services.BuildServiceProvider();
            var mvcConfiguration = serviceProvider.GetRequiredService<IConfigureOptions<MvcOptions>>();

            mvcConfiguration.Configure( mvcOptions );

            // assert
            services.Single( sd => sd.ServiceType == typeof( IOptions<ODataApiVersioningOptions> ) ).ImplementationInstance.GetType().Should().Be( typeof( OptionsWrapper<ODataApiVersioningOptions> ) );
            services.Single( sd => sd.ServiceType == typeof( IActionSelector ) ).ImplementationType.Should().Be( typeof( ODataApiVersionActionSelector ) );
            services.Single( sd => sd.ServiceType == typeof( IODataApiVersionProvider ) ).ImplementationType.Name.Should().Be( "ODataApiVersionProvider" );
            services.Single( sd => sd.ServiceType == typeof( VersionedODataModelBuilder ) ).ImplementationType.Should().Be( typeof( VersionedODataModelBuilder ) );
            mvcOptions.Conventions.Single().GetType().Name.Should().Be( "MetadataControllerConvention" );
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
            builder.EnableApiVersioning( o => o.Conventions = customConventions );

            // assert
            var options = services.BuildServiceProvider().GetRequiredService<IOptions<ODataApiVersioningOptions>>().Value;
            options.Conventions.Should().BeSameAs( customConventions );
        }
    }
}