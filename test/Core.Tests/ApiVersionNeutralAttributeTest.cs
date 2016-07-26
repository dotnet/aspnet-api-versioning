namespace Microsoft.AspNetCore.Mvc
{
    using ApplicationModels;
    using FluentAssertions;
    using System.Linq;
    using System.Reflection;
    using Versioning;
    using Xunit;
    using static System.Type;

    public class ApiVersionNeutralAttributeTest
    {
        [Fact]
        public void convention_should_apply_api_versionX2Dneutral_model()
        {
            // arrange
            var model = ApiVersionModel.Neutral;
            var type = typeof( object );
            var attributes = new object[0];
            var actionMethod = type.GetRuntimeMethod( nameof( object.ToString ), EmptyTypes );
            var controller = new ControllerModel( type.GetTypeInfo(), attributes )
            {
                Actions =
                {
                    new ActionModel( actionMethod, attributes )
                }
            };
            var convention = new ApiVersionNeutralAttribute();

            // act
            convention.Apply( controller );

            // assert
            controller.GetProperty<ApiVersionModel>().Should().BeSameAs( model );
            controller.Actions.Single().GetProperty<ApiVersionModel>().Should().BeSameAs( model );
        }
    }
}
