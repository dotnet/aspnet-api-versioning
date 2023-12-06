// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OData.Batch;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Batch;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Template;
using System.Diagnostics;

internal sealed class ODataBatchPathMapping
{
    private readonly (TemplateMatcher, ODataBatchHandler, ApiVersion)[] mappings;
    private readonly IApiVersionSelector selector;
    private int count;

    internal ODataBatchPathMapping( int capacity, IApiVersionSelector selector )
    {
        mappings = new ValueTuple<TemplateMatcher, ODataBatchHandler, ApiVersion>[capacity];
        this.selector = selector;
    }

    public void Add( string prefixName, string routeTemplate, ODataBatchHandler handler, ApiVersion version )
    {
        Debug.Assert( count < mappings.Length, "The capacity has been exceeded." );

        var template = TemplateParser.Parse( routeTemplate.TrimStart( '/' ) );
        var matcher = new TemplateMatcher( template, [] );

        handler.PrefixName = prefixName;
        mappings[count++] = (matcher, handler, version);
    }

    public bool TryGetHandler( HttpContext context, [NotNullWhen( true )] out ODataBatchHandler? batchHandler )
    {
        if ( count == 0 )
        {
            batchHandler = default;
            return false;
        }

        var routeData = new RouteValueDictionary();
        var candidates = new Dictionary<ApiVersion, int>( capacity: mappings.Length );

        batchHandler = SelectExactMatch( context, routeData, candidates ) ??
                       SelectBestCandidate( context, candidates, routeData );

        return batchHandler is not null;
    }

    public ValueTask<ODataBatchHandler?> TryGetHandlerAsync( HttpContext context, CancellationToken cancellationToken )
    {
        if ( count == 0 )
        {
            return ValueTask.FromResult( default( ODataBatchHandler ) );
        }

        var routeData = new RouteValueDictionary();
        var candidates = new Dictionary<ApiVersion, int>( capacity: mappings.Length );

        if ( SelectExactMatch( context, routeData, candidates ) is var handler )
        {
            return ValueTask.FromResult( handler );
        }

        return SelectBestCandidateAsync( context, candidates, routeData, cancellationToken );
    }

    private static void MergeRouteData( HttpContext context, RouteValueDictionary routeData )
    {
        if ( routeData.Count == 0 )
        {
            return;
        }

        var batchRouteData = context.ODataFeature().BatchRouteData;

        foreach ( var (key, value) in routeData )
        {
            batchRouteData.Add( key, value );
        }
    }

    private ODataBatchHandler? SelectExactMatch(
        HttpContext context,
        RouteValueDictionary routeData,
        Dictionary<ApiVersion, int> candidates )
    {
        var path = context.Request.Path;
        var feature = context.ApiVersioningFeature();
        var unspecified = feature.RawRequestedApiVersions.Count == 0;

        for ( var i = 0; i < count; i++ )
        {
            ref readonly var mapping = ref mappings[i];
            var (matcher, handler, version) = mapping;

            if ( !matcher.TryMatch( path, routeData ) )
            {
                routeData.Clear();
                continue;
            }

            // odata now uses a batch handler per prefix, when there need only be one
            // for the entire application. the batch handler itself is, or at least
            // should be, version-neutral. try to match the declared version.
            if ( feature.RequestedApiVersion != version )
            {
                if ( unspecified )
                {
                    // when unspecified, track which mappings are potential candidates
                    candidates.Add( version, i );
                }

                routeData.Clear();
                continue;
            }

            MergeRouteData( context, routeData );
            return handler;
        }

        return default;
    }

    private ODataBatchHandler? SelectBestCandidate(
        HttpContext context,
        Dictionary<ApiVersion, int> candidates,
        RouteValueDictionary routeData,
        ApiVersion version )
    {
        if ( version is null || !candidates.TryGetValue( version, out var index ) )
        {
            return default;
        }

        ref readonly var mapping = ref mappings[index];
        var (matcher, handler, _) = mapping;

        routeData.Clear();
        matcher.TryMatch( context.Request.Path, routeData );
        MergeRouteData( context, routeData );

        // it's important that the resolved api version be set here to ensure the correct
        // ODataOptions are resolved by ODataBatchHandler when executed
        context.ApiVersioningFeature().RequestedApiVersion = version;

        return handler;
    }

    private ODataBatchHandler? SelectBestCandidate(
        HttpContext context,
        Dictionary<ApiVersion, int> candidates,
        RouteValueDictionary routeData )
    {
        if ( candidates.Count == 0 )
        {
            return default;
        }

        // ~/$batch is always version-neutral so there is no need to check
        // ApiVersioningOptions.AllowDefaultVersionWhenUnspecified. use the
        // configured IApiVersionSelector to provide a chance to select the
        // most appropriate version.
        var model = new ApiVersionModel( candidates.Keys, Enumerable.Empty<ApiVersion>() );
        var version = selector.SelectVersion( context.Request, model );

        return SelectBestCandidate( context, candidates, routeData, version );
    }

    private async ValueTask<ODataBatchHandler?> SelectBestCandidateAsync(
        HttpContext context,
        Dictionary<ApiVersion, int> candidates,
        RouteValueDictionary routeData,
        CancellationToken cancellationToken )
    {
        if ( candidates.Count == 0 )
        {
            return default;
        }

        // ~/$batch is always version-neutral so there is no need to check
        // ApiVersioningOptions.AllowDefaultVersionWhenUnspecified. use the
        // configured IApiVersionSelector to provide a chance to select the
        // most appropriate version.
        var model = new ApiVersionModel( candidates.Keys, Enumerable.Empty<ApiVersion>() );
        var version = await selector.SelectVersionAsync( context.Request, model, cancellationToken ).ConfigureAwait( false );

        return SelectBestCandidate( context, candidates, routeData, version );
    }
}