// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using System;
using System.Reflection;
using System.Xml.Linq;
#if NETFRAMEWORK
using DateOnly = System.DateTime;
#endif

public class NamespaceParserTest
{
    [Theory]
    [MemberData( nameof( NamespaceWithOneVersion ) )]
    public void parse_should_return_single_version( string @namespace, string version )
    {
        // arrange
        var type = new TestType( @namespace );
        var expected = ApiVersionParser.Default.Parse( version );

        // act
        var result = NamespaceParser.Default.Parse( type );

        // assert
        result.Should().Equal( expected );
    }

    [Theory]
    [MemberData( nameof( NamespaceWithMultipleVersions ) )]
    public void parse_should_return_multiple_versions( string @namespace, string[] versions )
    {
        // arrange
        var type = new TestType( @namespace );
        var expected = versions.Select( static v => ApiVersionParser.Default.Parse( v ) ).ToArray();

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

    public static TheoryData<string, string> NamespaceWithOneVersion => new()
    {
        { "v1", "1" },
        { "v1RC", "1.0-RC" },
        { "v20180401", "2018-04-01" },
        { "v20180401_Beta", "2018-04-01-Beta" },
        { "v20180401Beta", "2018-04-01-Beta" },
        { "Contoso.Api.v1.Controllers", "1" },
        { "Contoso.Api.v1_1.Controllers", "1.1" },
        { "Contoso.Api.v0_9_Beta.Controllers", "0.9-Beta" },
        { "Contoso.Api.v20180401.Controllers", "2018-04-01" },
        { "Contoso.Api.v2018_04_01.Controllers", "2018-04-01" },
        { "Contoso.Api.v20180401_Beta.Controllers", "2018-04-01-Beta" },
        { "Contoso.Api.v2018_04_01_Beta.Controllers", "2018-04-01-Beta" },
        { "Contoso.Api.v2018_04_01_1_0_Beta.Controllers", "2018-04-01.1.0-Beta" },
        { "MyRestaurant.Vegetarian.Food.v1_1.Controllers", "1.1" },
        { "VersioningSample.V5.Controllers", "5.0" },
    };

    public static TheoryData<string, string[]> NamespaceWithMultipleVersions => new()
    {
        { "Contoso.Api.v1.Controllers.v1", ["1", "1"] },
        { "Contoso.Api.v1_1.Controllers.v1", ["1.1", "1"] },
        { "Contoso.Api.v2_0.Controllers.v2", ["2.0", "2"] },
        { "Contoso.Api.v20180401.Controllers.v1", ["2018-04-01", "1"] },
        { "Contoso.Api.v2018_04_01.Controllers.v2_0_Beta", ["2018-04-01", "2.0-Beta"] },
        { "v2018_04_01.Controllers.v2_0_RC", ["2018-04-01", "2.0-RC"] },
    };

#pragma warning disable IDE0079
#pragma warning disable CA1034

    public sealed class TestType : TypeDelegator
    {
        public TestType( string @namespace ) : base( typeof( object ) ) => Namespace = @namespace;

        public override string Namespace { get; }
    }
}