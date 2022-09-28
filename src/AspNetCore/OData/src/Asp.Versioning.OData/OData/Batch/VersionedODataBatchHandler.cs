// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OData.Batch;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Batch;

/// <summary>
/// Represents a versioned OData batch handler.
/// </summary>
[CLSCompliant( false )]
public class VersionedODataBatchHandler : DefaultODataBatchHandler
{
    /// <inheritdoc />
    public override async Task ProcessBatchAsync( HttpContext context, RequestDelegate nextHandler )
    {
        // HACK: IHttpContextAccessor will NOT flow correctly between the top-level batch
        // request and the subrequests. This prevents VersionedODataOptions from resolving
        // the correct ODataOptions at different continuations. To address this, capture
        // the current ODataOptions now and decorate IServiceProvider so that it will
        // consistently return the same state of ODataOptions in this scope. Next, flow
        // the rest of the request as normal and then revert IServiceProvider upon completion.
        //
        // REF: https://github.com/OData/WebApi/issues/2294
        using var scope = new ODataBatchRequestServicesScope( context );
        await base.ProcessBatchAsync( context, nextHandler ).ConfigureAwait( true );
    }
}