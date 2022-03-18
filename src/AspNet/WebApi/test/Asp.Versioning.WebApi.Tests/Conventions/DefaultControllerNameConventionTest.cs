// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Conventions;

public partial class DefaultControllerNameConventionTest
{
    [Theory]
    [InlineData( "Values" )]
    [InlineData( "ValuesController2" )]
    public void normalize_name_should_not_trim_suffix( string controllerName )
    {
        // arrange
        var convention = new DefaultControllerNameConvention();

        // act
        var name = convention.NormalizeName( controllerName );

        // assert
        name.Should().Be( controllerName );
    }
}