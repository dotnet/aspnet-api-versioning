namespace Microsoft.AspNetCore.Mvc.Abstractions
{
    using FluentAssertions;
    using Microsoft.AspNetCore.Mvc.ApplicationModels;
    using Microsoft.AspNetCore.Mvc.Versioning;
    using System;
    using System.Reflection;
    using Xunit;
    using static Microsoft.AspNetCore.Mvc.Versioning.ApiVersionMapping;

    public class ActionDescriptorExtensionsTest
    {
        [Fact]
        public void action_should_be_implicitly_versioned_when_no_api_versions_have_been_mapped()
        {
            // arrange
            var controller = new ControllerModel( typeof( ControllerBase ).GetTypeInfo(), Array.Empty<object>() );
            var action = new ActionDescriptor();
            var version = new ApiVersion( 1, 0 );
            var model = new ApiVersionModel( version );

            controller.SetProperty( model );
            action.SetProperty( controller );

            // act
            var result = action.MappingTo( version );

            // assert
            result.Should().Be( Implicit );
        }

        [Fact]
        public void implicitly_versioned_action_should_not_be_mapped_to_specific_api_version()
        {
            // arrange
            var action = new ActionDescriptor();
            var version = new ApiVersion( 42, 0 );

            // act
            var result = action.MappingTo( version );

            // assert
            result.Should().Be( None );
        }

        [Fact]
        public void action_should_be_mapped_to_specific_api_version()
        {
            // arrange
            var action = new ActionDescriptor();
            var version = new ApiVersion( 42, 0 );
            var model = new ApiVersionModel( version );

            action.SetProperty( model );

            // act
            var result = action.MappingTo( version );

            // assert
            result.Should().Be( Explicit );
        }
    }
}