// Copyright (c) .NET Foundation and contributors. All rights reserved.

// Ignore Spelling: Interop
namespace Asp.Versioning.Mvc.UsingAttributes;

using Microsoft.Extensions.DependencyInjection;

public class InteropFixture : BasicFixture
{
    protected override void OnConfigureServices( IServiceCollection services )
    {
        services.AddSingleton<IProblemDetailsFactory, ErrorObjectFactory>();
        base.OnConfigureServices( services );
    }
}