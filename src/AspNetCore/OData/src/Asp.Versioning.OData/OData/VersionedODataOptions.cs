// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OData;

using Asp.Versioning.OData.Batch;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Batch;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

/// <summary>
/// Represents the detailed configuration options of a versioned OData request.
/// </summary>
[CLSCompliant( false )]
public class VersionedODataOptions : IOptions<ODataOptions>
{
    private readonly IHttpContextAccessor httpContextAccessor;
    private ODataOptions? defaultOptions;
    private IReadOnlyDictionary<ApiVersion, ODataOptions>? mapping;
    private ODataBatchPathMapping? batchMapping;

    /// <summary>
    /// Initializes a new instance of the <see cref="VersionedODataOptions"/> class.
    /// </summary>
    /// <param name="httpContextAccessor">The <see cref="IHttpContextAccessor">accessor</see>
    /// for the current <see cref="HttpContext">HTTP context</see>.</param>
    /// <param name="apiVersionSelector">The <see cref="IApiVersionSelector">API version selector</see>
    /// used to select an API version when it cannot otherwise be determined.</param>
    public VersionedODataOptions(
        IHttpContextAccessor httpContextAccessor,
        IApiVersionSelector apiVersionSelector )
    {
        this.httpContextAccessor = httpContextAccessor;
        ApiVersionSelector = apiVersionSelector;
    }

    /// <summary>
    /// Gets the current OData options.
    /// </summary>
    /// <value>The current <see cref="ODataOptions">OData options</see>.</value>
    public ODataOptions Value
    {
        get
        {
            if ( TryResolveOptions( out var value ) )
            {
                return value;
            }

            return defaultOptions ??= new();
        }
    }

    /// <summary>
    /// Gets or sets the mapping of API version to OData options.
    /// </summary>
    /// <value>A <see cref="IReadOnlyDictionary{TKey, TValue}">read-only dictionary</see>
    /// of <see cref="ApiVersion">API version</see> to <see cref="ODataOptions">OData options</see>.</value>
    public IReadOnlyDictionary<ApiVersion, ODataOptions> Mapping
    {
        get => mapping ?? new Dictionary<ApiVersion, ODataOptions>( capacity: 0 );
        set
        {
            mapping = value;
            batchMapping = MapBatchPaths( mapping, ApiVersionSelector );
        }
    }

    /// <summary>
    /// Gets the current HTTP context.
    /// </summary>
    /// <value>The current <see cref="HttpContext">HTTP context</see>, if any.</value>
    protected HttpContext? HttpContext => httpContextAccessor.HttpContext;

    /// <summary>
    /// Gets the selector used to choose API versions.
    /// </summary>
    /// <value>The associated <see cref="IApiVersionSelector">API version selector</see>.</value>
    protected IApiVersionSelector ApiVersionSelector { get; }

    /// <summary>
    /// Attempts to retrieve the configured batch handler for the current context.
    /// </summary>
    /// <param name="context">The current <see cref="HttpContext">HTTP context</see>.</param>
    /// <param name="handler">The retrieved <see cref="ODataBatchHandler">OData batch handler</see> or <c>null</c>.</param>
    /// <returns>True if the <paramref name="handler"/> was successfully retrieved; otherwise, false.</returns>
    public virtual bool TryGetBatchHandler( HttpContext context, [NotNullWhen( true )] out ODataBatchHandler? handler )
    {
        ArgumentNullException.ThrowIfNull( context );

        if ( batchMapping is null )
        {
            handler = default;
            return false;
        }

        return batchMapping.TryGetHandler( context, out handler );
    }

    /// <summary>
    /// Attempts to get the current OData options.
    /// </summary>
    /// <param name="context">The current <see cref="HttpContext">HTTP context</see>.</param>
    /// <param name="options">The resolved <see cref="ODataOptions">OData options</see> or <c>null</c>.</param>
    /// <returns>True if the current OData were successfully resolved; otherwise, false.</returns>
    public virtual bool TryGetValue( HttpContext? context, [NotNullWhen( true )] out ODataOptions? options )
    {
        if ( context == null || mapping == null || mapping.Count == 0 )
        {
            options = default;
            return false;
        }

        var apiVersion = context.GetRequestedApiVersion();

        if ( apiVersion == null )
        {
            var model = new ApiVersionModel( mapping.Keys, Array.Empty<ApiVersion>() );
            apiVersion = ApiVersionSelector.SelectVersion( context.Request, model );

            if ( apiVersion == null )
            {
                options = default;
                return false;
            }
        }

        return mapping.TryGetValue( apiVersion, out options );
    }

    /// <summary>
    /// Attempts to resolve the current OData options.
    /// </summary>
    /// <param name="options">The resolved <see cref="ODataOptions">OData options</see> or <c>null</c>.</param>
    /// <returns>True if the current OData were successfully resolved; otherwise, false.</returns>
    protected virtual bool TryResolveOptions( [NotNullWhen( true )] out ODataOptions? options ) =>
        TryGetValue( httpContextAccessor.HttpContext, out options );

    private static ODataBatchPathMapping MapBatchPaths(
        IReadOnlyDictionary<ApiVersion, ODataOptions> mapping,
        IApiVersionSelector selector )
    {
        if ( mapping.Count == 0 )
        {
            return new( capacity: 0, selector );
        }

        var count = mapping.Values.Sum( value => value.RouteComponents.Count );
        var batchMapping = new ODataBatchPathMapping( count, selector );

        foreach ( var (version, options) in mapping )
        {
            foreach ( var (prefix, (_, serviceProvider)) in options.RouteComponents )
            {
                if ( serviceProvider.GetService<ODataBatchHandler>() is not ODataBatchHandler handler )
                {
                    continue;
                }

                var template = "/$batch";

                if ( !string.IsNullOrEmpty( prefix ) )
                {
                    template = '/' + prefix.Trim( '/' ) + template;
                }

                batchMapping.Add( prefix, template, handler, version );
            }
        }

        return batchMapping;
    }
}