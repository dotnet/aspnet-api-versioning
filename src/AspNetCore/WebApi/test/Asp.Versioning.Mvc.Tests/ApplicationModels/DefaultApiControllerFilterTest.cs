// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.ApplicationModels;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System.Reflection;

public class DefaultApiControllerFilterTest
{
    [Fact]
    public void apply_should_not_filter_list_without_specifications()
    {
        // arrange
        var filter = new DefaultApiControllerFilter( Enumerable.Empty<IApiControllerSpecification>() );
        var controllerType = typeof( ControllerBase ).GetTypeInfo();
        var attributes = Array.Empty<object>();
        var controllers = new List<ControllerModel>()
            {
                new( controllerType, attributes ),
                new( controllerType, attributes ),
                new( controllerType, attributes ),
            };

        // act
        var result = filter.Apply( controllers );

        // assert
        result.Should().BeSameAs( controllers );
    }

    [Fact]
    public void apply_should_filter_controllers()
    {
        // arrange
        var specification = new Mock<IApiControllerSpecification>();
        var controllerBaseType = typeof( ControllerBase ).GetTypeInfo();
        var controllerType = typeof( Controller ).GetTypeInfo();

        specification.Setup( s => s.IsSatisfiedBy( It.Is<ControllerModel>( m => m.ControllerType.Equals( controllerBaseType ) ) ) ).Returns( true );
        specification.Setup( s => s.IsSatisfiedBy( It.Is<ControllerModel>( m => m.ControllerType.Equals( controllerType ) ) ) ).Returns( false );

        var filter = new DefaultApiControllerFilter( new[] { specification.Object } );
        var attributes = Array.Empty<object>();
        var controllers = new List<ControllerModel>()
            {
                new( controllerType, attributes ),
                new( controllerBaseType, attributes ),
                new( controllerType, attributes ),
            };

        // act
        var result = filter.Apply( controllers );

        // assert
        result.Single().Should().BeSameAs( controllers[1] );
    }
}