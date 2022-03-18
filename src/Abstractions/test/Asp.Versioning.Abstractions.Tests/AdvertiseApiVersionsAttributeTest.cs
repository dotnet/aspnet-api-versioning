// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using static Asp.Versioning.ApiVersionProviderOptions;

public class AdvertiseApiVersionsAttributeTest
{
    [Theory]
    [InlineData( false, Advertised )]
    [InlineData( true, Advertised | Deprecated )]
    public void new_advertise_api_versions_attribute_should_have_expected_options( bool deprecated, ApiVersionProviderOptions expected )
    {
        // arrange
        IApiVersionProvider attribute = new AdvertiseApiVersionsAttribute( 42.0 ) { Deprecated = deprecated };

        // act
        var options = attribute.Options;

        // assert
        options.Should().Be( expected );
    }
}