namespace Microsoft.Web.Http.Dispatcher
{
    using Microsoft.Web.Http.Controllers;
    using Microsoft.Web.Http.Routing;
    using Microsoft.Web.Http.Versioning;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Net.Http;
    using System.Web.Http;
    using System.Web.Http.Controllers;
    using System.Web.Http.Routing;
    using static System.StringComparison;

    sealed class ControllerSelectionContext
    {
        readonly Lazy<string> controllerName;
        readonly Lazy<ConcurrentDictionary<string, HttpControllerDescriptorGroup>> controllerInfoCache;
        readonly Lazy<CandidateAction[]> conventionRouteCandidates;
        readonly Lazy<CandidateAction[]> directRouteCandidates;
        readonly Lazy<ApiVersionModel> allVersions;
        readonly ApiVersionRequestProperties requestProperties;

        internal ControllerSelectionContext(
            HttpRequestMessage request,
            Func<HttpRequestMessage, string> controllerName,
            Lazy<ConcurrentDictionary<string, HttpControllerDescriptorGroup>> controllerInfoCache )
        {
            Contract.Requires( request != null );
            Contract.Requires( controllerInfoCache != null );

            Request = request;
            requestProperties = request.ApiVersionProperties();
            this.controllerName = new Lazy<string>( () => controllerName( Request ) );
            this.controllerInfoCache = controllerInfoCache;
            RouteData = request.GetRouteData();
            conventionRouteCandidates = new Lazy<CandidateAction[]>( GetConventionRouteCandidates );
            directRouteCandidates = new Lazy<CandidateAction[]>( () => RouteData?.GetDirectRouteCandidates() );
            allVersions = new Lazy<ApiVersionModel>( CreateAggregatedModel );
        }

        internal HttpRequestMessage Request { get; }

        internal IHttpRouteData RouteData { get; }

        internal string ControllerName => controllerName.Value;

        internal ApiVersion RequestedVersion
        {
            get => requestProperties.RequestedApiVersion;
            set => requestProperties.RequestedApiVersion = value;
        }

        internal CandidateAction[] ConventionRouteCandidates => conventionRouteCandidates.Value;

        internal bool HasConventionBasedRoutes => ConventionRouteCandidates != null && ConventionRouteCandidates.Length > 0;

        internal CandidateAction[] DirectRouteCandidates => directRouteCandidates.Value;

        internal bool HasAttributeBasedRoutes => DirectRouteCandidates != null;

        internal ApiVersionModel AllVersions => allVersions.Value;

        static bool RouteTemplatesIntersect( string template1, string template2 ) =>
            template1.StartsWith( template2, OrdinalIgnoreCase ) || template2.StartsWith( template1, OrdinalIgnoreCase );

        static IEnumerable<HttpControllerDescriptor> EnumerateControllersInDataTokens( IDictionary<string, object> dataTokens )
        {
            Contract.Requires( dataTokens != null );
            Contract.Ensures( Contract.Result<IEnumerable<HttpControllerDescriptor>>() != null );

            if ( dataTokens.TryGetValue( RouteDataTokenKeys.Controller, out var value ) )
            {
                if ( value is HttpControllerDescriptor controllerDescriptor )
                {
                    yield return controllerDescriptor;
                }

                yield break;
            }

            if ( dataTokens.TryGetValue( RouteDataTokenKeys.Actions, out value ) )
            {
                if ( value is HttpActionDescriptor[] actionDescriptors )
                {
                    foreach ( var actionDescriptor in actionDescriptors )
                    {
                        yield return actionDescriptor.ControllerDescriptor;
                    }
                }
            }
        }

        IEnumerable<HttpControllerDescriptor> EnumerateDirectRoutes()
        {
            Contract.Ensures( Contract.Result<IEnumerable<HttpControllerDescriptor>>() != null );

            if ( RouteData == null || DirectRouteCandidates == null )
            {
                return Enumerable.Empty<HttpControllerDescriptor>();
            }

            var subroutes = RouteData.GetSubRoutes();
            var subroute = subroutes?.FirstOrDefault();

            if ( subroute == null || subroute.Values.Count == 0 )
            {
                return DirectRouteCandidates.Select( c => c.ActionDescriptor.ControllerDescriptor );
            }

            var config = Request.GetConfiguration();
            var routes = config?.Routes.OfType<IEnumerable<IHttpRoute>>().FirstOrDefault();

            if ( routes == null )
            {
                return DirectRouteCandidates.Select( c => c.ActionDescriptor.ControllerDescriptor );
            }

            var template = subroute.Route.RouteTemplate;
            var controllers = from route in routes
                              where RouteTemplatesIntersect( route.RouteTemplate, template )
                              from controller in EnumerateControllersInDataTokens( route.DataTokens )
                              select controller;

            return controllers.Distinct();
        }

        CandidateAction[] GetConventionRouteCandidates()
        {
            if ( string.IsNullOrEmpty( ControllerName ) || !controllerInfoCache.Value.TryGetValue( ControllerName, out var controllers ) )
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