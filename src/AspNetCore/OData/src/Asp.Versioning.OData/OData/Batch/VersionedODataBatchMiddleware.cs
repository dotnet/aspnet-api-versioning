// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OData.Batch;

using Microsoft.AspNetCore.Http;

/// <summary>
/// Represents the versioned OData batching middleware.
/// </summary>
[CLSCompliant( false )]
public class VersionedODataBatchMiddleware
{
    private readonly RequestDelegate next;
    private readonly VersionedODataOptions options;

    /// <summary>
    /// Initializes a new instance of the <see cref="VersionedODataBatchMiddleware"/> class.
    /// </summary>
    /// <param name="next">The <see cref="RequestDelegate"/> representing the next step in the request pipeline.</param>
    /// <param name="options">The <see cref="VersionedODataOptions">options</see> used in batch requests.</param>
    public VersionedODataBatchMiddleware( RequestDelegate next, VersionedODataOptions options )
    {
        this.next = next ?? throw new ArgumentNullException( nameof( next ) );
        this.options = options ?? throw new ArgumentNullException( nameof( options ) );
    }

    /// <summary>
    /// Invokes the middleware.
    /// </summary>
    /// <param name="context">The current <see cref="HttpContext">HTTP context</see>.</param>
    /// <returns>A <see cref="Task">task</see> representing the asynchronous operation.</returns>
    public Task Invoke( HttpContext context )
    {
        ArgumentNullException.ThrowIfNull( context );

        if ( HttpMethods.IsPost( context.Request.Method ) &&
             options.TryGetBatchHandler( context, out var handler ) )
        {
            return handler.ProcessBatchAsync( context, next );
        }

        return next( context );
    }
}