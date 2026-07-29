// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

#if NETFRAMEWORK
using DateOnly = System.DateTime;
#endif

public class ApiVersionRangeTest
{
    [Theory]
    [InlineData( "[1.0, ]" )]
    [InlineData( "[1.0 ,]" )]
    [InlineData( "[1.0 , ]" )]
    [InlineData( "[, 1.0]" )]
    [InlineData( "[ ,1.0]" )]
    [InlineData( "[ , 1.0]" )]
    public void range_should_parse_with_inner_whitespace( string rule )
    {
        // arrange


        // act
        Action parse = () => ApiVersionRange.Parse( rule );

        // assert
        parse.Should().NotThrow();
    }

    [Theory]
    [InlineData( "1.0" )]
    [InlineData( "[1.0,)" )]
    public void range_should_contain_minimum_version_inclusive( string rule )
    {
        // arrange
        var range = ApiVersionRange.Parse( rule );

        // act
        var contains = range.Contains( new( 1.0 ) );

        // assert
        contains.Should().BeTrue();
    }

    [Fact]
    public void range_should_contain_minimum_version_exclusive()
    {
        // arrange
        var range = ApiVersionRange.Parse( "(1.0,)" );

        // act
        var contains = range.Contains( new( 1.1 ) );

        // assert
        contains.Should().BeTrue();
    }

    [Fact]
    public void range_should_contain_exact_version()
    {
        // arrange
        var range = ApiVersionRange.Parse( "[1.0]" );

        // act
        var contains = range.Contains( new( 1.0 ) );

        // assert
        contains.Should().BeTrue();
    }

    [Fact]
    public void range_should_contain_maximum_version_inclusive()
    {
        // arrange
        var range = ApiVersionRange.Parse( "(,1.0]" );

        // act
        var contains = range.Contains( new( 1.0 ) );

        // assert
        contains.Should().BeTrue();
    }

    [Fact]
    public void range_should_contain_maximum_version_exclusive()
    {
        // arrange
        var range = ApiVersionRange.Parse( "(,1.0)" );

        // act
        var contains = range.Contains( new( 0.9 ) );

        // assert
        contains.Should().BeTrue();
    }

    [Theory]
    [InlineData( 1.0 )]
    [InlineData( 1.5 )]
    [InlineData( 2.0 )]
    public void range_should_contain_exact_range_inclusive( double version )
    {
        // arrange
        var range = ApiVersionRange.Parse( "[1.0,2.0]" );

        // act
        var contains = range.Contains( new( version ) );

        // assert
        contains.Should().BeTrue();
    }

    [Fact]
    public void range_should_contain_exact_range_exclusive()
    {
        // arrange
        var range = ApiVersionRange.Parse( "(1.0,2.0)" );

        // act
        var contains = range.Contains( new( 1.5 ) );

        // assert
        contains.Should().BeTrue();
    }

    [Fact]
    public void range_should_contain_inclusive_minimum_and_exclusive_maximum()
    {
        // arrange
        var range = ApiVersionRange.Parse( "[1.0,2.0)" );

        // act
        var contains = range.Contains( new( 1.5 ) );

        // assert
        contains.Should().BeTrue();
    }

    [Fact]
    public void range_should_be_invalid()
    {
        // arrange


        // act
        Action parse = () => ApiVersionRange.Parse( "(1.0)" );

        // assert
        parse.Should().Throw<FormatException>().WithMessage( "The API version range \"(1.0)\" is invalid." );
    }

    [Theory]
    [InlineData( "1.0" )]
    [InlineData( "[1.0,)" )]
    public void range_should_not_contain_minimum_version_inclusive( string rule )
    {
        // arrange
        var range = ApiVersionRange.Parse( rule );

        // act
        var contains = range.Contains( new( 0.9 ) );

        // assert
        contains.Should().BeFalse();
    }

    [Fact]
    public void range_should_not_contain_minimum_version_exclusive()
    {
        // arrange
        var range = ApiVersionRange.Parse( "(1.0,)" );

        // act
        var contains = range.Contains( new( 1.0 ) );

        // assert
        contains.Should().BeFalse();
    }

    [Fact]
    public void range_not_should_contain_exact_version()
    {
        // arrange
        var range = ApiVersionRange.Parse( "[1.0]" );

        // act
        var contains = range.Contains( new( 1.1 ) );

        // assert
        contains.Should().BeFalse();
    }

    [Fact]
    public void range_not_should_contain_maximum_version_inclusive()
    {
        // arrange
        var range = ApiVersionRange.Parse( "(,1.0]" );

        // act
        var contains = range.Contains( new( 1.1 ) );

        // assert
        contains.Should().BeFalse();
    }

    [Fact]
    public void range_should_not_contain_maximum_version_exclusive()
    {
        // arrange
        var range = ApiVersionRange.Parse( "(,1.0)" );

        // act
        var contains = range.Contains( new( 1.0 ) );

        // assert
        contains.Should().BeFalse();
    }

    [Fact]
    public void range_should_not_contain_exact_range_inclusive()
    {
        // arrange
        var range = ApiVersionRange.Parse( "[1.0,2.0]" );

        // act
        var contains = range.Contains( new( 3.0 ) );

        // assert
        contains.Should().BeFalse();
    }

    [Theory]
    [InlineData( 1.0 )]
    [InlineData( 2.0 )]
    [InlineData( 3.0 )]
    public void range_should_not_contain_exact_range_exclusive( double version )
    {
        // arrange
        var range = ApiVersionRange.Parse( "(1.0,2.0)" );

        // act
        var contains = range.Contains( new( version ) );

        // assert
        contains.Should().BeFalse();
    }

    [Fact]
    public void range_should_not_contain_inclusive_minimum_and_exclusive_maximum()
    {
        // arrange
        var range = ApiVersionRange.Parse( "[1.0,2.0)" );

        // act
        var contains = range.Contains( new( 3.0 ) );

        // assert
        contains.Should().BeFalse();
    }

    [Fact]
    public void range_should_contain_minimum_date_version_inclusive()
    {
        // arrange
        var range = ApiVersionRange.Parse( "2026-07-01" );

        // act
        var contains = range.Contains( new( new DateOnly( 2026, 7, 1 ) ) );

        // assert
        contains.Should().BeTrue();
    }

    [Fact]
    public void range_should_not_contain_date_inclusive_minimum_and_exclusive_maximum()
    {
        // arrange
        var range = ApiVersionRange.Parse( "[2026-07-01,2027-01-01)" );

        // act
        var contains = range.Contains( new( new DateOnly( 2026, 10, 1 ) ) );

        // assert
        contains.Should().BeTrue();
    }

    [Fact]
    public void range_should_contain_minimum_version_with_status_inclusive()
    {
        // arrange
        var range = ApiVersionRange.Parse( "1.0-preview.1" );

        // act
        var contains = range.Contains( new( 1.0 ) );

        // assert
        contains.Should().BeTrue();
    }

    [Theory]
    [InlineData( 1.0 )]
    [InlineData( 3.0 )]
    public void range_should_contain_version_in_split_interval( double version )
    {
        // arrange
        var range = ApiVersionRange.Parse( "[1.0,2.0)", "(2.0,)" );

        // act
        var contains = range.Contains( new( version ) );

        // assert
        contains.Should().BeTrue();
    }

    [Fact]
    public void range_should_not_contain_version_in_split_interval()
    {
        // arrange
        var range = ApiVersionRange.Parse( "[1.0,2.0)", "(2.0,)" );

        // act
        var contains = range.Contains( new( 2.0 ) );

        // assert
        contains.Should().BeFalse();
    }

    [Fact]
    public void any_range_should_always_contain_version()
    {
        // arrange


        // act
        var contains = ApiVersionRange.Any.Contains( ApiVersion.Default );

        // assert
        contains.Should().BeTrue();
    }

    [Fact]
    public void empty_range_should_never_contain_version()
    {
        // arrange


        // act
        var contains = ApiVersionRange.Empty.Contains( ApiVersion.Default );

        // assert
        contains.Should().BeFalse();
    }
}