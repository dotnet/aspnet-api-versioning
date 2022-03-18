// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

public class ISunsetPolicyBuilderExtensionsTest
{
    [Fact]
    public void link_should_build_url_from_string()
    {
        // arrange
        var builder = Mock.Of<ISunsetPolicyBuilder>();

        // act
        builder.Link( "http://tempuri.org" );

        // assert
        Mock.Get( builder ).Verify( b => b.Link( new Uri( "http://tempuri.org" ) ) );
    }

    [Fact]
    public void effective_should_build_date_from_parts()
    {
        // arrange
        var builder = Mock.Of<ISunsetPolicyBuilder>();
        var date = new DateTime( 2022, 2, 1 );

        // act
        builder.Effective( 2022, 2, 1 );

        // assert
        Mock.Get( builder ).Verify( b => b.Effective( new( date ) ) );
    }
}