// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

#if !NETFRAMEWORK
using Microsoft.Extensions.Options;
#endif

public class SunsetPolicyManagerTest
{
    [Fact]
    public void try_get_policy_should_return_false_for_no_name_and_version()
    {
        // arrange
        var options = new ApiVersioningOptions();
        var manager = NewSunsetPolicyManager( options );

        // act
        var result = manager.TryGetPolicy( default, default, out _ );

        // assert
        result.Should().BeFalse();
    }

    [Fact]
    public void try_get_policy_should_return_false_without_any_policies()
    {
        // arrange
        var options = new ApiVersioningOptions();
        var manager = NewSunsetPolicyManager( options );

        // act
        var result = manager.TryGetPolicy( ApiVersion.Default, out _ );

        // assert
        result.Should().BeFalse();
    }

    [Fact]
    public void try_get_policy_should_return_true_for_matching_policy()
    {
        // arrange
        var options = new ApiVersioningOptions();
        var manager = NewSunsetPolicyManager( options );

        options.Policies.Sunset( ApiVersion.Default ).Effective( 2022, 2, 1 );

        // act
        var result = manager.TryGetPolicy( ApiVersion.Default, out var policy );

        // assert
        result.Should().BeTrue();
        policy.Should().NotBeNull();
    }

    private static SunsetPolicyManager NewSunsetPolicyManager( ApiVersioningOptions options )
    {
#if NETFRAMEWORK
        return new( options );
#else
        return new( Options.Create( options ) );
#endif
    }
}