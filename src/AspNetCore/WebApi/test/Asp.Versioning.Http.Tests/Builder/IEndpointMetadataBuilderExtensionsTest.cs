// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Builder;

public class IEndpointMetadataBuilderExtensionsTest
{
    [Fact]
    public void report_api_versions_should_set_property_to_true()
    {
        // arrange
        var builder = Mock.Of<IEndpointMetadataBuilder>();

        // act
        builder.ReportApiVersions();

        // assert
        builder.ReportApiVersions.Should().BeTrue();
    }
}