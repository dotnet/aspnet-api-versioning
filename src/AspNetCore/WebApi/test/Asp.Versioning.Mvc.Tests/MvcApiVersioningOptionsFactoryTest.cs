// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using Asp.Versioning.Conventions;
using Microsoft.Extensions.Options;

public class MvcApiVersioningOptionsFactoryTest
{
    [Fact]
    public void create_should_construct_expected_options()
    {
        // arrange
        var conventionBuilder = new ApiVersionConventionBuilder();
        var configure = Mock.Of<IConfigureOptions<MvcApiVersioningOptions>>();
        var postConfigure = Mock.Of<IPostConfigureOptions<MvcApiVersioningOptions>>();
        var factory = new MvcApiVersioningOptionsFactory<MvcApiVersioningOptions>(
            conventionBuilder,
            new[] { configure },
            new[] { postConfigure } );

        // act
        var options = factory.Create( Options.DefaultName );

        // assert
        options.Conventions.Should().BeSameAs( conventionBuilder );
        Mock.Get( configure ).Verify( c => c.Configure( options ) );
        Mock.Get( postConfigure ).Verify( c => c.PostConfigure( Options.DefaultName, options ) );
    }
}