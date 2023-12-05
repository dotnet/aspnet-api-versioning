// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Controllers;

using System.Globalization;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Services;
using static Asp.Versioning.ApiVersionMapping;

/// <summary>
/// Represents the logic for selecting a versioned, action method.
/// </summary>
public class ApiVersionActionSelector : IHttpActionSelector
{
    private readonly object cacheKey = new();
    private ActionSelectorCacheItem? fastCache;

    /// <summary>
    /// Selects and returns the action descriptor to invoke given the provided controller context.
    /// </summary>
    /// <param name="controllerContext">The current <see cref="HttpControllerContext">controller context</see>.</param>
    /// <returns>The <see cref="HttpActionDescriptor">action descriptor</see> that matches the current
    /// <paramref name="controllerContext">controller context</paramref>.</returns>
    public virtual HttpActionDescriptor? SelectAction( HttpControllerContext controllerContext )
    {
        ArgumentNullException.ThrowIfNull( controllerContext );
        var internalSelector = GetInternalSelector( controllerContext.ControllerDescriptor );
        return internalSelector.SelectAction( controllerContext, SelectActionVersion );
    }

    /// <summary>
    /// Creates and returns an action descriptor mapping for the specified controller descriptor.
    /// </summary>
    /// <param name="controllerDescriptor">The <see cref="HttpControllerDescriptor">controller descriptor</see> to create a mapping for.</param>
    /// <returns>A <see cref="ILookup{TKey,TValue}">lookup</see>, which represents the route-to-action mapping for the
    /// specified <paramref name="controllerDescriptor">controller descriptor</paramref>.</returns>
    public virtual ILookup<string, HttpActionDescriptor> GetActionMapping( HttpControllerDescriptor controllerDescriptor )
    {
        ArgumentNullException.ThrowIfNull( controllerDescriptor );

        var actionMappings = ( from descriptor in controllerDescriptor.AsEnumerable( includeCandidates: true )
                               let selector = GetInternalSelector( descriptor )
                               select selector.GetActionMapping() ).ToArray();

        return actionMappings.Length == 1 ? actionMappings[0] : new AggregatedActionMapping( actionMappings );
    }

    /// <summary>
    /// Selects the version of an action using the provided controller context and candidate action descriptors.
    /// </summary>
    /// <param name="controllerContext">The current <see cref="HttpControllerContext">controller context</see>.</param>
    /// <param name="candidateActions">The <see cref="IReadOnlyList{T}">read-only list</see> of candidate
    /// <see cref="HttpActionDescriptor">action descriptors</see> to select from.</param>
    /// <returns>The matching <see cref="HttpActionDescriptor">action descriptor</see> or <c>null</c> is no
    /// match is found.</returns>
    /// <remarks>This method should return <c>null</c> if either no match is found or the matched action is
    /// ambiguous among the provided list of <paramref name="candidateActions">candidate actions</paramref>.</remarks>
    protected virtual HttpActionDescriptor? SelectActionVersion( HttpControllerContext controllerContext, IReadOnlyList<HttpActionDescriptor> candidateActions )
    {
        ArgumentNullException.ThrowIfNull( controllerContext );
        ArgumentNullException.ThrowIfNull( candidateActions );

        if ( candidateActions.Count == 0 )
        {
            return null;
        }

        var request = controllerContext.Request;
        var requestedVersion = request.GetRequestedApiVersion();

        if ( candidateActions.Count == 1 )
        {
            var action = candidateActions[0];
            var metadata = action.GetApiVersionMetadata();

            return metadata.MappingTo( requestedVersion ) != None ? action : null;
        }

        var bestMatches = new List<HttpActionDescriptor>( candidateActions.Count );
        var implicitMatches = new List<HttpActionDescriptor>( bestMatches.Count );

        for ( var i = 0; i < candidateActions.Count; i++ )
        {
            var action = candidateActions[i];
            var metadata = action.GetApiVersionMetadata();

            switch ( metadata.MappingTo( requestedVersion ) )
            {
                case Explicit:
                    bestMatches.Add( action );
                    break;
                case Implicit:
                    implicitMatches.Add( action );
                    break;
            }
        }

        return bestMatches.Count switch
        {
            0 => implicitMatches.Count switch
            {
                0 => default,
                1 => implicitMatches[0],
                _ => throw CreateAmbiguousActionException( implicitMatches ),
            },
            1 => bestMatches[0],
            _ => throw CreateAmbiguousActionException( bestMatches ),
        };
    }

    private static Exception CreateAmbiguousActionException( IEnumerable<HttpActionDescriptor> matches )
    {
        var ambiguityList = ActionSelectorCacheItem.CreateAmbiguousMatchList( matches );
        var message = string.Format( CultureInfo.CurrentCulture, SR.ApiControllerActionSelector_AmbiguousMatch, ambiguityList );
        return new InvalidOperationException( message );
    }

    private ActionSelectorCacheItem GetInternalSelector( HttpControllerDescriptor controllerDescriptor )
    {
        controllerDescriptor = Decorator.GetInner( controllerDescriptor );
        ActionSelectorCacheItem selector;

        if ( fastCache == null )
        {
            selector = new( controllerDescriptor );
            Interlocked.CompareExchange( ref fastCache, selector, null );
            return selector;
        }

        if ( fastCache.HttpControllerDescriptor == controllerDescriptor )
        {
            return fastCache;
        }

        if ( controllerDescriptor.Properties.TryGetValue( cacheKey, out var cacheValue ) )
        {
            return (ActionSelectorCacheItem) cacheValue;
        }

        selector = new( controllerDescriptor );
        controllerDescriptor.Properties.TryAdd( cacheKey, selector );
        return selector;
    }
}