// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Mvc.UsingNamespace;

using Asp.Versioning;
using Asp.Versioning.ApplicationModels;
using Asp.Versioning.Conventions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

public class AgreementsFixture : HttpServerFixture
{
    public AgreementsFixture()
    {
        FilteredControllerTypes.Add( typeof( Controllers.V1.AgreementsController ) );
        FilteredControllerTypes.Add( typeof( Controllers.V2.AgreementsController ) );
        FilteredControllerTypes.Add( typeof( Controllers.V3.AgreementsController ) );
    }

    protected override void OnAddApiVersioning( ApiVersioningOptions options ) =>
        options.ReportApiVersions = true;

    protected override void OnAddMvcApiVersioning( MvcApiVersioningOptions options ) =>
        options.Conventions.Add( new VersionByNamespaceConvention() );

    protected override void OnConfigureServices( IServiceCollection services )
    {
        base.OnConfigureServices( services );
        services.AddSingleton<IApiControllerFilter, NoControllerFilter>();
    }

    protected override void OnConfigureEndpoints( IEndpointRouteBuilder endpoints )
    {
        endpoints.MapControllerRoute( "VersionedQueryString", "api/{controller}/{accountId}/{action=Get}" );
        endpoints.MapControllerRoute( "VersionedUrl", "v{version:apiVersion}/{controller}/{accountId}/{action=Get}" );
    }
}