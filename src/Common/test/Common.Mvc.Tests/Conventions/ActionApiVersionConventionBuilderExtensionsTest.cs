// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Conventions;

#if NETFRAMEWORK
using ControllerBase = System.Web.Http.Controllers.IHttpController;
using DateOnly = System.DateTime;
#else
using Microsoft.AspNetCore.Mvc;
#endif

public class ActionApiVersionConventionBuilderExtensionsTest
{
    [Fact]
    public void map_to_api_version_should_add_major_version()
    {
        // arrange
        var actionBuilder = new TestActionApiVersionConventionBuilder();

        // act
        actionBuilder.MapToApiVersion( 1 );

        // assert
        actionBuilder.ProtectedMappedVersions.Single().Should().Be( new ApiVersion( 1, 0 ) );
    }

    [Fact]
    public void map_to_api_version_should_add_major_version_with_status()
    {
        // arrange
        var actionBuilder = new TestActionApiVersionConventionBuilder();

        // act
        actionBuilder.MapToApiVersion( 1, "beta" );

        // assert
        actionBuilder.ProtectedMappedVersions.Single().Should().Be( new ApiVersion( 1, 0, "beta" ) );
    }

    [Fact]
    public void map_to_api_version_should_add_major_and_minor_version()
    {
        // arrange
        var actionBuilder = new TestActionApiVersionConventionBuilder();

        // act
        actionBuilder.MapToApiVersion( 1, 5 );

        // assert
        actionBuilder.ProtectedMappedVersions.Single().Should().Be( new ApiVersion( 1, 5 ) );
    }

    [Fact]
    public void map_to_api_version_should_add_major_and_minor_version_with_status()
    {
        // arrange
        var actionBuilder = new TestActionApiVersionConventionBuilder();

        // act
        actionBuilder.MapToApiVersion( 1, 5, "rc" );

        // assert
        actionBuilder.ProtectedMappedVersions.Single().Should().Be( new ApiVersion( 1, 5, "rc" ) );
    }

    [Fact]
    public void map_to_api_version_should_add_group_version_parts()
    {
        // arrange
        var actionBuilder = new TestActionApiVersionConventionBuilder();

        // act
        actionBuilder.MapToApiVersion( 2016, 9, 10 );

        // assert
        actionBuilder.ProtectedMappedVersions.Single().Should().Be( new ApiVersion( new DateOnly( 2016, 9, 10 ) ) );
    }

    [Fact]
    public void map_to_api_version_should_add_group_version_parts_with_status()
    {
        // arrange
        var actionBuilder = new TestActionApiVersionConventionBuilder();

        // act
        actionBuilder.MapToApiVersion( 2016, 9, 10, "alpha" );

        // assert
        actionBuilder.ProtectedMappedVersions.Single().Should().Be( new ApiVersion( new DateOnly( 2016, 9, 10 ), "alpha" ) );
    }

    [Fact]
    public void map_to_api_version_should_add_group_version()
    {
        // arrange
        var actionBuilder = new TestActionApiVersionConventionBuilder();
        var groupVersion = new DateOnly( 2016, 9, 10 );

        // act
        actionBuilder.MapToApiVersion( groupVersion );

        // assert
        actionBuilder.ProtectedMappedVersions.Single().Should().Be( new ApiVersion( groupVersion ) );
    }

    [Fact]
    public void map_to_api_version_should_add_group_version_with_status()
    {
        // arrange
        var actionBuilder = new TestActionApiVersionConventionBuilder();
        var groupVersion = new DateOnly( 2016, 9, 10 );

        // act
        actionBuilder.MapToApiVersion( groupVersion, "alpha" );

        // assert
        actionBuilder.ProtectedMappedVersions.Single().Should().Be( new ApiVersion( groupVersion, "alpha" ) );
    }

    [Fact]
    public void map_to_api_versions_should_add_multiple_api_versions()
    {
        // arrange
        var apiVersions = new ApiVersion[] { new( 1, 0 ), new( 2, 0 ), new( 3, 0 ) };
        var actionBuilder = new TestActionApiVersionConventionBuilder();

        // act
        actionBuilder.MapToApiVersions( apiVersions );

        // assert
        actionBuilder.ProtectedMappedVersions.Should().BeEquivalentTo( new ApiVersion[] { new( 1, 0 ), new( 2, 0 ), new( 3, 0 ) } );
    }

    private sealed class TestActionApiVersionConventionBuilder : ActionApiVersionConventionBuilder
    {
        internal TestActionApiVersionConventionBuilder() : base( new( typeof( ControllerBase ) ) ) { }

        internal ICollection<ApiVersion> ProtectedMappedVersions => MappedVersions;
    }
}