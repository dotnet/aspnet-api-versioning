namespace Microsoft.Web.Http.Versioning.Conventions
{
    using System;
    using FluentAssertions;
    using Xunit;

    public class GroupedControllerNameConventionTest
    {
        [Fact]
        public void normalize_name_should_trim_suffix()
        {
            // arrange
            var convention = new GroupedControllerNameConvention();

            // act
            var name = convention.NormalizeName( "ValuesController" );

            // assert
            name.Should().Be( "Values" );
        }

        [Theory]
        [InlineData( "Values" )]
        [InlineData( "ValuesController2" )]
        public void normalize_name_should_not_trim_suffix( string controllerName )
        {
            // arrange
            var convention = new GroupedControllerNameConvention();

            // act
            var name = convention.NormalizeName( controllerName );

            // assert
            name.Should().Be( controllerName );
        }

        [Theory]
        [InlineData( "Values" )]
        [InlineData( "Values2" )]
        public void group_name_should_trim_trailing_numbers( string controllerName )
        {
            // arrange
            var convention = new GroupedControllerNameConvention();

            // act
            var name = convention.GroupName( controllerName );

            // assert
            name.Should().Be( "Values" );
        }
    }
}