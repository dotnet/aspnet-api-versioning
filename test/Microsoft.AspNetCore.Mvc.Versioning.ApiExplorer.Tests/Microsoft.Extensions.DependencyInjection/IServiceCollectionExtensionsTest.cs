namespace Microsoft.Extensions.DependencyInjection
{
    using AspNetCore.Mvc;
    using FluentAssertions;
    using Microsoft.AspNetCore.Mvc.ApiExplorer;
    using Options;
    using System.Linq;
    using Xunit;

    public class IServiceCollectionExtensionsTest
    {
        [Fact]
        public void add_versioned_api_explorer_should_configure_mvc()
        {
            // arrange
            var services = new ServiceCollection();
            var mvcOptions = new MvcOptions();

            services.AddVersionedApiExplorer();

            var serviceProvider = services.BuildServiceProvider();
            var mvcConfiguration = serviceProvider.GetRequiredService<IConfigureOptions<MvcOptions>>();

            // act
            mvcConfiguration.Configure( mvcOptions );

            // assert
            services.Single( sd => sd.ServiceType == typeof( IOptions<ApiExplorerOptions> ) ).ImplementationFactory.Should().NotBeNull();
            services.Single( sd => sd.ServiceType == typeof( IApiVersionDescriptionProvider ) ).ImplementationType.Should().Be( typeof( DefaultApiVersionDescriptionProvider ) );
            services.Single( sd => sd.ServiceType == typeof( IApiDescriptionGroupCollectionProvider ) ).ImplementationType.Should().Be( typeof( ApiDescriptionGroupCollectionProvider ) );
            services.Single( sd => sd.ImplementationType == typeof( VersionedApiDescriptionProvider ) ).ServiceType.Should().Be( typeof( IApiDescriptionProvider ) );
        }
    }
}