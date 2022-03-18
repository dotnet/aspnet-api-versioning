// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Conventions;

#if NETFRAMEWORK
using ControllerBase = System.Web.Http.Controllers.IHttpController;
using DateOnly = System.DateTime;
#else
using Microsoft.AspNetCore.Mvc;
#endif

public class ControllerApiVersionConventionBuilderExtensionsTTest
{
    [Fact]
    public void has_api_version_should_add_major_version()
    {
        // arrange
        var controllerBuilder = new TestControllerApiVersionConventionBuilder();

        // act
        controllerBuilder.HasApiVersion( 1 );

        // assert
        controllerBuilder.ProtectedSupportedVersions.Single().Should().Be( new ApiVersion( 1, 0 ) );
    }

    [Fact]
    public void has_api_version_should_add_major_version_with_status()
    {
        // arrange
        var controllerBuilder = new TestControllerApiVersionConventionBuilder();

        // act
        controllerBuilder.HasApiVersion( 1, "beta" );

        // assert
        controllerBuilder.ProtectedSupportedVersions.Single().Should().Be( new ApiVersion( 1, 0, "beta" ) );
    }

    [Fact]
    public void has_api_version_should_add_major_and_minor_version()
    {
        // arrange
        var controllerBuilder = new TestControllerApiVersionConventionBuilder();

        // act
        controllerBuilder.HasApiVersion( 1, 5 );

        // assert
        controllerBuilder.ProtectedSupportedVersions.Single().Should().Be( new ApiVersion( 1, 5 ) );
    }

    [Fact]
    public void has_api_version_should_add_major_and_minor_version_with_status()
    {
        // arrange
        var controllerBuilder = new TestControllerApiVersionConventionBuilder();

        // act
        controllerBuilder.HasApiVersion( 1, 5, "rc" );

        // assert
        controllerBuilder.ProtectedSupportedVersions.Single().Should().Be( new ApiVersion( 1, 5, "rc" ) );
    }

    [Fact]
    public void has_api_version_should_add_group_version_parts()
    {
        // arrange
        var controllerBuilder = new TestControllerApiVersionConventionBuilder();

        // act
        controllerBuilder.HasApiVersion( 2016, 9, 10 );

        // assert
        controllerBuilder.ProtectedSupportedVersions.Single().Should().Be( new ApiVersion( new DateOnly( 2016, 9, 10 ) ) );
    }

    [Fact]
    public void has_api_version_should_add_group_version_parts_with_status()
    {
        // arrange
        var controllerBuilder = new TestControllerApiVersionConventionBuilder();

        // act
        controllerBuilder.HasApiVersion( 2016, 9, 10, "alpha" );

        // assert
        controllerBuilder.ProtectedSupportedVersions.Single().Should().Be( new ApiVersion( new DateOnly( 2016, 9, 10 ), "alpha" ) );
    }

    [Fact]
    public void has_api_version_should_add_group_version()
    {
        // arrange
        var controllerBuilder = new TestControllerApiVersionConventionBuilder();
        var groupVersion = new DateOnly( 2016, 9, 10 );

        // act
        controllerBuilder.HasApiVersion( groupVersion );

        // assert
        controllerBuilder.ProtectedSupportedVersions.Single().Should().Be( new ApiVersion( groupVersion ) );
    }

    [Fact]
    public void has_api_version_should_add_group_version_with_status()
    {
        // arrange
        var controllerBuilder = new TestControllerApiVersionConventionBuilder();
        var groupVersion = new DateOnly( 2016, 9, 10 );

        // act
        controllerBuilder.HasApiVersion( groupVersion, "alpha" );

        // assert
        controllerBuilder.ProtectedSupportedVersions.Single().Should().Be( new ApiVersion( groupVersion, "alpha" ) );
    }

    [Fact]
    public void has_api_versions_should_add_multiple_api_versions()
    {
        // arrange
        var apiVersions = new[] { new ApiVersion( 1, 0 ), new ApiVersion( 2, 0 ), new ApiVersion( 3, 0 ) };
        var controllerBuilder = new TestControllerApiVersionConventionBuilder();

        // act
        controllerBuilder.HasApiVersions( apiVersions );

        // assert
        controllerBuilder.ProtectedSupportedVersions.Should().BeEquivalentTo( new ApiVersion[] { new( 1, 0 ), new( 2, 0 ), new( 3, 0 ) } );
    }

    [Fact]
    public void has_deprecated_api_version_should_add_major_version()
    {
        // arrange
        var controllerBuilder = new TestControllerApiVersionConventionBuilder();

        // act
        controllerBuilder.HasDeprecatedApiVersion( 1 );

        // assert
        controllerBuilder.ProtectedDeprecatedVersions.Single().Should().Be( new ApiVersion( 1, 0 ) );
    }

    [Fact]
    public void has_deprecated_api_version_should_add_major_version_with_status()
    {
        // arrange
        var controllerBuilder = new TestControllerApiVersionConventionBuilder();

        // act
        controllerBuilder.HasDeprecatedApiVersion( 1, "beta" );

        // assert
        controllerBuilder.ProtectedDeprecatedVersions.Single().Should().Be( new ApiVersion( 1, 0, "beta" ) );
    }

    [Fact]
    public void has_deprecated_api_version_should_add_major_and_minor_version()
    {
        // arrange
        var controllerBuilder = new TestControllerApiVersionConventionBuilder();

        // act
        controllerBuilder.HasDeprecatedApiVersion( 1, 5 );

        // assert
        controllerBuilder.ProtectedDeprecatedVersions.Single().Should().Be( new ApiVersion( 1, 5 ) );
    }

    [Fact]
    public void has_deprecated_api_version_should_add_major_and_minor_version_with_status()
    {
        // arrange
        var controllerBuilder = new TestControllerApiVersionConventionBuilder();

        // act
        controllerBuilder.HasDeprecatedApiVersion( 1, 5, "rc" );

        // assert
        controllerBuilder.ProtectedDeprecatedVersions.Single().Should().Be( new ApiVersion( 1, 5, "rc" ) );
    }

    [Fact]
    public void has_deprecated_api_version_should_add_group_version_parts()
    {
        // arrange
        var controllerBuilder = new TestControllerApiVersionConventionBuilder();

        // act
        controllerBuilder.HasDeprecatedApiVersion( 2016, 9, 10 );

        // assert
        controllerBuilder.ProtectedDeprecatedVersions.Single().Should().Be( new ApiVersion( new DateOnly( 2016, 9, 10 ) ) );
    }

    [Fact]
    public void has_deprecated_api_version_should_add_group_version_parts_with_status()
    {
        // arrange
        var controllerBuilder = new TestControllerApiVersionConventionBuilder();

        // act
        controllerBuilder.HasDeprecatedApiVersion( 2016, 9, 10, "alpha" );

        // assert
        controllerBuilder.ProtectedDeprecatedVersions.Single().Should().Be( new ApiVersion( new DateOnly( 2016, 9, 10 ), "alpha" ) );
    }

    [Fact]
    public void has_deprecated_api_version_should_add_group_version()
    {
        // arrange
        var controllerBuilder = new TestControllerApiVersionConventionBuilder();
        var groupVersion = new DateOnly( 2016, 9, 10 );

        // act
        controllerBuilder.HasDeprecatedApiVersion( groupVersion );

        // assert
        controllerBuilder.ProtectedDeprecatedVersions.Single().Should().Be( new ApiVersion( groupVersion ) );
    }

    [Fact]
    public void has_deprecated_api_version_should_add_group_version_with_status()
    {
        // arrange
        var controllerBuilder = new TestControllerApiVersionConventionBuilder();
        var groupVersion = new DateOnly( 2016, 9, 10 );

        // act
        controllerBuilder.HasDeprecatedApiVersion( groupVersion, "alpha" );

        // assert
        controllerBuilder.ProtectedDeprecatedVersions.Single().Should().Be( new ApiVersion( groupVersion, "alpha" ) );
    }

    [Fact]
    public void has_deprecated_api_versions_should_add_multiple_api_versions()
    {
        // arrange
        var apiVersions = new[] { new ApiVersion( 1, 0 ), new ApiVersion( 2, 0 ), new ApiVersion( 3, 0 ) };
        var controllerBuilder = new TestControllerApiVersionConventionBuilder();

        // act
        controllerBuilder.HasDeprecatedApiVersions( apiVersions );

        // assert
        controllerBuilder.ProtectedDeprecatedVersions.Should().BeEquivalentTo( new ApiVersion[] { new( 1, 0 ), new( 2, 0 ), new( 3, 0 ) } );
    }

    [Fact]
    public void advertises_api_version_should_add_major_version()
    {
        // arrange
        var controllerBuilder = new TestControllerApiVersionConventionBuilder();

        // act
        controllerBuilder.AdvertisesApiVersion( 1 );

        // assert
        controllerBuilder.ProtectedAdvertisedVersions.Single().Should().Be( new ApiVersion( 1, 0 ) );
    }

    [Fact]
    public void advertises_api_version_should_add_major_version_with_status()
    {
        // arrange
        var controllerBuilder = new TestControllerApiVersionConventionBuilder();

        // act
        controllerBuilder.AdvertisesApiVersion( 1, "beta" );

        // assert
        controllerBuilder.ProtectedAdvertisedVersions.Single().Should().Be( new ApiVersion( 1, 0, "beta" ) );
    }

    [Fact]
    public void advertises_api_version_should_add_major_and_minor_version()
    {
        // arrange
        var controllerBuilder = new TestControllerApiVersionConventionBuilder();

        // act
        controllerBuilder.AdvertisesApiVersion( 1, 5 );

        // assert
        controllerBuilder.ProtectedAdvertisedVersions.Single().Should().Be( new ApiVersion( 1, 5 ) );
    }

    [Fact]
    public void advertises_api_version_should_add_major_and_minor_version_with_status()
    {
        // arrange
        var controllerBuilder = new TestControllerApiVersionConventionBuilder();

        // act
        controllerBuilder.AdvertisesApiVersion( 1, 5, "rc" );

        // assert
        controllerBuilder.ProtectedAdvertisedVersions.Single().Should().Be( new ApiVersion( 1, 5, "rc" ) );
    }

    [Fact]
    public void advertises_api_version_should_add_group_version_parts()
    {
        // arrange
        var controllerBuilder = new TestControllerApiVersionConventionBuilder();

        // act
        controllerBuilder.AdvertisesApiVersion( 2016, 9, 10 );

        // assert
        controllerBuilder.ProtectedAdvertisedVersions.Single().Should().Be( new ApiVersion( new DateOnly( 2016, 9, 10 ) ) );
    }

    [Fact]
    public void advertises_api_version_should_add_group_version_parts_with_status()
    {
        // arrange
        var controllerBuilder = new TestControllerApiVersionConventionBuilder();

        // act
        controllerBuilder.AdvertisesApiVersion( 2016, 9, 10, "alpha" );

        // assert
        controllerBuilder.ProtectedAdvertisedVersions.Single().Should().Be( new ApiVersion( new DateOnly( 2016, 9, 10 ), "alpha" ) );
    }

    [Fact]
    public void advertises_api_version_should_add_group_version()
    {
        // arrange
        var controllerBuilder = new TestControllerApiVersionConventionBuilder();
        var groupVersion = new DateOnly( 2016, 9, 10 );

        // act
        controllerBuilder.AdvertisesApiVersion( groupVersion );

        // assert
        controllerBuilder.ProtectedAdvertisedVersions.Single().Should().Be( new ApiVersion( groupVersion ) );
    }

    [Fact]
    public void advertises_api_version_should_add_group_version_with_status()
    {
        // arrange
        var controllerBuilder = new TestControllerApiVersionConventionBuilder();
        var groupVersion = new DateOnly( 2016, 9, 10 );

        // act
        controllerBuilder.AdvertisesApiVersion( groupVersion, "alpha" );

        // assert
        controllerBuilder.ProtectedAdvertisedVersions.Single().Should().Be( new ApiVersion( groupVersion, "alpha" ) );
    }

    [Fact]
    public void advertises_api_versions_should_add_multiple_api_versions()
    {
        // arrange
        var apiVersions = new[] { new ApiVersion( 1, 0 ), new ApiVersion( 2, 0 ), new ApiVersion( 3, 0 ) };
        var controllerBuilder = new TestControllerApiVersionConventionBuilder();

        // act
        controllerBuilder.AdvertisesApiVersions( apiVersions );

        // assert
        controllerBuilder.ProtectedAdvertisedVersions.Should().BeEquivalentTo( new ApiVersion[] { new( 1, 0 ), new( 2, 0 ), new( 3, 0 ) } );
    }

    [Fact]
    public void advertises_deprecated_api_version_should_add_major_version()
    {
        // arrange
        var controllerBuilder = new TestControllerApiVersionConventionBuilder();

        // act
        controllerBuilder.AdvertisesDeprecatedApiVersion( 1 );

        // assert
        controllerBuilder.ProtectedDeprecatedAdvertisedVersions.Single().Should().Be( new ApiVersion( 1, 0 ) );
    }

    [Fact]
    public void advertises_deprecated_api_version_should_add_major_version_with_status()
    {
        // arrange
        var controllerBuilder = new TestControllerApiVersionConventionBuilder();

        // act
        controllerBuilder.AdvertisesDeprecatedApiVersion( 1, "beta" );

        // assert
        controllerBuilder.ProtectedDeprecatedAdvertisedVersions.Single().Should().Be( new ApiVersion( 1, 0, "beta" ) );
    }

    [Fact]
    public void advertises_deprecated_api_version_should_add_major_and_minor_version()
    {
        // arrange
        var controllerBuilder = new TestControllerApiVersionConventionBuilder();

        // act
        controllerBuilder.AdvertisesDeprecatedApiVersion( 1, 5 );

        // assert
        controllerBuilder.ProtectedDeprecatedAdvertisedVersions.Single().Should().Be( new ApiVersion( 1, 5 ) );
    }

    [Fact]
    public void advertises_deprecated_api_version_should_add_major_and_minor_version_with_status()
    {
        // arrange
        var controllerBuilder = new TestControllerApiVersionConventionBuilder();

        // act
        controllerBuilder.AdvertisesDeprecatedApiVersion( 1, 5, "rc" );

        // assert
        controllerBuilder.ProtectedDeprecatedAdvertisedVersions.Single().Should().Be( new ApiVersion( 1, 5, "rc" ) );
    }

    [Fact]
    public void advertises_deprecated_api_version_should_add_group_version_parts()
    {
        // arrange
        var controllerBuilder = new TestControllerApiVersionConventionBuilder();

        // act
        controllerBuilder.AdvertisesDeprecatedApiVersion( 2016, 9, 10 );

        // assert
        controllerBuilder.ProtectedDeprecatedAdvertisedVersions.Single().Should().Be( new ApiVersion( new DateOnly( 2016, 9, 10 ) ) );
    }

    [Fact]
    public void advertises_deprecated_api_version_should_add_group_version_parts_with_status()
    {
        // arrange
        var controllerBuilder = new TestControllerApiVersionConventionBuilder();

        // act
        controllerBuilder.AdvertisesDeprecatedApiVersion( 2016, 9, 10, "alpha" );

        // assert
        controllerBuilder.ProtectedDeprecatedAdvertisedVersions.Single().Should().Be( new ApiVersion( new DateOnly( 2016, 9, 10 ), "alpha" ) );
    }

    [Fact]
    public void advertises_deprecated_api_version_should_add_group_version()
    {
        // arrange
        var controllerBuilder = new TestControllerApiVersionConventionBuilder();
        var groupVersion = new DateOnly( 2016, 9, 10 );

        // act
        controllerBuilder.AdvertisesDeprecatedApiVersion( groupVersion );

        // assert
        controllerBuilder.ProtectedDeprecatedAdvertisedVersions.Single().Should().Be( new ApiVersion( groupVersion ) );
    }

    [Fact]
    public void advertises_deprecated_api_version_should_add_group_version_with_status()
    {
        // arrange
        var controllerBuilder = new TestControllerApiVersionConventionBuilder();
        var groupVersion = new DateOnly( 2016, 9, 10 );

        // act
        controllerBuilder.AdvertisesDeprecatedApiVersion( groupVersion, "alpha" );

        // assert
        controllerBuilder.ProtectedDeprecatedAdvertisedVersions.Single().Should().Be( new ApiVersion( groupVersion, "alpha" ) );
    }

    [Fact]
    public void advertises_deprecated_api_versions_should_add_multiple_api_versions()
    {
        // arrange
        var apiVersions = new[] { new ApiVersion( 1, 0 ), new ApiVersion( 2, 0 ), new ApiVersion( 3, 0 ) };
        var controllerBuilder = new TestControllerApiVersionConventionBuilder();

        // act
        controllerBuilder.AdvertisesDeprecatedApiVersions( apiVersions );

        // assert
        controllerBuilder.ProtectedDeprecatedAdvertisedVersions.Should().BeEquivalentTo( new ApiVersion[] { new( 1, 0 ), new( 2, 0 ), new( 3, 0 ) } );
    }

    private sealed class TestControllerApiVersionConventionBuilder : ControllerApiVersionConventionBuilder<ControllerBase>
    {
        internal ICollection<ApiVersion> ProtectedSupportedVersions => SupportedVersions;

        internal ICollection<ApiVersion> ProtectedDeprecatedVersions => DeprecatedVersions;

        internal ICollection<ApiVersion> ProtectedAdvertisedVersions => AdvertisedVersions;

        internal ICollection<ApiVersion> ProtectedDeprecatedAdvertisedVersions => DeprecatedAdvertisedVersions;
    }
}