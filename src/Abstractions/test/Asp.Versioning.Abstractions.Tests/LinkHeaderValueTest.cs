// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using static System.UriKind;

public class LinkHeaderValueTest
{
    [Fact]
    public void to_string_should_return_relative_url()
    {
        // arrange
        var url = new Uri( "test", Relative );
        var header = new LinkHeaderValue( url, "test" );

        // act
        var value = header.ToString();

        // assert
        value.Should().Be( "</test>; rel=\"test\"" );
    }

    [Fact]
    public void to_string_should_return_absolute_url()
    {
        // arrange
        var url = new Uri( "http://tempuri.org/test", Absolute );
        var header = new LinkHeaderValue( url, "test" );

        // act
        var value = header.ToString();

        // assert
        value.Should().Be( "<http://tempuri.org/test>; rel=\"test\"" );
    }

    [Fact]
    public void to_string_should_return_all_target_attributes()
    {
        // arrange
        var url = new Uri( "http://tempuri.org/test" );
        var header = new LinkHeaderValue( url, "test" )
        {
            Language = "en",
            Media = "screen",
            Title = "Test Case",
            Type = "text/plain",
            Extensions = { ["ext"] = "42" },
        };

        // act
        var value = header.ToString();

        // assert
        value.Should().Be(
            "<http://tempuri.org/test>; rel=\"test\"; hreflang=\"en\"; media=\"screen\"; " +
            "title=\"Test Case\"; type=\"text/plain\"; ext=\"42\"" );
    }

    [Fact]
    public void to_string_should_return_add_multiple_languages()
    {
        // arrange
        var url = new Uri( "http://tempuri.org/test" );
        var header = new LinkHeaderValue( url, "test" )
        {
            Languages = { "en", "es" },
        };

        // act
        var value = header.ToString();

        // assert
        value.Should().Be( "<http://tempuri.org/test>; rel=\"test\"; hreflang=\"en\"; hreflang=\"es\"" );
    }

    [Theory]
    [InlineData( "", "The key cannot be empty." )]
    [InlineData( "42", "The first character must be a letter." )]
    [InlineData( "extra key", "Only letters, numbers, '-', and '_' are allowed." )]
    public void extension_should_not_allow_invalid_key( string key, string errorMessage )
    {
        // arrange
#if NETFRAMEWORK
        errorMessage += $"{Environment.NewLine}Parameter name: {nameof( key )}";
#else
        errorMessage += $" (Parameter '{nameof( key )}')";
#endif
        var url = new Uri( "http://tempuri.org/test" );
        var header = new LinkHeaderValue( url, "test" );

        // act
        Action addExtension = () => header.Extensions.Add( key, "any" );

        // assert
        addExtension.Should().Throw<ArgumentException>()
                    .Where( e => e.Message == errorMessage )
                    .Where( e => e.ParamName == nameof( key ) );
    }

    [Theory]
    [InlineData( "ext" )]
    [InlineData( "ext42" )]
    [InlineData( "ext-val" )]
    [InlineData( "ext-val-42" )]
    [InlineData( "ext_val" )]
    [InlineData( "ext_val_42" )]
    public void extension_should_allow_valid_keys( string key )
    {
        // arrange
        var url = new Uri( "http://tempuri.org/test" );
        var header = new LinkHeaderValue( url, "test" );

        // act
        header.Extensions.Add( key, "any" );

        // assert
        header.Extensions.Should().HaveCount( 1 );
    }

    [Fact]
    public void parse_should_process_target_link_and_relation_type()
    {
        // arrange


        // act
        var header = LinkHeaderValue.Parse( "<http://tempuri.org/test>; rel=\"test\"" );

        // assert
        header.Should().BeEquivalentTo( new LinkHeaderValue( new Uri( "http://tempuri.org/test" ), "test" ) );
    }

    [Fact]
    public void parse_should_process_all_target_attributes()
    {
        // arrange
        const string Value = "<http://tempuri.org/test>; rel=\"test\"; hreflang=\"en\"; media=\"screen\"; " +
            "title=\"Test Case\"; type=\"text/plain\"; api-version=\"2\"";

        // act
        var header = LinkHeaderValue.Parse( Value );

        // assert
        header.Should().BeEquivalentTo(
            new LinkHeaderValue( new Uri( "http://tempuri.org/test" ), "test" )
            {
                Language = "en",
                Media = "screen",
                Title = "Test Case",
                Type = "text/plain",
                Extensions = { ["api-version"] = "2" },
            } );
    }

    [Fact]
    public void parse_should_ignore_whitespace()
    {
        // arrange
        const string Value = " <http://tempuri.org/test> ; rel = \"test\" ; ";

        // act
        var header = LinkHeaderValue.Parse( Value );

        // assert
        header.Should().BeEquivalentTo( new LinkHeaderValue( new Uri( "http://tempuri.org/test" ), "test" ) );
    }

    [Fact]
    public void parse_should_resolve_relative_url()
    {
        // arrange
        var requestUrl = new Uri( "http://tempuri.org" );

        // act
        var header = LinkHeaderValue.Parse( "</test>; rel=\"test\"", url => new( requestUrl, url ) );

        // assert
        header.Should().BeEquivalentTo( new LinkHeaderValue( new Uri( "http://tempuri.org/test" ), "test" ) );
    }

    [Fact]
    public void parse_list_should_return_list()
    {
        // arrange
        var input = new[]
        {
            "</test1>; rel=\"test\"",
            "</test2>; rel=\"test\"",
            "</test3>; rel=\"test\"",
        };

        // act
        var list = LinkHeaderValue.ParseList( input );

        // assert
        list.Should().HaveCount( 3 );
    }

    [Fact]
    public void parse_should_fail_when_input_is_invalid()
    {
        // arrange


        // act
        var parse = () => LinkHeaderValue.Parse( "</test>" );

        // assert
        parse.Should().Throw<FormatException>();
    }

    [Fact]
    public void parse_list_should_fail_when_any_input_is_invalid()
    {
        // arrange
        var input = new[]
        {
            "</test1>; rel=\"test\"",
            "</test2>; ",
            "</test3>; rel=\"test\"",
        };

        // act
        var parseList = () => LinkHeaderValue.ParseList( input );

        // assert
        parseList.Should().Throw<FormatException>();
    }

    [Fact]
    public void try_parse_should_handle_missing_relation_type()
    {
        // arrange


        // act
        var result = LinkHeaderValue.TryParse( "</test>", default, out var header );

        // assert
        result.Should().BeFalse();
        header.Should().BeNull();
    }

    [Fact]
    public void try_parse_list_should_skip_invalid_input()
    {
        // arrange
        var input = new[]
        {
            "<http://tempuri.org/1>; rel=\"test\"",
            "<http://tempuri.org/2>; ",
            "<http://tempuri.org/3>; rel=\"test\"",
        };

        // act
        var result = LinkHeaderValue.TryParseList( input, default, out var list );

        // assert
        result.Should().BeTrue();
        list.Should().BeEquivalentTo(
            new LinkHeaderValue[]
            {
                new( new( "http://tempuri.org/1" ), "test" ),
                new( new( "http://tempuri.org/3" ), "test" ),
            } );
    }
}