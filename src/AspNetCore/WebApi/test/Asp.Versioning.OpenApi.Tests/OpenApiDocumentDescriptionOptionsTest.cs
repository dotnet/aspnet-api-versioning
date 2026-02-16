// Copyright (c) .NET Foundation and contributors. All rights reserved.


namespace Asp.Versioning.OpenApi;

public class OpenApiDocumentDescriptionOptionsTest
{
    [Fact]
    public void deprecation_notice_should_be_default_without_a_date()
    {
        // arrange
        var options = new OpenApiDocumentDescriptionOptions();
        var policy = new DeprecationPolicy();

        // act
        var actual = options.DeprecationNotice( policy );

        // assert
        actual.Should().Be( "The API is deprecated." );
    }

    [Fact]
    public void deprecation_notice_should_return_expected_message()
    {
        // arrange
        var expected = "The API was deprecated on 2/8/2026.";
        var options = new OpenApiDocumentDescriptionOptions();
        var date = new DateTimeOffset( new DateTime( 2026, 2, 8 ) );
        var policy = new DeprecationPolicy( date );

        // act
        var actual = options.DeprecationNotice( policy );

        // assert
        actual.Should().Be( expected );
    }

    [Fact]
    public void sunset_notice_should_be_null_without_a_date()
    {
        // arrange
        var options = new OpenApiDocumentDescriptionOptions();
        var policy = new SunsetPolicy();

        // act
        var actual = options.SunsetNotice( policy );

        // assert
        actual.Should().BeNull();
    }

    [Fact]
    public void sunset_notice_should_return_expected_message()
    {
        // arrange
        var expected = "The API was sunset on 2/8/2026.";
        var options = new OpenApiDocumentDescriptionOptions();
        var date = new DateTimeOffset( new DateTime( 2026, 2, 8 ) );
        var policy = new SunsetPolicy( date );

        // act
        var actual = options.SunsetNotice( policy );

        // assert
        actual.Should().Be( expected );
    }
}