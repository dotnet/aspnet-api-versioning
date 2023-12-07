// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Conventions;

public partial class OriginalControllerNameConventionTest
{
    [Theory]
    [MemberData( nameof( NormalizeNameData ) )]
    public void normalize_name_should_not_trim_suffix( string controllerName )
    {
        // arrange
        var convention = new OriginalControllerNameConvention();

        // act
        var name = convention.NormalizeName( controllerName );

        // assert
        name.Should().Be( controllerName );
    }

    [Fact]
    public void group_name_should_return_original_name()
    {
        // arrange
        var convention = new OriginalControllerNameConvention();

        // act
        var name = convention.GroupName( "Values2" );

        // assert
        name.Should().Be( "Values2" );
    }

    public static IEnumerable<object[]> NormalizeNameData
    {
        get
        {
            return new object[][]
            {
               ["Values"],
               ["Values2"],
#if NETFRAMEWORK
               ["ValuesController2"],
#endif
            };
        }
    }
}