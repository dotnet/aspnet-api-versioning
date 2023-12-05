// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.ApplicationModels;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Attributes;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using System.Reflection;

public class ODataControllerSpecificationTest
{
    [Theory]
    [InlineData( typeof( NormalODataController ), true )]
    [InlineData( typeof( CustomODataController ), true )]
    [InlineData( typeof( NonODataController ), false )]
    public void is_satisfied_by_should_return_expected_value( Type controllerType, bool expected )
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

#pragma warning disable IDE0079
#pragma warning disable CA1812

    private sealed class NormalODataController : ODataController
    {
        [EnableQuery]
        public OkResult Get() => Ok();
    }

    [ODataAttributeRouting]
    private sealed class CustomODataController : ControllerBase
    {
        [EnableQuery]
        public OkResult Get() => Ok();
    }

    [Route( "api/test" )]
    private sealed class NonODataController : ControllerBase
    {
        [HttpGet]
        public OkResult Get() => Ok();
    }
}