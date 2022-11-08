// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Routing;

using Microsoft.AspNetCore.Http;

internal sealed class ContentTypeApiVersionDecorator
{
    private readonly RequestDelegate decorated;
    private readonly string parameterName;

    public ContentTypeApiVersionDecorator( RequestDelegate decorated, string parameterName )
    {
        this.decorated = decorated;
        this.parameterName = parameterName;
    }

    public static implicit operator RequestDelegate( ContentTypeApiVersionDecorator decorator ) =>
        context =>
        {
            context.Response.OnStarting( AddContentTypeApiVersion, (context, decorator.parameterName) );
            return decorator.decorated( context );
        };

    private static Task AddContentTypeApiVersion( object state )
    {
        var (context, parameterName) = ((HttpContext, string)) state;
        context.Response.AddApiVersionToContentType( parameterName );
        return Task.CompletedTask;
    }
}