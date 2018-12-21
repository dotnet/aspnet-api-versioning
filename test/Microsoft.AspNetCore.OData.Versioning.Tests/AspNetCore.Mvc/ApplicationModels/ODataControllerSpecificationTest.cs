namespace Microsoft.AspNetCore.Mvc.ApplicationModels
{
    using FluentAssertions;
    using Microsoft.AspNet.OData;
    using Microsoft.AspNet.OData.Routing;
    using System;
    using System.Reflection;
    using Xunit;

    public class ODataControllerSpecificationTest
    {
        [Theory]
        [InlineData( typeof( NormalODataController ), true )]
        [InlineData( typeof( CustomODataController ), true )]
        [InlineData( typeof( NonODataController ), false )]
        public void is_satisified_by_should_return_expected_value( Type controllerType, bool expected )
        {
            // arrange
            var specification = new ODataControllerSpecification();
            var attributes = controllerType.GetCustomAttributes( inherit: true );
            var controller = new ControllerModel( controllerType.GetTypeInfo(), attributes );

            // act
            var result = specification.IsSatisfiedBy( controller );

            // assert
            result.Should().Be( expected );
        }

        [ODataRoutePrefix( "Normal" )]
        sealed class NormalODataController : ODataController
        {
            [ODataRoute]
            public IActionResult Get() => Ok();
        }

        [ODataRouting]
        [ODataFormatting]
        [ODataRoutePrefix( "Custom" )]
        sealed class CustomODataController : ControllerBase
        {
            [ODataRoute]
            public IActionResult Get() => Ok();
        }

        [Route( "api/test" )]
        sealed class NonODataController : ControllerBase
        {
            [HttpGet]
            public IActionResult Get() => Ok();
        }
    }
}