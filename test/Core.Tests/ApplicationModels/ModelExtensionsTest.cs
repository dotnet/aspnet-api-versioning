namespace Microsoft.AspNetCore.Mvc.ApplicationModels
{
    using FluentAssertions;
    using System.Collections.Generic;
    using System.Reflection;
    using Xunit;
    using static System.Type;

    public class ModelExtensionsTest
    {
        private sealed class TestPropertyValue
        {
        }

        [Fact]
        public void controller_model_should_not_have_explicit_versioning_by_default()
        {
            // arrange
            var controllerType = typeof( object ).GetTypeInfo();
            var controller = new ControllerModel( controllerType, new object[0] );

            // act
            var result = controller.HasExplicitVersioning();

            // assert
            result.Should().BeFalse();
        }

        [Fact]
        public void controller_model_with_api_version_should_have_explicit_versioning()
        {
            // arrange
            var controllerType = typeof( object ).GetTypeInfo();
            var attributes = new object[] { new ApiVersionAttribute( "42.0" ) };
            var controller = new ControllerModel( controllerType, attributes );

            // act
            var result = controller.HasExplicitVersioning();

            // assert
            result.Should().BeTrue();
        }

        [Fact]
        public void set_property_should_update_controller_model_properties()
        {
            // arrange
            var controllerType = typeof( object ).GetTypeInfo();
            var controller = new ControllerModel( controllerType, new object[0] );
            var value = new TestPropertyValue();

            // act
            controller.SetProperty( value );

            // assert
            controller.GetProperty<TestPropertyValue>().Should().BeSameAs( value );
        }

        [Fact]
        public void set_property_should_update_action_model_properties()
        {
            // arrange
            var actionMethod = typeof( object ).GetRuntimeMethod( nameof( object.ToString ), EmptyTypes );
            var action = new ActionModel( actionMethod, new object[0] );
            var value = new TestPropertyValue();

            // act
            action.SetProperty( value );

            // assert
            action.GetProperty<TestPropertyValue>().Should().BeSameAs( value );
        }
    }
}
