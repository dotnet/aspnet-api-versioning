// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Builder;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;

public class IEndpointConventionBuilderExtensionsTest
{
    [Fact]
    public void use_api_versioning_should_return_same_instance()
    {
        // arrange
        var endpoints = Mock.Of<IEndpointConventionBuilder>();
        var source = Mock.Of<IApiVersionParameterSource>();
        var options = Options.Create( new ApiVersioningOptions() );
        var versionSet = new ApiVersionSetBuilder( null, source, options ).Build();
        var builder1 = endpoints.UseApiVersioning( versionSet );

        // act
        var builder2 = builder1.UseApiVersioning( versionSet );

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