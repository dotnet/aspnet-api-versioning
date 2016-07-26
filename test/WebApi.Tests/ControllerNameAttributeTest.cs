namespace Microsoft.Web.Http
{
    using FluentAssertions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Xunit;

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
}
