namespace Microsoft.AspNetCore.Mvc.Versioning.Conventions
{
    using System;
    using FluentAssertions;
    using Xunit;

    public class DefaultControllerNameConventionTest
    {
        [Theory]
        [InlineData( "Values" )]
        [InlineData( "Values2" )]
        public void normalize_name_should_trim_suffix( string controllerName )
        {
            // arrange
            var convention = new DefaultControllerNameConvention();

            // act
            var name = convention.NormalizeName( controllerName );

            // assert
            name.Should().Be( "Values" );
        }

        [Fact]
        public void group_name_should_return_original_name()
        {
            // arrange
            var convention = new DefaultControllerNameConvention();

            // act
            var name = convention.GroupName( "Values" );

            // assert
            name.Should().Be( "Values" );
        }
    }
}
