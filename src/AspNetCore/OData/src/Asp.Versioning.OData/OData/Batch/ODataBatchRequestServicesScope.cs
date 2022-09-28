// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OData.Batch;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

/// <summary>
/// Represents the request services scope for an OData batch request.
/// </summary>
[CLSCompliant( false )]
public class ODataBatchRequestServicesScope : IDisposable, IServiceProvider
{
    private readonly HttpContext context;
    private readonly IServiceProvider original;
    private readonly IOptions<ODataOptions> options;
    private readonly Type optionsType = typeof( IOptions<ODataOptions> );
    private bool disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="ODataBatchRequestServicesScope"/> class.
    /// </summary>
    /// <param name="context">The current <see cref="HttpContext">HTTP context</see>.</param>
    public ODataBatchRequestServicesScope( HttpContext context )
    {
        this.context = context ?? throw new ArgumentNullException( nameof( context ) );
        original = context.RequestServices;
        options = Options.Create( original.GetRequiredService<IOptions<ODataOptions>>().Value );
        context.RequestServices = this;
    }

    /// <inheritdoc />
    public virtual object? GetService( Type serviceType ) =>
        optionsType.Equals( serviceType ) ? options : original.GetService( serviceType );

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose( true );
        GC.SuppressFinalize( this );
    }

    /// <summary>
    /// Releases the managed and, optionally, the unmanaged resources used by the
    /// <see cref="ODataBatchRequestServicesScope"/> class.
    /// </summary>
    /// <param name="disposing">Indicates whether the object is being disposed.</param>
    protected virtual void Dispose( bool disposing )
    {
        if ( disposed )
        {
            return;
        }

        disposed = true;

        if ( disposing )
        {
            context.RequestServices = original;
        }
    }
}