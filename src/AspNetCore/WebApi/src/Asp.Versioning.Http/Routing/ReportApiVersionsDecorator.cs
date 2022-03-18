// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Routing;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

internal sealed class ReportApiVersionsDecorator
{
    private readonly RequestDelegate decorated;
    private readonly ApiVersionMetadata metadata;

    public ReportApiVersionsDecorator( RequestDelegate decorated, ApiVersionMetadata metadata )
    {
        this.decorated = decorated;
        this.metadata = metadata;
    }

    public static implicit operator RequestDelegate( ReportApiVersionsDecorator decorator ) =>
        ( context ) =>
        {
            var reporter = context.RequestServices.GetRequiredService<IReportApiVersions>();
            var model = decorator.metadata.Map( reporter.Mapping );
            var response = context.Response;

            response.OnStarting( ReportApiVersions, (reporter, response, model) );

            return decorator.decorated( context );
        };

    private static Task ReportApiVersions( object state )
    {
        var (reporter, response, model) = ((IReportApiVersions, HttpResponse, ApiVersionModel)) state;
        reporter.Report( response, model );
        return Task.CompletedTask;
    }
}