// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

public class ILinkBuilderExtensionsTest
{
    [Fact]
    public void link_should_build_url_from_string()
    {
        // arrange
        var builder = Mock.Of<ILinkBuilder>();

        // act
        builder.Link( "http://tempuri.org" );

        // assert
        Mock.Get( builder ).Verify( b => b.Link( new Uri( "http://tempuri.org" ) ) );
    }
}