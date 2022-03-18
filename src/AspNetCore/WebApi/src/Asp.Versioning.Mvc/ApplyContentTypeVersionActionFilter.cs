// Copyright (c) .NET Foundation and contributors. All rights reserved.

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
        if ( context == null )
        {
            throw new ArgumentNullException( nameof( context ) );
        }

        var httpContext = context.HttpContext;

        httpContext.Response.OnStarting( AddContentTypeApiVersion, (httpContext, parameterName) );
    }

    private Task AddContentTypeApiVersion( object state )
    {
        var (context, parameterName) = ((HttpContext, string)) state;
        context.Response.AddApiVersionToContentType( parameterName );
        return Task.CompletedTask;
    }
}