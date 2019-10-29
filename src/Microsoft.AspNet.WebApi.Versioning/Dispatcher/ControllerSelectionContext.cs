namespace Microsoft.Web.Http.Dispatcher
{
    using Microsoft.Web.Http.Controllers;
    using Microsoft.Web.Http.Routing;
    using Microsoft.Web.Http.Versioning;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Web.Http;
    using System.Web.Http.Routing;

    sealed class ControllerSelectionContext
    {
        readonly Lazy<string?> controllerName;
        readonly Lazy<ConcurrentDictionary<string, HttpControllerDescriptorGroup>> controllerInfoCache;
        readonly Lazy<CandidateAction[]?> conventionRouteCandidates;
        readonly Lazy<CandidateAction[]?> directRouteCandidates;
        readonly Lazy<ApiVersionModel> allVersions;
        readonly ApiVersionRequestProperties requestProperties;

        internal ControllerSelectionContext(
            HttpRequestMessage request,
            Func<HttpRequestMessage, string?> controllerName,
            Lazy<ConcurrentDictionary<string, HttpControllerDescriptorGroup>> controllerInfoCache )
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

        CandidateAction[]? GetConventionRouteCandidates()
        {
            if ( string.IsNullOrEmpty( ControllerName ) || !controllerInfoCache.Value.TryGetValue( ControllerName!, out var controllers ) )
            {
                return default;
            }

            var candidates = new List<CandidateAction>();

            foreach ( var controller in controllers )
            {
                var actionSelector = controller.Configuration.Services.GetActionSelector();
                var actions = actionSelector.GetActionMapping( controller ).SelectMany( g => g );

                foreach ( var action in actions )
                {
                    candidates.Add( new CandidateAction( action ) );
                }
            }

            return candidates.ToArray();
        }

        ApiVersionModel CreateAggregatedModel()
        {
            var models = Enumerable.Empty<ApiVersionModel>();

            if ( HasConventionBasedRoutes )
            {
                models = models.Union( ConventionRouteCandidates.Select( c => c.ActionDescriptor.GetApiVersionModel() ) );
            }

            if ( HasAttributeBasedRoutes )
            {
                models = models.Union( DirectRouteCandidates.Select( c => c.ActionDescriptor.GetApiVersionModel() ) );
            }

            return models.Aggregate();
        }
    }
}