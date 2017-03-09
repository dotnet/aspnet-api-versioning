namespace Microsoft.Web.Http.Dispatcher
{
    using Controllers;
    using Routing;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Net.Http;
    using System.Web.Http;
    using System.Web.Http.Controllers;
    using System.Web.Http.Routing;
    using Versioning;

    sealed class ApiVersionControllerAggregator
    {
        readonly Lazy<string> controllerName;
        readonly Lazy<ConcurrentDictionary<string, HttpControllerDescriptorGroup>> controllerInfoCache;
        readonly Lazy<HttpControllerDescriptorGroup> conventionRouteCandidates;
        readonly Lazy<CandidateAction[]> directRouteCandidates;
        readonly Lazy<ApiVersionModel> allVersions;

        internal ApiVersionControllerAggregator(
            HttpRequestMessage request,
            Func<HttpRequestMessage, string> controllerName,
            Lazy<ConcurrentDictionary<string, HttpControllerDescriptorGroup>> controllerInfoCache )
        {
            Contract.Requires( request != null );
            Contract.Requires( controllerInfoCache != null );

            Request = request;
            this.controllerName = new Lazy<string>( () => controllerName( Request ) );
            this.controllerInfoCache = controllerInfoCache;
            RouteData = request.GetRouteData();
            conventionRouteCandidates = new Lazy<HttpControllerDescriptorGroup>( GetConventionRouteCandidates );
            directRouteCandidates = new Lazy<CandidateAction[]>( () => RouteData?.GetDirectRouteCandidates() );
            allVersions = new Lazy<ApiVersionModel>( AggregateAllCandiateVersions );
        }

        HttpControllerDescriptorGroup GetConventionRouteCandidates()
        {
            if ( string.IsNullOrEmpty( ControllerName ) )
            {
                return null;
            }

            if ( controllerInfoCache.Value.TryGetValue( ControllerName, out var candidates ) )
            {
                return candidates;
            }

            return null;
        }

        ApiVersionModel AggregateAllCandiateVersions() =>
            ( ConventionRouteCandidates ?? Enumerable.Empty<HttpControllerDescriptor>() ).Union( EnumerateDirectRoutes() ).AggregateVersions();

        internal HttpRequestMessage Request { get; }

        internal IHttpRouteData RouteData { get; }

        internal string ControllerName => controllerName.Value;

        internal ApiVersion RequestedApiVersion => Request.GetRequestedApiVersion();

        internal HttpControllerDescriptorGroup ConventionRouteCandidates => conventionRouteCandidates.Value;

        internal bool HasConventionBasedRoutes => ConventionRouteCandidates != null && ConventionRouteCandidates.Count > 0;

        internal CandidateAction[] DirectRouteCandidates => directRouteCandidates.Value;

        internal bool HasAttributeBasedRoutes => DirectRouteCandidates != null;

        internal ApiVersionModel AllVersions => allVersions.Value;

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
            var comparer = StringComparer.OrdinalIgnoreCase;
            var controllers = from route in routes
                              where comparer.Equals( route.RouteTemplate, template )
                              from controller in EnumerateControllersInDataTokens( route.DataTokens )
                              select controller;

            return controllers.Distinct();
        }

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
    }
}