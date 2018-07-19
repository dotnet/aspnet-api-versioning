namespace Microsoft.Extensions.DependencyInjection
{
    using FluentAssertions;
    using Microsoft.AspNetCore.Mvc.ApiExplorer;
    using System.Linq;
    using Xunit;

    public class IServiceCollectionExtensionsTest
    {
        [Fact]
        public void add_versioned_api_explorer_should_configure_mvc()
        {
            // arrange
            var services = new ServiceCollection();
            
            // act
            services.AddVersionedApiExplorer();

            // assert
            services.Single( sd => sd.ServiceType == typeof( IApiVersionDescriptionProvider ) ).ImplementationType.Should().Be( typeof( DefaultApiVersionDescriptionProvider ) );
            services.Single( sd => sd.ImplementationType == typeof( VersionedApiDescriptionProvider ) ).ServiceType.Should().Be( typeof( IApiDescriptionProvider ) );
        }
    }
}