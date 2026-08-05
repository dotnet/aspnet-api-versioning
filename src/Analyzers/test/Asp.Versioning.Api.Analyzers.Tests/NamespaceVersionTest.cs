// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Analyzers;


public class NamespaceVersionTest
{
    [Theory]
    [InlineData( "v1" )]
    [InlineData( "V1" )]
    [InlineData( "v1_1" )]
    [InlineData( "v2_0_Beta" )]
    [InlineData( "v20180401" )]
    [InlineData( "v2018_04_01_1_1_Beta" )]
    [InlineData( "_1" )]
    [InlineData( "_1_1" )]
    [InlineData( "_20180401" )]
    [InlineData( "_2018_04_01" )]
    [InlineData( "_2018_04_01_Beta" )]
    [InlineData( "_2018_04_01_1_0_Beta" )]
    [InlineData( "Api.v1.Controllers" )]
    [InlineData( "Company.Api._2018_04_01" )]
    public void is_versioned_should_return_true_for_a_versioned_namespace( string @namespace ) =>
        NamespaceVersion.IsVersioned( @namespace ).Should().BeTrue();

    [Theory]
    [InlineData( "" )]
    [InlineData( "Api" )]
    [InlineData( "Api.Controllers" )]
    [InlineData( "Version1" )]
    [InlineData( "vNext" )]
    [InlineData( "v" )]
    [InlineData( "v1_1_Bad-Status" )]
    [InlineData( "v20181301" )]
    [InlineData( "v2018_13_01" )]
    [InlineData( "Api.Models.Orders" )]
    public void is_versioned_should_return_false_for_an_unversioned_namespace( string @namespace ) =>
        NamespaceVersion.IsVersioned( @namespace ).Should().BeFalse();

    [Theory]
    [MemberData( nameof( NamespaceTypes ) )]
    public void is_versioned_should_agree_with_namespace_parser( string typeName )
    {
        // arrange
        var type = Types[typeName];
        var expected = NamespaceParser.Default.Parse( type ).Count > 0;

        // act
        var versioned = NamespaceVersion.IsVersioned( type.Namespace );

        // assert
        versioned.Should().Be( expected, "the port must agree with NamespaceParser" );
    }

    public static TheoryData<string> NamespaceTypes => [.. Types.Keys];

    private static readonly IReadOnlyDictionary<string, Type> Types = new Dictionary<string, Type>
    {
        ["V1"] = typeof( Versioned.V1.VersionedV1Marker ),
        ["v1_1"] = typeof( Versioned.v1_1.Versionedv1_1Marker ),
        ["V2_0_Beta"] = typeof( Versioned.V2_0_Beta.VersionedV2_0_BetaMarker ),
        ["_20180401"] = typeof( Versioned._20180401.Versioned20180401Marker ),
        ["_2018_04_01"] = typeof( Versioned._2018_04_01.Versioned2018_04_01Marker ),
        ["_2018_04_01_1_0_Beta"] = typeof( Versioned._2018_04_01_1_0_Beta.Versioned2018_04_01_1_0_BetaMarker ),
        ["v2018_04_01_1_1_Beta"] = typeof( Versioned.v2018_04_01_1_1_Beta.Versionedv2018_04_01_1_1_BetaMarker ),
        ["Controllers"] = typeof( Unversioned.Controllers.UnversionedControllersMarker ),
        ["Version1"] = typeof( Unversioned.Version1.UnversionedVersion1Marker ),
        ["vNext"] = typeof( Unversioned.vNext.UnversionedvNextMarker ),
        ["v20181301"] = typeof( Unversioned.v20181301.Unversionedv20181301Marker ),
    };
}