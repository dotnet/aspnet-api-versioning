// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Mvc.UsingAttributes;

using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;

public class UIFixture : HttpServerFixture
{
    protected override void OnAddApiVersioning( ApiVersioningOptions options ) => options.ReportApiVersions = true;

    protected override void OnConfigurePartManager( ApplicationPartManager partManager )
    {
        partManager.FeatureProviders.Add( (IApplicationFeatureProvider) FilteredControllerTypes );
        partManager.ApplicationParts.Add( new AssemblyPart( GetType().Assembly ) );
    }

    protected override void OnConfigureServices( IServiceCollection services ) =>
        services.AddControllersWithViews().AddRazorRuntimeCompilation();
}