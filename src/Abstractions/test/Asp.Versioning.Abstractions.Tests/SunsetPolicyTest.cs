// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

public class SunsetPolicyTest
{
    [Fact]
    public void links_should_not_allow_invalid_relation_type()
    {
        // arrange
        var message = "The relation type for a sunset policy link must be \"sunset\".";
#if NETFRAMEWORK
        message += $"{Environment.NewLine}Parameter name: item";
#else
        message += " (Parameter 'item')";
#endif
        var policy = new SunsetPolicy();
        var url = new Uri( "http://tempuri.org/test" );
        var header = new LinkHeaderValue( url, "test" );

        // act
        var add = () => policy.Links.Add( header );

        // assert
        add.Should().Throw<ArgumentException>().And
           .Message.Should().Be( message );
    }
}