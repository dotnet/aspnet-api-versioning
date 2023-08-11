// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Microsoft.Extensions.DependencyInjection;

using Asp.Versioning;
using Microsoft.Extensions.Options;

public class IServiceCollectionExtensionsTest
{
    [Fact]
    public void add_api_versioning_should_not_allow_default_neutral_api_version()
    {
        // arrange
        var services = new ServiceCollection();

        services.AddApiVersioning( options => options.DefaultApiVersion = ApiVersion.Neutral );

        var provider = services.BuildServiceProvider();

        // act
        Func<ApiVersioningOptions> options = () => provider.GetRequiredService<IOptions<ApiVersioningOptions>>().Value;

        // assert
        options.Should().Throw<OptionsValidationException>();
    }
}