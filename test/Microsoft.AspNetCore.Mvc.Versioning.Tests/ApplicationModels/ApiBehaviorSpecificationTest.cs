namespace Microsoft.AspNetCore.Mvc.ApplicationModels
{
    using FluentAssertions;
    using System;
    using System.Reflection;
    using Xunit;

    public class ApiBehaviorSpecificationTest
    {
        [Theory]
        [InlineData( typeof( ApiBehaviorController ), true )]
        [InlineData( typeof( NonApiBehaviorController ), false )]
        public void is_satisfied_by_should_return_expected_result( Type controllerType, bool expected )
        {
            // arrange
            var specification = new ApiBehaviorSpecification();
            var attributes = controllerType.GetCustomAttributes( inherit: false );
            var controller = new ControllerModel( controllerType.GetTypeInfo(), attributes );

            // act
            var result = specification.IsSatisfiedBy( controller );

            // assert
            result.Should().Be( expected );
        }

        [ApiController]
        [Route( "api/test" )]
        sealed class ApiBehaviorController : ControllerBase
        {
            [HttpGet]
            public IActionResult Get() => Ok();
        }

        [Route( "/" )]
        sealed class NonApiBehaviorController : Controller
        {
            [HttpGet]
            public IActionResult Index() => View();
        }
    }
}