// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OData.Batch;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Batch;

/// <summary>
/// Represents a versioned, unbuffered OData batch handler.
/// </summary>
[CLSCompliant( false )]
public class VersionedUnbufferedODataBatchHandler : UnbufferedODataBatchHandler
{
    /// <inheritdoc />
    public override async Task ProcessBatchAsync( HttpContext context, RequestDelegate nextHandler )
    {
        using var scope = new ODataBatchRequestServicesScope( context );
        await base.ProcessBatchAsync( context, nextHandler ).ConfigureAwait( true );
    }
}