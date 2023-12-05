// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0079
#pragma warning disable CA1812

namespace Asp.Versioning;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using static Asp.Versioning.ApiVersionParameterLocation;

internal sealed class ApplyContentTypeVersionActionFilter : IActionFilter
{
    private readonly string parameterName;

    public ApplyContentTypeVersionActionFilter( IApiVersionParameterSource source ) =>
        parameterName = source.GetParameterName( MediaTypeParameter );

    public void OnActionExecuted( ActionExecutedContext context ) { }

    public void OnActionExecuting( ActionExecutingContext context )
    {
        ArgumentNullException.ThrowIfNull( context );
        var httpContext = context.HttpContext;
        httpContext.Response.OnStarting( AddContentTypeApiVersion, httpContext );
    }

    private Task AddContentTypeApiVersion( object state )
    {
        var context = (HttpContext) state;
        context.Response.AddApiVersionToContentType( parameterName );
        return Task.CompletedTask;
    }
}