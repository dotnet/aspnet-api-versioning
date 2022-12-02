// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Routing;

using Microsoft.AspNetCore.Http;

internal sealed class ReportApiVersionsDecorator
{
    private readonly RequestDelegate decorated;
    private readonly IReportApiVersions reporter;
    private readonly ApiVersionModel model;

    public ReportApiVersionsDecorator( RequestDelegate decorated, IReportApiVersions reporter, ApiVersionMetadata metadata )
    {
        this.decorated = decorated;
        this.reporter = reporter;
        model = metadata.Map( reporter.Mapping );
    }

    public static implicit operator RequestDelegate( ReportApiVersionsDecorator decorator ) =>
        ( context ) =>
        {
            var response = context.Response;
            response.OnStarting( ReportApiVersions, (decorator, response) );
            return decorator.decorated( context );
        };

    private static Task ReportApiVersions( object state )
    {
        var (decorator, response) = ((ReportApiVersionsDecorator, HttpResponse)) state;
        decorator.reporter.Report( response, decorator.model );
        return Task.CompletedTask;
    }
}