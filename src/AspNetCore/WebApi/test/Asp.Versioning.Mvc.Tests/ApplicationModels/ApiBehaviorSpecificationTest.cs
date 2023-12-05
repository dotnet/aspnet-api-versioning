// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0079
#pragma warning disable CA1812

namespace Asp.Versioning.ApplicationModels;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System.Reflection;

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
    private sealed class ApiBehaviorController : ControllerBase
    {
        [HttpGet]
        public OkResult Get() => Ok();
    }

    [Route( "/" )]
    private sealed class NonApiBehaviorController : Controller
    {
        [HttpGet]
        public ViewResult Index() => View();
    }
}