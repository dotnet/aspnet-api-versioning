// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

public class ISunsetPolicyManagerExtensionsTest
{
    [Fact]
    public void try_get_policy_should_get_global_policy_by_version()
    {
        // arrange
        var manager = new Mock<ISunsetPolicyManager>();
        var version = ApiVersion.Default;
        var expected = new SunsetPolicy();

        manager.Setup( m => m.TryGetPolicy( default, It.IsAny<ApiVersion>(), out expected ) )
               .Returns( true );

        // act
        manager.Object.TryGetPolicy( version, out var policy );

        // assert
        policy.Should().NotBeNull();
    }

    [Fact]
    public void try_get_policy_should_get_global_policy_by_name()
    {
        // arrange
        var manager = new Mock<ISunsetPolicyManager>();
        var expected = new SunsetPolicy();

        manager.Setup( m => m.TryGetPolicy( "Test", default, out expected ) )
               .Returns( true );

        // act
        manager.Object.TryGetPolicy( "Test", out var policy );

        // assert
        policy.Should().NotBeNull();
    }

    [Fact]
    public void resolve_policy_should_return_most_specific_result()
    {
        // arrange
        var manager = new Mock<ISunsetPolicyManager>();
        var expected = new SunsetPolicy();
        var other = new SunsetPolicy();

        manager.Setup( m => m.TryGetPolicy( "Test", new ApiVersion( 1.0, null ), out expected ) ).Returns( true );
        manager.Setup( m => m.TryGetPolicy( default, new ApiVersion( 1.0, null ), out other ) ).Returns( true );

        // act
        var policy = manager.Object.ResolvePolicyOrDefault( "Test", new ApiVersion( 1.0 ) );

        // assert
        policy.Should().BeSameAs( expected );
    }

    [Fact]
    public void resolve_policy_should_fall_back_to_global_result()
    {
        // arrange
        var manager = new Mock<ISunsetPolicyManager>();
        var expected = new SunsetPolicy();
        var other = new SunsetPolicy();

        manager.Setup( m => m.TryGetPolicy( "Test", new ApiVersion( 1.0, null ), out other ) ).Returns( true );
        manager.Setup( m => m.TryGetPolicy( default, new ApiVersion( 1.0, null ), out expected ) ).Returns( true );

        // act
        var policy = manager.Object.ResolvePolicyOrDefault( default, new ApiVersion( 1.0 ) );

        // assert
        policy.Should().BeSameAs( expected );
    }
}