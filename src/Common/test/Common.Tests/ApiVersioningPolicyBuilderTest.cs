// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

public class ApiVersioningPolicyBuilderTest
{
    [Fact]
    public void sunset_should_not_allow_empty_name_and_version()
    {
        // arrange
        var builder = new ApiVersioningPolicyBuilder();

        // act
        Func<ISunsetPolicyBuilder> sunset = () => builder.Sunset( default, default );

        // assert
        sunset.Should().Throw<ArgumentException>().And
              .Message.Should().Be( "'name' and 'apiVersion' cannot both be null." );
    }

    [Theory]
    [InlineData( "Test", null )]
    [InlineData( null, 1.1 )]
    [InlineData( "Test", 1.1 )]
    public void sunset_should_return_same_policy_builder( string name, double? version )
    {
        // arrange
        var apiVersion = version is null ? default : new ApiVersion( version.Value );
        var builder = new ApiVersioningPolicyBuilder();
        var expected = builder.Sunset( name, apiVersion );

        // act
        var result = builder.Sunset( name, apiVersion );

        // assert
        result.Should().BeSameAs( expected );
    }

    [Fact]
    public void of_type_should_return_empty_list_for_unknown_type()
    {
        // arrange
        var builder = new ApiVersioningPolicyBuilder();

        // act
        var list = builder.OfType<object>();

        // assert
        list.Should().BeEmpty();
    }

    [Fact]
    public void of_type_should_return_filtered_builders()
    {
        // arrange
        var builder = new ApiVersioningPolicyBuilder();
        var expected = builder.Sunset( default, ApiVersion.Default );

        // act
        var list = builder.OfType<ISunsetPolicyBuilder>();

        // assert
        list.Single().Should().BeSameAs( expected );
    }
}