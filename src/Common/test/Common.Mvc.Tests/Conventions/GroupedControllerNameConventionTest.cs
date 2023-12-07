// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Conventions;

public partial class GroupedControllerNameConventionTest
{
    [Theory]
    [MemberData(nameof(NormalizeNameData))]
    public void normalize_name_should_not_trim_suffix( string controllerName )
    {
        // arrange
        var convention = new GroupedControllerNameConvention();

        // act
        var name = convention.NormalizeName( controllerName );

        // assert
        name.Should().Be( controllerName );
    }

    [Theory]
    [InlineData( "Values" )]
    [InlineData( "Values2" )]
    public void group_name_should_trim_trailing_numbers( string controllerName )
    {
        // arrange
        var convention = new GroupedControllerNameConvention();

        // act
        var name = convention.GroupName( controllerName );

        // assert
        name.Should().Be( "Values" );
    }

    public static IEnumerable<object[]> NormalizeNameData
    {
        get
        {
            return new object[][]
            {
                ["Values"],
#if NETFRAMEWORK
                ["ValuesController2"],
#else
                ["Values2"],
#endif
            };
        }
    }
}