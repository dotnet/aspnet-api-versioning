// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

// REF: https://github.com/dotnet/aspnetcore/blob/release/7.0/src/Http/Http.Extensions/src/DefaultProblemDetailsWriter.cs
internal sealed class Rfc7231ProblemDetailsWriter : IProblemDetailsWriter
{
    private static readonly MediaTypeHeaderValue jsonMediaType = new( "application/json" );
    private static readonly MediaTypeHeaderValue problemDetailsJsonMediaType = new( ProblemDetailsDefaults.MediaType.Json );
    private readonly IProblemDetailsWriter decorated;

    public Rfc7231ProblemDetailsWriter( IProblemDetailsWriter decorated ) => this.decorated = decorated;

    public bool CanWrite( ProblemDetailsContext context )
    {
        if ( decorated.CanWrite( context ) )
        {
            return true;
        }

        var httpContext = context.HttpContext;
        var accept = httpContext.Request.Headers.Accept;

        // REF: https://www.rfc-editor.org/rfc/rfc7231#section-5.3.2
        if ( accept.Count == 0 )
        {
            return true;
        }

        var acceptHeader = httpContext.Request.GetTypedHeaders().Accept;

        for ( var i = 0; i < acceptHeader.Count; i++ )
        {
            var acceptHeaderValue = acceptHeader[i];

            if ( acceptHeaderValue.IsSubsetOf( jsonMediaType ) ||
                 acceptHeaderValue.IsSubsetOf( problemDetailsJsonMediaType ) )
            {
                return true;
            }
        }

        return false;
    }

    public ValueTask WriteAsync( ProblemDetailsContext context ) => decorated.WriteAsync( context );
}