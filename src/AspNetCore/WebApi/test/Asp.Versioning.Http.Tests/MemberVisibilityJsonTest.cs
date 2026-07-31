// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Text.Json;

public class MemberVisibilityJsonTest
{
    [Theory]
    [InlineData( "1.0", """{"id":42,"name":"Bill"}""" )]
    [InlineData( "2.0", """{"id":42,"name":"Bill","email":"bill@contoso.com"}""" )]
    [InlineData( "3.0", """{"id":42,"name":"Bill","email":"bill@contoso.com","rank":7}""" )]
    public void serialize_should_only_write_members_visible_to_api_version( string version, string expected )
    {
        // arrange
        var options = NewSerializerOptions( version );

        // act
        var json = JsonSerializer.Serialize( new Person(), options );

        // assert
        json.Should().Be( expected );
    }

    [Fact]
    public void serialize_should_filter_nested_and_repeated_members()
    {
        // arrange
        var options = NewSerializerOptions( "1.0" );
        var order = new Order();

        // act
        var json = JsonSerializer.Serialize( order, options );

        // assert
        json.Should().Be( """{"home":{"street":"1 Main St"},"people":[{"id":42,"name":"Bill"}]}""" );
    }

    [Fact]
    public void serialize_should_write_all_members_when_api_version_is_unspecified()
    {
        // arrange
        var options = NewSerializerOptions( version: default );

        // act
        var json = JsonSerializer.Serialize( new Person(), options );

        // assert
        json.Should().Be( """{"id":42,"name":"Bill","email":"bill@contoso.com","rank":7}""" );
    }

    [Fact]
    public void serialize_should_not_change_a_type_without_filtered_members()
    {
        // arrange
        var options = NewSerializerOptions( "1.0" );

        // act
        var json = JsonSerializer.Serialize( new Address(), options );

        // assert
        json.Should().Be( """{"street":"1 Main St"}""" );
    }

    [Fact]
    public void deserialize_should_reject_a_member_not_visible_to_api_version()
    {
        // arrange
        var options = NewSerializerOptions( "1.0" );
        var json = """{"id":1,"name":"Ann","email":"ann@contoso.com"}""";

        // act
        var deserialize = () => JsonSerializer.Deserialize<Person>( json, options );

        // assert
        deserialize.Should().Throw<JsonException>();
    }

    [Fact]
    public void deserialize_should_allow_a_member_visible_to_api_version()
    {
        // arrange
        var options = NewSerializerOptions( "2.0" );
        var json = """{"id":1,"name":"Ann","email":"ann@contoso.com"}""";

        // act
        var person = JsonSerializer.Deserialize<Person>( json, options );

        // assert
        person.Email.Should().Be( "ann@contoso.com" );
    }

    private static JsonSerializerOptions NewSerializerOptions( string version )
    {
        var httpContext = new DefaultHttpContext();

        if ( version is not null )
        {
            httpContext.ApiVersioningFeature.RequestedApiVersion = ApiVersionParser.Default.Parse( version );
        }

        var services = new ServiceCollection();

        services.AddLogging();
        services.AddApiVersioning();
        services.AddSingleton<IHttpContextAccessor>( new HttpContextAccessor() { HttpContext = httpContext } );

        var provider = services.BuildServiceProvider();

        return provider.GetRequiredService<IOptions<JsonOptions>>().Value.SerializerOptions;
    }

#pragma warning disable CA1812

    private sealed class Address
    {
        public string Street { get; set; } = "1 Main St";
    }

    private sealed class Person
    {
        public int Id { get; set; } = 42;

        public string Name { get; set; } = "Bill";

        [VisibleInApiVersion( "2.0" )]
        public string Email { get; set; } = "bill@contoso.com";

        [VisibleInApiVersion( "3.0" )]
        public int Rank { get; set; } = 7;
    }

    private sealed class Order
    {
        public Address Home { get; set; } = new();

        public List<Person> People { get; set; } = [new()];
    }
}