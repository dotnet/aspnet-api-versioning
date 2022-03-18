// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using System.Net.Http;

public class ConstantApiVersionSelectorTest
{
    [Fact]
    public void select_version_should_return_constant_value()
    {
        // arrange
        var request = new HttpRequestMessage();
        var version = new ApiVersion( new DateTime( 2016, 06, 22 ) );
        var selector = new ConstantApiVersionSelector( version );

        // act
        var selectedVersion = selector.SelectVersion( request, ApiVersionModel.Default );

        // assert
        selectedVersion.Should().Be( version );
    }
}