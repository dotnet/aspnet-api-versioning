// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

#if NETFRAMEWORK
using DateOnly = System.DateTime;
#endif

public class IApiVersioningPolicyBuilderExtensionsTest
{
    [Fact]
    public void sunset_should_add_global_policy_by_name()
    {
        // arrange
        var builder = Mock.Of<IApiVersioningPolicyBuilder>();

        // act
        builder.Sunset( "Test" );

        // assert
        Mock.Get( builder ).Verify( b => b.Sunset( "Test", default ) );
    }

    [Fact]
    public void sunset_should_add_global_policy_by_api_version()
    {
        // arrange
        var builder = Mock.Of<IApiVersioningPolicyBuilder>();

        // act
        builder.Sunset( ApiVersion.Default );

        // assert
        Mock.Get( builder ).Verify( b => b.Sunset( default, ApiVersion.Default ) );
    }

    [Fact]
    public void sunset_should_add_global_policy_by_version_parts()
    {
        // arrange
        var builder = Mock.Of<IApiVersioningPolicyBuilder>();
        var version = new ApiVersion( 1, 1, "RC" );

        // act
        builder.Sunset( 1, 1, "RC" );

        // assert
        Mock.Get( builder ).Verify( b => b.Sunset( default, version ) );
    }

    [Fact]
    public void sunset_should_add_global_policy_by_version()
    {
        // arrange
        var builder = Mock.Of<IApiVersioningPolicyBuilder>();
        var version = new ApiVersion( 1.1, "RC" );

        // act
        builder.Sunset( 1.1, "RC" );

        // assert
        Mock.Get( builder ).Verify( b => b.Sunset( default, version ) );
    }

    [Fact]
    public void sunset_should_add_global_policy_by_date()
    {
        // arrange
        var builder = Mock.Of<IApiVersioningPolicyBuilder>();
        var version = new ApiVersion( new DateOnly( 2022, 2, 1 ), "RC" );

        // act
        builder.Sunset( 2022, 2, 1, "RC" );

        // assert
        Mock.Get( builder ).Verify( b => b.Sunset( default, version ) );
    }

    [Fact]
    public void sunset_should_add_global_policy_by_date_parts()
    {
        // arrange
        var builder = Mock.Of<IApiVersioningPolicyBuilder>();
        var version = new ApiVersion( new DateOnly( 2022, 2, 1 ), "RC" );

        // act
        builder.Sunset( new DateOnly( 2022, 2, 1 ), "RC" );

        // assert
        Mock.Get( builder ).Verify( b => b.Sunset( default, version ) );
    }

    [Fact]
    public void sunset_should_add_policy_by_name_and_api_version()
    {
        // arrange
        var builder = Mock.Of<IApiVersioningPolicyBuilder>();

        // act
        builder.Sunset( "Test", ApiVersion.Default );

        // assert
        Mock.Get( builder ).Verify( b => b.Sunset( "Test", ApiVersion.Default ) );
    }

    [Fact]
    public void sunset_should_add_policy_by_name_and_version_parts()
    {
        // arrange
        var builder = Mock.Of<IApiVersioningPolicyBuilder>();
        var version = new ApiVersion( 1, 1, "RC" );

        // act
        builder.Sunset( "Test", 1, 1, "RC" );

        // assert
        Mock.Get( builder ).Verify( b => b.Sunset( "Test", version ) );
    }

    [Fact]
    public void sunset_should_add_policy_by_name_version()
    {
        // arrange
        var builder = Mock.Of<IApiVersioningPolicyBuilder>();
        var version = new ApiVersion( 1.1, "RC" );

        // act
        builder.Sunset( "Test", 1.1, "RC" );

        // assert
        Mock.Get( builder ).Verify( b => b.Sunset( "Test", version ) );
    }

    [Fact]
    public void sunset_should_add_policy_by_name_and_date()
    {
        // arrange
        var builder = Mock.Of<IApiVersioningPolicyBuilder>();
        var version = new ApiVersion( new DateOnly( 2022, 2, 1 ), "RC" );

        // act
        builder.Sunset( "Test", 2022, 2, 1, "RC" );

        // assert
        Mock.Get( builder ).Verify( b => b.Sunset( "Test", version ) );
    }

    [Fact]
    public void sunset_should_add_policy_by_name_and_date_parts()
    {
        // arrange
        var builder = Mock.Of<IApiVersioningPolicyBuilder>();
        var version = new ApiVersion( new DateOnly( 2022, 2, 1 ), "RC" );

        // act
        builder.Sunset( "Test", new DateOnly( 2022, 2, 1 ), "RC" );

        // assert
        Mock.Get( builder ).Verify( b => b.Sunset( "Test", version ) );
    }
}