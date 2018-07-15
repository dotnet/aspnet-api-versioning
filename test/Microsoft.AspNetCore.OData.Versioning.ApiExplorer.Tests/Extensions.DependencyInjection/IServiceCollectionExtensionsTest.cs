namespace Microsoft.Extensions.DependencyInjection
{
    using FluentAssertions;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.ApiExplorer;
    using Microsoft.Extensions.Options;
    using System.Linq;
    using Xunit;

    public class IServiceCollectionExtensionsTest
    {
        [Fact]
        public void add_odata_api_explorer_should_configure_mvc()
        {
            // arrange
            var services = new ServiceCollection();
            var mvcOptions = new MvcOptions();

            services.AddODataApiExplorer();

            var serviceProvider = services.BuildServiceProvider();
            var mvcConfiguration = serviceProvider.GetRequiredService<IConfigureOptions<MvcOptions>>();

            // act
            mvcConfiguration.Configure( mvcOptions );

            // assert
            services.Single( sd => sd.ServiceType == typeof( IOptions<ApiExplorerOptions> ) ).ImplementationFactory.Should().NotBeNull();
            services.Single( sd => sd.ServiceType == typeof( IApiVersionDescriptionProvider ) ).ImplementationType.Should().Be( typeof( DefaultApiVersionDescriptionProvider ) );
            services.Single( sd => sd.ServiceType == typeof( IApiDescriptionGroupCollectionProvider ) ).ImplementationType.Should().Be( typeof( ApiDescriptionGroupCollectionProvider ) );
            services.Single( sd => sd.ImplementationType == typeof( VersionedApiDescriptionProvider ) ).ServiceType.Should().Be( typeof( IApiDescriptionProvider ) );
            services.Single( sd => sd.ImplementationType == typeof( ODataApiDescriptionProvider ) ).ServiceType.Should().Be( typeof( IApiDescriptionProvider ) );
        }
    }
}