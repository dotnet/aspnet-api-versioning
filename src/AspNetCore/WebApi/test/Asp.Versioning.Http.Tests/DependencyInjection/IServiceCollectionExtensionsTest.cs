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

    // REF: https://github.com/dotnet/aspnet-api-versioning/issues/1191
    [Fact]
    public void add_api_versioning_should_not_displace_or_wrap_user_registered_problem_details_writers()
    {
        // arrange
        var services = new ServiceCollection();
        var customWriter = new TestProblemDetailsWriter();

        services.AddProblemDetails();
        services.AddSingleton<IProblemDetailsWriter>( customWriter );

        var writersBefore = services.Where( s => s.ServiceType == typeof( IProblemDetailsWriter ) ).ToArray();

        // act
        services.AddApiVersioning();

        // assert
        var writersAfter = services.Where( s => s.ServiceType == typeof( IProblemDetailsWriter ) ).ToArray();
        writersAfter.Should().Equal( writersBefore );

        using var provider = services.BuildServiceProvider();
        provider.GetServices<IProblemDetailsWriter>().Should().Contain( customWriter );
    }

    private sealed class TestProblemDetailsWriter : IProblemDetailsWriter
    {
        public bool CanWrite( ProblemDetailsContext context ) => true;

        public ValueTask WriteAsync( ProblemDetailsContext context ) => ValueTask.CompletedTask;
    }
}