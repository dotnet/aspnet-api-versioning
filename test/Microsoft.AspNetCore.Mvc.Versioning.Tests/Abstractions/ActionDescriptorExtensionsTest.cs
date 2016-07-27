namespace Microsoft.AspNetCore.Mvc.Abstractions
{
    using ApplicationModels;
    using FluentAssertions;
    using System.Reflection;
    using Versioning;
    using Xunit;

    public class ActionDescriptorExtensionsTest
    {
        [Fact]
        public void action_should_be_implicitly_versioned_when_no_api_versions_have_been_mapped()
        {
            // arrange
            var version = new ApiVersion( 1, 0 );
            var controller = new ControllerModel( typeof( object ).GetTypeInfo(), new object[0] );
            var action = new ActionDescriptor();

            controller.SetProperty( new ApiVersionModel( version ) );
            action.SetProperty( controller );

            // act
            var result = action.IsImplicitlyMappedTo( version );

            // assert
            result.Should().BeTrue();
        }

        [Fact]
        public void action_should_not_be_implicitly_versioned_when_api_versions_have_been_mapped()
        {
            // arrange
            var version = new ApiVersion( 42, 0 );
            var action = new ActionDescriptor();
            var model = new ApiVersionModel( version );

            action.SetProperty( model );

            // act
            var result = action.IsImplicitlyMappedTo( version );
            
            // assert
            result.Should().BeFalse();
        }

        [Fact]
        public void implicitly_versioned_action_should_not_be_mapped_to_specific_api_version()
        {
            // arrange
            var action = new ActionDescriptor();
            var version = new ApiVersion( 42, 0 );

            // act
            var result = action.IsMappedTo( version );

            // assert
            result.Should().BeFalse();
        }

        [Fact]
        public void action_should_be_mapped_to_specific_api_version()
        {
            // arrange
            var action = new ActionDescriptor();
            var version = new ApiVersion( 42, 0 );
            var model = new ApiVersionModel( version );

            // act
            action.SetProperty( model );

            // assert
            action.IsMappedTo( version ).Should().BeTrue();
        }

        [Fact]
        public void action_should_be_api_versionX2Dneutral_without_a_model()
        {
            // arrange
            var action = new ActionDescriptor();

            // act
            var result = action.IsApiVersionNeutral();

            // assert
            result.Should().BeTrue();
        }

        [Fact]
        public void action_should_be_api_versionX2Dneutral_with_a_neutral_model()
        {
            // arrange
            var action = new ActionDescriptor();
            var model = ApiVersionModel.Neutral;

            action.SetProperty( model );

            // act
            var result = action.IsApiVersionNeutral();

            // assert
            result.Should().BeTrue();
        }

        [Fact]
        public void action_should_be_api_versionX2Dneutral_with_a_versioned_model()
        {
            // arrange
            var action = new ActionDescriptor();
            var model = new ApiVersionModel( new ApiVersion( 42, 0 ) );

            action.SetProperty( model );

            // act
            var result = action.IsApiVersionNeutral();

            // assert
            result.Should().BeFalse();
        }
    }
}