// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OData;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData;
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
            if ( !TryResolveOptions( out var options ) )
            {
                options = defaultOptions ??= new();
            }

            return options;
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
        set => mapping = value;
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
    /// Attempts to resolve the current OData options.
    /// </summary>
    /// <param name="options">The resolved <see cref="ODataOptions">OData options</see> or <c>null</c>.</param>
    /// <returns>True if the current OData were successfully resolved; otherwise, false.</returns>
    protected virtual bool TryResolveOptions( [NotNullWhen( true )] out ODataOptions? options )
    {
        if ( mapping == null ||
             mapping.Count == 0 ||
             httpContextAccessor.HttpContext is not HttpContext context )
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
}