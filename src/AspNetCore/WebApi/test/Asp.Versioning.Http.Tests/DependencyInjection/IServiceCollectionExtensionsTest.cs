// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0130

namespace Microsoft.Extensions.DependencyInjection;

using Asp.Versioning;
using Microsoft.AspNetCore.Http;
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

    [Fact]
    public void add_api_versioning_should_not_displace_or_wrap_user_registered_problem_details_writers()
    {
        // arrange
        var services = new ServiceCollection();
        var writer = new Mock<IProblemDetailsWriter>();

        writer.Setup( w => w.CanWrite( It.IsAny<ProblemDetailsContext>() ) ).Returns( true );
        services.AddProblemDetails();
        services.AddSingleton( writer.Object );

        var before = services.Where( s => s.ServiceType == typeof( IProblemDetailsWriter ) ).ToArray();

        // act
        services.AddApiVersioning();

        // assert
        var after = services.Where( s => s.ServiceType == typeof( IProblemDetailsWriter ) ).ToArray();
        using var provider = services.BuildServiceProvider();

        after.Should().Equal( before );
        provider.GetServices<IProblemDetailsWriter>().Should().Contain( writer.Object );
    }
}