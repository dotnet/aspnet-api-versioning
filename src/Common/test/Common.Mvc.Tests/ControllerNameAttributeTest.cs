// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

public class ControllerNameAttributeTest
{
    [Fact]
    public void new_controller_name_attribute_should_set_name()
    {
        // arrange
        var expected = "Test";

        // act
        var attribute = new ControllerNameAttribute( expected );

        // assert
        attribute.Name.Should().Be( expected );
    }
}