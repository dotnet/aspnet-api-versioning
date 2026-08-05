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
}