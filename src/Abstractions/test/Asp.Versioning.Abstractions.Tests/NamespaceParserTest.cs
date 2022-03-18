// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using System.Reflection;
#if NETFRAMEWORK
using DateOnly = System.DateTime;
#endif

public class NamespaceParserTest
{
    [Theory]
    [MemberData( nameof( NamespaceWithOneVersion ) )]
    public void parse_should_return_single_version( Type type, ApiVersion expected )
    {
        // arrange


        // act
        var result = NamespaceParser.Default.Parse( type );

        // assert
        result.Should().Equal( expected );
    }

    [Theory]
    [MemberData( nameof( NamespaceWithMultipleVersions ) )]
    public void parse_should_return_multiple_versions( Type type, ApiVersion[] expected )
    {
        // arrange


        // act
        var result = NamespaceParser.Default.Parse( type );

        // assert
        result.Should().Equal( expected );
    }

    [Fact]
    public void parse_should_return_no_versions()
    {
        // arrange
        var type = new TestType( "Contoso.Api.Controllers" );

        // act
        var result = NamespaceParser.Default.Parse( type );

        // assert
        result.Should().BeEmpty();
    }

    public static IEnumerable<object[]> NamespaceWithOneVersion
    {
        get
        {
            yield return new object[] { new TestType( "v1" ), new ApiVersion( 1 ) };
            yield return new object[] { new TestType( "v1RC" ), new ApiVersion( 1, 0, "RC" ) };
            yield return new object[] { new TestType( "v20180401" ), new ApiVersion( new DateOnly( 2018, 4, 1 ) ) };
            yield return new object[] { new TestType( "v20180401_Beta" ), new ApiVersion( new DateOnly( 2018, 4, 1 ), "Beta" ) };
            yield return new object[] { new TestType( "v20180401Beta" ), new ApiVersion( new DateOnly( 2018, 4, 1 ), "Beta" ) };
            yield return new object[] { new TestType( "Contoso.Api.v1.Controllers" ), new ApiVersion( 1 ) };
            yield return new object[] { new TestType( "Contoso.Api.v1_1.Controllers" ), new ApiVersion( 1, 1 ) };
            yield return new object[] { new TestType( "Contoso.Api.v0_9_Beta.Controllers" ), new ApiVersion( 0, 9, "Beta" ) };
            yield return new object[] { new TestType( "Contoso.Api.v20180401.Controllers" ), new ApiVersion( new DateOnly( 2018, 4, 1 ) ) };
            yield return new object[] { new TestType( "Contoso.Api.v2018_04_01.Controllers" ), new ApiVersion( new DateOnly( 2018, 4, 1 ) ) };
            yield return new object[] { new TestType( "Contoso.Api.v20180401_Beta.Controllers" ), new ApiVersion( new DateOnly( 2018, 4, 1 ), "Beta" ) };
            yield return new object[] { new TestType( "Contoso.Api.v2018_04_01_Beta.Controllers" ), new ApiVersion( new DateOnly( 2018, 4, 1 ), "Beta" ) };
            yield return new object[] { new TestType( "Contoso.Api.v2018_04_01_1_0_Beta.Controllers" ), new ApiVersion( new DateOnly( 2018, 4, 1 ), 1, 0, "Beta" ) };
            yield return new object[] { new TestType( "MyRestaurant.Vegetarian.Food.v1_1.Controllers" ), new ApiVersion( 1, 1 ) };
            yield return new object[] { new TestType( "VersioningSample.V5.Controllers" ), new ApiVersion( 5, 0 ) };
        }
    }

    public static IEnumerable<object[]> NamespaceWithMultipleVersions
    {
        get
        {
            yield return new object[] { new TestType( "Contoso.Api.v1.Controllers.v1" ), new ApiVersion[] { new( 1 ), new( 1 ) } };
            yield return new object[] { new TestType( "Contoso.Api.v1_1.Controllers.v1" ), new ApiVersion[] { new( 1, 1 ), new( 1 ) } };
            yield return new object[] { new TestType( "Contoso.Api.v2_0.Controllers.v2" ), new ApiVersion[] { new( 2, 0 ), new( 2 ) } };
            yield return new object[] { new TestType( "Contoso.Api.v20180401.Controllers.v1" ), new ApiVersion[] { new( new DateOnly( 2018, 4, 1 ) ), new( 1 ) } };
            yield return new object[] { new TestType( "Contoso.Api.v2018_04_01.Controllers.v2_0_Beta" ), new ApiVersion[] { new( new DateOnly( 2018, 4, 1 ) ), new( 2, 0, "Beta" ) } };
            yield return new object[] { new TestType( "v2018_04_01.Controllers.v2_0_RC" ), new ApiVersion[] { new( new DateOnly( 2018, 4, 1 ) ), new( 2, 0, "RC" ) } };
        }
    }

    private sealed class TestType : TypeDelegator
    {
        public TestType( string @namespace ) : base( typeof( object ) ) => Namespace = @namespace;

        public override string Namespace { get; }
    }
}