// Copyright (c) .NET Foundation and contributors. All rights reserved.

// Ignore Spelling: Interop
namespace Asp.Versioning.Http;

using Microsoft.Extensions.DependencyInjection;

public class InteropFixture : MinimalApiFixture
{
    protected override void OnConfigureServices( IServiceCollection services )
    {
        services.AddSingleton<IProblemDetailsFactory, ErrorObjectFactory>();
        base.OnConfigureServices( services );
    }
}