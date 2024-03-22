// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Http;

public class ApiVersionEnumeratorTest
{
    [Fact]
    public void enumerator_should_process_single_header_value()
    {
        // arrange
        var response = new HttpResponseMessage();

        response.Headers.Add( "api-supported-versions", "1.0" );

        var enumerator = new ApiVersionEnumerator( response, "api-supported-versions" );

        // act
        var results = enumerator.ToArray();

        // assert
        results.Should().BeEquivalentTo( [new ApiVersion( 1.0 )] );
    }

    [Fact]
    public void enumerator_should_process_multiple_header_values()
    {
        // arrange
        var response = new HttpResponseMessage();

        response.Headers.Add( "api-supported-versions", ["1.0", "2.0"] );

        var enumerator = new ApiVersionEnumerator( response, "api-supported-versions" );

        // act
        var results = enumerator.ToArray();

        // assert
        results.Should().BeEquivalentTo( new ApiVersion[] { new( 1.0 ), new( 2.0 ) } );
    }

    [Theory]
    [InlineData( "1.0,2.0" )]
    [InlineData( "1.0, 2.0" )]
    [InlineData( "1.0,,2.0" )]
    [InlineData( "1.0, abc, 2.0" )]
    public void enumerator_should_process_single_header_comma_separated_values( string value )
    {
        // arrange
        var response = new HttpResponseMessage();

        response.Headers.Add( "api-supported-versions", [value] );

        var enumerator = new ApiVersionEnumerator( response, "api-supported-versions" );

        // act
        var results = enumerator.ToArray();

        // assert
        results.Should().BeEquivalentTo( new ApiVersion[] { new( 1.0 ), new( 2.0 ) } );
    }

    [Fact]
    public void enumerator_should_process_many_header_comma_separated_values()
    {
        // arrange
        const string Value = "1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 9.0, 10.0";
        var response = new HttpResponseMessage();

        response.Headers.Add( "api-supported-versions", [Value] );

        var enumerator = new ApiVersionEnumerator( response, "api-supported-versions" );

        // act
        var results = enumerator.ToArray();

        // assert
        results.Should().BeEquivalentTo(
            new ApiVersion[]
            {
                new( 1.0 ),
                new( 2.0 ),
                new( 3.0 ),
                new( 4.0 ),
                new( 5.0 ),
                new( 6.0 ),
                new( 7.0 ),
                new( 8.0 ),
                new( 9.0 ),
                new( 10.0 ),
            } );
    }

    [Fact]
    public void enumerator_should_process_multiple_header_comma_separated_values()
    {
        // arrange
        var response = new HttpResponseMessage();

        response.Headers.Add( "api-supported-versions", ["1.0, 2.0", "3.0, 4.0"] );

        var enumerator = new ApiVersionEnumerator( response, "api-supported-versions" );

        // act
        var results = enumerator.ToArray();

        // assert
        results.Should().BeEquivalentTo(
            new ApiVersion[]
            {
                new( 1.0 ),
                new( 2.0 ),
                new( 3.0 ),
                new( 4.0 ),
            } );
    }
}