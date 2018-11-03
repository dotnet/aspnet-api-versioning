namespace Microsoft.AspNetCore.Mvc.ApplicationModels
{
    using FluentAssertions;
    using System.Reflection;
    using Xunit;
    using static System.Type;

    public class ModelExtensionsTest
    {
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

        sealed class TestPropertyValue { }
    }
}