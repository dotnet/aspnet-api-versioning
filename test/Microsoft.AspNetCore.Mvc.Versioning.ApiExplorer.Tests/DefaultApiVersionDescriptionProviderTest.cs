namespace Microsoft.AspNetCore.Mvc.ApiExplorer
{
    using FluentAssertions;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.ApplicationModels;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.AspNetCore.Mvc.Versioning;
    using Microsoft.Extensions.Options;
    using Moq;
    using System.Reflection;
    using Xunit;
    using static System.Linq.Enumerable;

    public class DefaultApiVersionDescriptionProviderTest
    {
        [Fact]
        public void api_version_descriptions_should_collate_expected_versions()
        {
            // arrange
            var actionProvider = new TestActionDescriptorCollectionProvider();
            var apiExplorerOptions = new OptionsWrapper<ApiExplorerOptions>( new ApiExplorerOptions() { GroupNameFormat = "'v'VVV" } );
            var descriptionProvider = new DefaultApiVersionDescriptionProvider( actionProvider, apiExplorerOptions );

            // act
            var descriptions = descriptionProvider.ApiVersionDescriptions;

            // assert
            descriptions.ShouldBeEquivalentTo(
                new[]
                {
                    new ApiVersionDescription( new ApiVersion( 0, 9 ), "v0.9", true ),
                    new ApiVersionDescription( new ApiVersion( 1, 0 ), "v1", false ),
                    new ApiVersionDescription( new ApiVersion( 2, 0 ), "v2", false ),
                    new ApiVersionDescription( new ApiVersion( 3, 0 ), "v3", false )
                } );
        }

        [Fact]
        public void is_deprecated_should_return_false_without_api_vesioning()
        {
            // arrange
            var provider = new DefaultApiVersionDescriptionProvider(
                new Mock<IActionDescriptorCollectionProvider>().Object,
                new OptionsWrapper<ApiExplorerOptions>( new ApiExplorerOptions() ) );
            var action = new ActionDescriptor();

            // act
            var result = provider.IsDeprecated( action, new ApiVersion( 1, 0 ) );

            // assert
            result.Should().BeFalse();
        }

        [Fact]
        public void is_deprecated_should_return_false_when_controller_is_versionX2Dneutral()
        {
            // arrange
            var provider = new DefaultApiVersionDescriptionProvider(
                new Mock<IActionDescriptorCollectionProvider>().Object,
                new OptionsWrapper<ApiExplorerOptions>( new ApiExplorerOptions() ) );
            var action = new ActionDescriptor();
            var controller = new ControllerModel( typeof( Controller ).GetTypeInfo(), new object[0] );

            controller.SetProperty( ApiVersionModel.Neutral );
            action.SetProperty( controller );

            // act
            var result = provider.IsDeprecated( action, new ApiVersion( 1, 0 ) );

            // assert
            result.Should().BeFalse();
        }

        [Theory]
        [InlineData( 1, true )]
        [InlineData( 2, false )]
        public void is_deprecated_should_return_expected_result_for_deprecated_version( int majorVersion, bool expected )
        {
            // arrange
            var provider = new DefaultApiVersionDescriptionProvider(
                new Mock<IActionDescriptorCollectionProvider>().Object,
                new OptionsWrapper<ApiExplorerOptions>( new ApiExplorerOptions() ) );
            var action = new ActionDescriptor();
            var controller = new ControllerModel( typeof( Controller ).GetTypeInfo(), new object[0] );
            var model = new ApiVersionModel(
                declaredVersions: new[] { new ApiVersion( 1, 0 ), new ApiVersion( 2, 0 ) },
                supportedVersions: new[] { new ApiVersion( 2, 0 ) },
                deprecatedVersions: new[] { new ApiVersion( 1, 0 ) },
                advertisedVersions: Empty<ApiVersion>(),
                deprecatedAdvertisedVersions: Empty<ApiVersion>() );

            controller.SetProperty( model );
            action.SetProperty( controller );

            // act
            var result = provider.IsDeprecated( action, new ApiVersion( majorVersion, 0 ) );

            // assert
            result.Should().Be( expected );
        }
    }
}