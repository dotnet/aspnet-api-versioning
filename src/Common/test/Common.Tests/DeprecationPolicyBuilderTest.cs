// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

public class DeprecationPolicyBuilderTest
{
    [Fact]
    public void constructor_should_not_allow_empty_name_and_version()
    {
        // arrange


        // act
        Func<DeprecationPolicyBuilder> @new = () => new DeprecationPolicyBuilder( default, default );

        // assert
        @new.Should().Throw<ArgumentException>().And
            .Message.Should().Be( "'name' and 'apiVersion' cannot both be null." );
    }

    [Fact]
    public void per_should_set_existing_immutable_policy()
    {
        // arrange
        var builder = new DeprecationPolicyBuilder( default, ApiVersion.Default );
        var policy = new DeprecationPolicy();

        // act
        builder.Per( policy );
        builder.Link( "http://tempuri.org" );

        var result = builder.Build();

        // assert
        result.Should().BeSameAs( policy );
        policy.HasLinks.Should().BeFalse();
    }

    [Fact]
    public void link_should_should_return_existing_builder()
    {
        // arrange
        var builder = new DeprecationPolicyBuilder( default, ApiVersion.Default );
        var expected = builder.Link( "http://tempuri.org" );

        // act
        var result = builder.Link( "http://tempuri.org" );

        // assert
        result.Should().BeSameAs( expected );
    }

    [Fact]
    public void build_should_construct_sunset_policy()
    {
        // arrange
        var builder = new DeprecationPolicyBuilder( default, ApiVersion.Default );

        builder.Effective( 2022, 2, 1 )
               .Link( "http://tempuri.org" );

        // act
        var policy = builder.Build();

        // assert
        policy.Should().BeEquivalentTo(
            new DeprecationPolicy(
                new DateTimeOffset( new DateTime( 2022, 2, 1 ) ),
                new LinkHeaderValue( new Uri( "http://tempuri.org" ), "sunset" ) ) );
    }
}