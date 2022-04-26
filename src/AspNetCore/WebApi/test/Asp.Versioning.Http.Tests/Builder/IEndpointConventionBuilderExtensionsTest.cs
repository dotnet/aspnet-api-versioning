// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Builder;

using Microsoft.AspNetCore.Builder;

public class IEndpointConventionBuilderExtensionsTest
{
    [Fact]
    public void use_api_versioning_should_return_same_instance()
    {
        // arrange
        var endpoints = Mock.Of<IEndpointConventionBuilder>();
        var versionSet = new ApiVersionSetBuilder( default ).Build();
        var builder1 = endpoints.WithApiVersionSet( versionSet );

        // act
        var builder2 = builder1.WithApiVersionSet( versionSet );

        // assert
        builder1.Should().BeSameAs( builder2 );
    }

    [Fact]
    public void report_api_versions_should_set_builder_property_to_true()
    {
        // arrange
        var builder = Mock.Of<IVersionedEndpointConventionBuilder>();

        // act
        builder.ReportApiVersions();

        // assert
        builder.ReportApiVersions.Should().BeTrue();
    }
}