// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Dispatcher;

using Asp.Versioning.Routing;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using static Asp.Versioning.ApiVersionMapping;

internal sealed class ControllerSelectionContext
{
    private readonly Lazy<string?> controllerName;
    private readonly Lazy<IDictionary<string, HttpControllerDescriptor>> controllerInfoCache;
    private readonly Lazy<CandidateAction[]?> conventionRouteCandidates;
    private readonly Lazy<CandidateAction[]?> directRouteCandidates;
    private readonly Lazy<ApiVersionModel> allVersions;
    private readonly ApiVersionRequestProperties requestProperties;

    internal ControllerSelectionContext(
        HttpRequestMessage request,
        Func<HttpRequestMessage, string?> controllerName,
        Lazy<IDictionary<string, HttpControllerDescriptor>> controllerInfoCache )
    {
        Request = request;
        requestProperties = request.ApiVersionProperties();
        this.controllerName = new Lazy<string?>( () => controllerName( Request ) );
        this.controllerInfoCache = controllerInfoCache;
        RouteData = request.GetRouteData();
        conventionRouteCandidates = new Lazy<CandidateAction[]?>( GetConventionRouteCandidates );
        directRouteCandidates = new Lazy<CandidateAction[]?>( () => RouteData?.GetDirectRouteCandidates() );
        allVersions = new Lazy<ApiVersionModel>( CreateAggregatedModel );
    }

    internal HttpRequestMessage Request { get; }

    internal IHttpRouteData RouteData { get; }

    internal string? ControllerName => controllerName.Value;

    internal ApiVersion? RequestedVersion
    {
        get => requestProperties.RequestedApiVersion;
        set => requestProperties.RequestedApiVersion = value;
    }

    internal CandidateAction[]? ConventionRouteCandidates => conventionRouteCandidates.Value;

    internal bool HasConventionBasedRoutes => ConventionRouteCandidates != null && ConventionRouteCandidates.Length > 0;

    internal CandidateAction[]? DirectRouteCandidates => directRouteCandidates.Value;

    internal bool HasAttributeBasedRoutes => DirectRouteCandidates != null;

    internal ApiVersionModel AllVersions => allVersions.Value;

    private CandidateAction[]? GetConventionRouteCandidates()
    {
        if ( string.IsNullOrEmpty( ControllerName ) || !controllerInfoCache.Value.TryGetValue( ControllerName!, out var controllers ) )
        {
            return default;
        }

        var candidates = default( List<CandidateAction> );

        foreach ( var controller in controllers.AsEnumerable() )
        {
            var actionSelector = controller.Configuration.Services.GetActionSelector();
            var actions = actionSelector.GetActionMapping( controller ).SelectMany( g => g );

            foreach ( var action in actions )
            {
                candidates ??= [];
                candidates.Add( new( action ) );
            }
        }

        return candidates?.ToArray();
    }

    private ApiVersionModel CreateAggregatedModel()
    {
        var models = Enumerable.Empty<ApiVersionModel>();

        if ( HasConventionBasedRoutes )
        {
            models = models.Union( Enumerate( ConventionRouteCandidates! ) );
        }

        if ( HasAttributeBasedRoutes )
        {
            models = models.Union( Enumerate( DirectRouteCandidates! ) );
        }

        return models.Aggregate();
    }

    private static IEnumerable<ApiVersionModel> Enumerate( CandidateAction[] candidates )
    {
        for ( var i = 0; i < candidates.Length; i++ )
        {
            yield return candidates[i].ActionDescriptor.GetApiVersionMetadata().Map( Explicit );
        }
    }
}