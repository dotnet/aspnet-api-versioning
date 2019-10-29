namespace Microsoft.Web.Http.Description
{
    using Microsoft.Web.Http.Routing;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Formatting;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Web.Http;
    using System.Web.Http.Controllers;
    using System.Web.Http.Description;
    using System.Web.Http.ModelBinding.Binders;
    using System.Web.Http.Routing;
    using System.Web.Http.Services;
    using static Microsoft.Web.Http.Versioning.ApiVersionMapping;
    using static System.Globalization.CultureInfo;
    using static System.String;
    using static System.Text.RegularExpressions.RegexOptions;
    using static System.Web.Http.Description.ApiParameterSource;

    /// <summary>
    /// Explores the URI space of the versioned services based on routes, controllers and actions available in the system.
    /// </summary>
    public class VersionedApiExplorer : IApiExplorer
    {
        static readonly Regex actionVariableRegex = new Regex( $"{{{RouteValueKeys.Action}}}", Compiled | IgnoreCase | CultureInvariant );
        static readonly Regex controllerVariableRegex = new Regex( $"{{{RouteValueKeys.Controller}}}", Compiled | IgnoreCase | CultureInvariant );
        readonly Lazy<ApiDescriptionGroupCollection> apiDescriptions;
        IDocumentationProvider? documentationProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="VersionedApiExplorer"/> class.
        /// </summary>
        /// <param name="configuration">The current <see cref="HttpConfiguration">HTTP configuration</see>.</param>
        public VersionedApiExplorer( HttpConfiguration configuration ) : this( configuration, new ApiExplorerOptions( configuration ) ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="VersionedApiExplorer"/> class.
        /// </summary>
        /// <param name="configuration">The current <see cref="HttpConfiguration">HTTP configuration</see>.</param>
        /// <param name="options">The associated <see cref="ApiExplorerOptions">API explorer options</see>.</param>
        public VersionedApiExplorer( HttpConfiguration configuration, ApiExplorerOptions options )
        {
            Configuration = configuration;
            Options = options;
            apiDescriptions = new Lazy<ApiDescriptionGroupCollection>( InitializeApiDescriptions );
        }

        /// <summary>
        /// Gets the current configuration associated with the API explorer.
        /// </summary>
        /// <value>The current <see cref="HttpConfiguration">HTTP configuration</see>.</value>
        protected HttpConfiguration Configuration { get; }

        /// <summary>
        /// Gets the options associated with the API explorer.
        /// </summary>
        /// <value>The <see cref="ApiExplorerOptions">API explorer options</see>.</value>
        protected virtual ApiExplorerOptions Options { get; }

        /// <summary>
        /// Gets the comparer used to compare API descriptions.
        /// </summary>
        /// <value>A <see cref="ApiDescriptionComparer">comparer</see> for <see cref="ApiDescription">API descriptions</see>.</value>
        protected virtual ApiDescriptionComparer Comparer { get; } = new ApiDescriptionComparer();

        /// <summary>
        /// Gets the object used to parse routes.
        /// </summary>
        /// <value>The configured <see cref="RouteParser">route parser</see>.</value>
        protected virtual RouteParser RouteParser { get; } = new RouteParser();

        Collection<ApiDescription> IApiExplorer.ApiDescriptions => ApiDescriptions.Flatten();

        /// <summary>
        /// Gets a collection of descriptions grouped by API version.
        /// </summary>
        /// <value>An <see cref="ApiDescriptionGroupCollection">API description group collection</see>.</value>
        public virtual ApiDescriptionGroupCollection ApiDescriptions => apiDescriptions.Value;

        /// <summary>
        /// Gets or sets the documentation provider. The provider will be responsible for documenting the API.
        /// </summary>
        /// <value>The <see cref="IDocumentationProvider">documentation provider</see> used to document APIs.</value>
        public IDocumentationProvider DocumentationProvider
        {
            get => documentationProvider ??= Configuration.Services.GetDocumentationProvider();
            set => documentationProvider = value;
        }

        /// <summary>
        /// Gets a collection of HTTP methods supported by the action.
        /// </summary>
        /// <param name="route">The associated <see cref="IHttpRoute">route</see>.</param>
        /// <param name="actionDescriptor">The <see cref="HttpActionDescriptor">action descriptor</see> to get the HTTP methods for.</param>
        /// <returns>A <see cref="Collection{T}">collection</see> of <see cref="HttpMethod">HTTP method</see>.</returns>
        protected virtual Collection<HttpMethod> GetHttpMethodsSupportedByAction( IHttpRoute route, HttpActionDescriptor actionDescriptor )
        {
            if ( route == null )
            {
                throw new ArgumentNullException( nameof( route ) );
            }

            if ( actionDescriptor == null )
            {
                throw new ArgumentNullException( nameof( actionDescriptor ) );
            }

            IList<HttpMethod> supportedMethods;
            IList<HttpMethod> actionHttpMethods = actionDescriptor.SupportedHttpMethods;
            var httpMethodConstraint = route.Constraints.Values.OfType<HttpMethodConstraint>().FirstOrDefault();

            if ( httpMethodConstraint == null )
            {
                supportedMethods = actionHttpMethods;
            }
            else
            {
                supportedMethods = httpMethodConstraint.AllowedMethods.Intersect( actionHttpMethods ).ToList();
            }

            return new Collection<HttpMethod>( supportedMethods );
        }

        /// <summary>
        /// Determines whether the action should be considered.
        /// </summary>
        /// <param name="actionRouteParameterValue">The action route parameter value.</param>
        /// <param name="actionDescriptor">The associated <see cref="HttpActionDescriptor">action descriptor</see>.</param>
        /// <param name="route">The associated <see cref="IHttpRoute">route</see>.</param>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> to consider the controller for.</param>
        /// <returns>True if the action should be explored; otherwise, false.</returns>
        protected virtual bool ShouldExploreAction( string actionRouteParameterValue, HttpActionDescriptor actionDescriptor, IHttpRoute route, ApiVersion apiVersion )
        {
            if ( actionDescriptor == null )
            {
                throw new ArgumentNullException( nameof( actionDescriptor ) );
            }

            if ( route == null )
            {
                throw new ArgumentNullException( nameof( route ) );
            }

            var setting = actionDescriptor.GetCustomAttributes<ApiExplorerSettingsAttribute>().FirstOrDefault();

            if ( ( setting == null || !setting.IgnoreApi ) && MatchRegexConstraint( route, RouteValueKeys.Action, actionRouteParameterValue ) )
            {
                return actionDescriptor.IsMappedTo( apiVersion );
            }

            return false;
        }

        /// <summary>
        /// Determines whether the controller should be considered.
        /// </summary>
        /// <param name="controllerRouteParameterValue">The controller route parameter value.</param>
        /// <param name="controllerDescriptor">The associated <see cref="HttpControllerDescriptor">controller descriptor</see>.</param>
        /// <param name="route">The associated <see cref="IHttpRoute">route</see>.</param>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> to consider the controller for.</param>
        /// <returns>True if the controller should be explored; otherwise, false.</returns>
        protected virtual bool ShouldExploreController( string controllerRouteParameterValue, HttpControllerDescriptor controllerDescriptor, IHttpRoute route, ApiVersion apiVersion )
        {
            if ( controllerDescriptor == null )
            {
                throw new ArgumentNullException( nameof( controllerDescriptor ) );
            }

            if ( route == null )
            {
                throw new ArgumentNullException( nameof( route ) );
            }

            var setting = controllerDescriptor.GetCustomAttributes<ApiExplorerSettingsAttribute>().FirstOrDefault();

            return ( setting == null || !setting.IgnoreApi ) && MatchRegexConstraint( route, RouteValueKeys.Controller, controllerRouteParameterValue );
        }

        /// <summary>
        /// Returns the group name for the specified API version.
        /// </summary>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> to retrieve a group name for.</param>
        /// <returns>The group name for the specified <paramref name="apiVersion">API version</paramref>.</returns>
        protected virtual string GetGroupName( ApiVersion apiVersion )
        {
            if ( apiVersion == null )
            {
                throw new ArgumentNullException( nameof( apiVersion ) );
            }

            return apiVersion.ToString( Options.GroupNameFormat, InvariantCulture );
        }

        /// <summary>
        /// Initializes the API descriptions to explore.
        /// </summary>
        /// <returns>A new <see cref="ApiDescriptionGroupCollection">collection</see> of
        /// <see cref="ApiDescriptionGroup">API description groups</see>.</returns>
        protected virtual ApiDescriptionGroupCollection InitializeApiDescriptions()
        {
            Configuration.EnsureInitialized();

            var newApiDescriptions = new ApiDescriptionGroupCollection();
            var controllerSelector = Configuration.Services.GetHttpControllerSelector();
            var controllerMappings = controllerSelector.GetControllerMapping();

            if ( controllerMappings == null )
            {
                return newApiDescriptions;
            }

            var routes = FlattenRoutes( Configuration.Routes ).ToArray();

            foreach ( var apiVersion in FlattenApiVersions( controllerMappings ) )
            {
                for ( var i = 0; i < routes.Length; i++ )
                {
                    var route = routes[i];
                    var directRouteCandidates = route.GetDirectRouteCandidates();
                    var directRouteController = GetDirectRouteController( directRouteCandidates, apiVersion );
                    var apiDescriptionGroup = newApiDescriptions.GetOrAdd( apiVersion, GetGroupName );
                    var descriptionsFromRoute = ( directRouteController != null && directRouteCandidates != null ) ?
                            ExploreDirectRouteControllers( directRouteController, directRouteCandidates.Select( c => c.ActionDescriptor ).ToArray(), route, apiVersion ) :
                            ExploreRouteControllers( controllerMappings, route, apiVersion );

                    // Remove ApiDescription that will lead to ambiguous action matching.
                    // E.g. a controller with Post() and PostComment(). When the route template is {controller}, it produces POST /controller and POST /controller.
                    descriptionsFromRoute = RemoveInvalidApiDescriptions( descriptionsFromRoute, apiVersion );

                    for ( var j = 0; j < descriptionsFromRoute.Count; j++ )
                    {
                        // Do not add the description if the previous route has a matching description with the same HTTP method and relative path.
                        // E.g. having two routes with the templates "api/Values/{id}" and "api/{controller}/{id}" can potentially produce the same
                        // relative path "api/Values/{id}" but only the first one matters.
                        var description = descriptionsFromRoute[j];
                        var index = apiDescriptionGroup.ApiDescriptions.IndexOf( description, Comparer );

                        if ( index < 0 )
                        {
                            description.GroupName = apiDescriptionGroup.Name;
                            apiDescriptionGroup.ApiDescriptions.Add( description );
                        }
                        else
                        {
                            var overrideImplicitlyMappedApiDescription = description.ActionDescriptor.MappingTo( apiVersion ) == Explicit;

                            if ( overrideImplicitlyMappedApiDescription )
                            {
                                description.GroupName = apiDescriptionGroup.Name;
                                apiDescriptionGroup.ApiDescriptions[index] = description;
                            }
                        }
                    }

                    if ( apiDescriptionGroup.ApiDescriptions.Count == 0 )
                    {
                        newApiDescriptions.Remove( apiVersion );
                    }
                }
            }

            for ( var i = 0; i < newApiDescriptions.Count; i++ )
            {
                var apiDescriptionGroup = newApiDescriptions[i];

                SortApiDescriptionGroup( apiDescriptionGroup );

                if ( Options.SubstituteApiVersionInUrl )
                {
                    foreach ( var apiDescription in apiDescriptionGroup.ApiDescriptions )
                    {
                        apiDescription.TryUpdateRelativePathAndRemoveApiVersionParameter( Options );
                    }
                }
            }

            return newApiDescriptions;
        }

        /// <summary>
        /// Sorts the items in the API description group.
        /// </summary>
        /// <param name="apiDescriptionGroup">The <see cref="ApiDescriptionGroup">group of API descriptions</see> to sort.</param>
        /// <remarks>The default implementation sorts API descriptions by HTTP method, path, and API version.</remarks>
        protected virtual void SortApiDescriptionGroup( ApiDescriptionGroup apiDescriptionGroup )
        {
            if ( apiDescriptionGroup == null )
            {
                throw new ArgumentNullException( nameof( apiDescriptionGroup ) );
            }

            if ( apiDescriptionGroup.ApiDescriptions.Count < 2 )
            {
                return;
            }

            var items = apiDescriptionGroup.ApiDescriptions.ToArray();

            Array.Sort( items, Comparer );

            apiDescriptionGroup.ApiDescriptions.Clear();
            apiDescriptionGroup.ApiDescriptions.AddRange( items );
        }

        /// <summary>
        /// Attempts to expand the URI-based parameters for a given route and set of parameter descriptions.
        /// </summary>
        /// <param name="route">The <see cref="IHttpRoute">route</see> to expand.</param>
        /// <param name="parsedRoute">The <see cref="IParsedRoute">parsed route</see> information.</param>
        /// <param name="parameterDescriptions">The associated <see cref="ICollection{T}">collection</see> of <see cref="ApiParameterDescription">parameter descriptions</see>.</param>
        /// <param name="expandedRouteTemplate">The expanded route template, if any.</param>
        /// <returns>True if the operation succeeded; otherwise, false.</returns>
        protected virtual bool TryExpandUriParameters( IHttpRoute route, IParsedRoute parsedRoute, ICollection<ApiParameterDescription> parameterDescriptions, out string? expandedRouteTemplate )
        {
            if ( route == null )
            {
                throw new ArgumentNullException( nameof( route ) );
            }

            if ( parsedRoute == null )
            {
                throw new ArgumentNullException( nameof( parsedRoute ) );
            }

            if ( parameterDescriptions == null )
            {
                throw new ArgumentNullException( nameof( parameterDescriptions ) );
            }

            var parameterValuesForRoute = new Dictionary<string, object>( StringComparer.OrdinalIgnoreCase );
            var emitPrefixes = ShouldEmitPrefixes( parameterDescriptions );
            var prefix = Empty;

            foreach ( var parameterDescription in parameterDescriptions )
            {
                if ( parameterDescription.Source == FromUri )
                {
                    if ( parameterDescription.ParameterDescriptor == null )
                    {
                        // Undeclared route parameter handling generates query string like "?name={name}"
                        AddPlaceholder( parameterValuesForRoute, parameterDescription.Name );
                    }
                    else if ( parameterDescription.ParameterDescriptor.ParameterType.CanConvertFromString() )
                    {
                        // Simple type generates query string like "?name={name}"
                        AddPlaceholder( parameterValuesForRoute, parameterDescription.Name );
                    }
                    else if ( IsBindableCollection( parameterDescription.ParameterDescriptor.ParameterType ) )
                    {
                        var parameterName = parameterDescription.ParameterDescriptor.ParameterName;
                        var innerType = GetCollectionElementType( parameterDescription.ParameterDescriptor.ParameterType );
                        var innerTypeProperties = innerType.GetBindableProperties().ToArray();

                        if ( innerTypeProperties.Any() )
                        {
                            // Complex array and collection generate query string like
                            // "?name[0].foo={name[0].foo}&name[0].bar={name[0].bar}&name[1].foo={name[1].foo}&name[1].bar={name[1].bar}"
                            AddPlaceholderForProperties( parameterValuesForRoute, innerTypeProperties, parameterName + "[0]." );
                            AddPlaceholderForProperties( parameterValuesForRoute, innerTypeProperties, parameterName + "[1]." );
                        }
                        else
                        {
                            // Simple array and collection generate query string like "?name[0]={name[0]}&name[1]={name[1]}".
                            AddPlaceholder( parameterValuesForRoute, parameterName + "[0]" );
                            AddPlaceholder( parameterValuesForRoute, parameterName + "[1]" );
                        }
                    }
                    else if ( IsBindableKeyValuePair( parameterDescription.ParameterDescriptor.ParameterType ) )
                    {
                        // KeyValuePair generates query string like "?key={key}&value={value}"
                        AddPlaceholder( parameterValuesForRoute, "key" );
                        AddPlaceholder( parameterValuesForRoute, "value" );
                    }
                    else if ( IsBindableDictionry( parameterDescription.ParameterDescriptor.ParameterType ) )
                    {
                        // Dictionary generates query string like
                        // "?dict[0].key={dict[0].key}&dict[0].value={dict[0].value}&dict[1].key={dict[1].key}&dict[1].value={dict[1].value}"
                        var parameterName = parameterDescription.ParameterDescriptor.ParameterName;
                        AddPlaceholder( parameterValuesForRoute, parameterName + "[0].key" );
                        AddPlaceholder( parameterValuesForRoute, parameterName + "[0].value" );
                        AddPlaceholder( parameterValuesForRoute, parameterName + "[1].key" );
                        AddPlaceholder( parameterValuesForRoute, parameterName + "[1].value" );
                    }
                    else if ( parameterDescription.CanConvertPropertiesFromString() )
                    {
                        if ( emitPrefixes )
                        {
                            prefix = parameterDescription.Name + ".";
                        }

                        // Inserting the individual properties of the object in the query string as all the complex object can not be converted from string,
                        // but all its individual properties can.
                        AddPlaceholderForProperties( parameterValuesForRoute, parameterDescription.GetBindableProperties(), prefix );
                    }
                }
            }

            var boundRouteTemplate = parsedRoute.Bind( null, parameterValuesForRoute, new HttpRouteValueDictionary( route.Defaults ), new HttpRouteValueDictionary( route.Constraints ) );

            if ( boundRouteTemplate == null )
            {
                expandedRouteTemplate = null;
                return false;
            }

            expandedRouteTemplate = Uri.UnescapeDataString( boundRouteTemplate.BoundTemplate );
            return true;
        }

        static IEnumerable<IHttpRoute> FlattenRoutes( IEnumerable<IHttpRoute> routes )
        {
            foreach ( var route in routes )
            {
                if ( route is IEnumerable<IHttpRoute> nested )
                {
                    foreach ( var subRoute in FlattenRoutes( nested ) )
                    {
                        yield return subRoute;
                    }
                }
                else
                {
                    yield return route;
                }
            }
        }

        IEnumerable<ApiVersion> FlattenApiVersions( IDictionary<string, HttpControllerDescriptor> controllerMapping )
        {
            var services = Configuration.Services;
            var assembliesResolver = services.GetAssembliesResolver();
            var typeResolver = services.GetHttpControllerTypeResolver();
            var actionSelector = services.GetActionSelector();
            var controllerTypes = typeResolver.GetControllerTypes( assembliesResolver );
            var controllerDescriptors = controllerMapping.Values;
            var declared = new HashSet<ApiVersion>();
            var supported = new HashSet<ApiVersion>();
            var deprecated = new HashSet<ApiVersion>();
            var advertisedSupported = new HashSet<ApiVersion>();
            var advertisedDeprecated = new HashSet<ApiVersion>();

            foreach ( var controllerType in controllerTypes )
            {
                var controller = FindControllerDescriptor( controllerDescriptors, controllerType );

                if ( controller == null )
                {
                    continue;
                }

                var model = controller.GetApiVersionModel();
                var actions = actionSelector.GetActionMapping( controller ).SelectMany( g => g );

                for ( var i = 0; i < model.DeclaredApiVersions.Count; i++ )
                {
                    declared.Add( model.DeclaredApiVersions[i] );
                }

                foreach ( var action in actions )
                {
                    model = action.GetApiVersionModel();

                    for ( var i = 0; i < model.DeclaredApiVersions.Count; i++ )
                    {
                        declared.Add( model.DeclaredApiVersions[i] );
                    }

                    for ( var i = 0; i < model.SupportedApiVersions.Count; i++ )
                    {
                        var version = model.SupportedApiVersions[i];
                        supported.Add( version );
                        advertisedSupported.Add( version );
                    }

                    for ( var i = 0; i < model.DeprecatedApiVersions.Count; i++ )
                    {
                        var version = model.DeprecatedApiVersions[i];
                        deprecated.Add( version );
                        advertisedDeprecated.Add( version );
                    }
                }
            }

            advertisedSupported.ExceptWith( declared );
            advertisedDeprecated.ExceptWith( declared );
            supported.ExceptWith( advertisedSupported );
            deprecated.ExceptWith( supported.Concat( advertisedDeprecated ) );
            supported.UnionWith( deprecated );

            if ( supported.Count == 0 )
            {
                supported.Add( Configuration.GetApiVersioningOptions().DefaultApiVersion );
                return supported;
            }

            return supported.OrderBy( v => v );
        }

        static HttpControllerDescriptor? FindControllerDescriptor( IEnumerable<HttpControllerDescriptor> controllerDescriptors, Type controllerType )
        {
            foreach ( var controllerDescriptor in controllerDescriptors )
            {
                if ( controllerDescriptor is IEnumerable<HttpControllerDescriptor> groupedControllerDescriptors )
                {
                    foreach ( var groupedControllerDescriptor in groupedControllerDescriptors )
                    {
                        if ( controllerType.Equals( groupedControllerDescriptor.ControllerType ) )
                        {
                            return groupedControllerDescriptor;
                        }
                    }
                }
                else if ( controllerType.Equals( controllerDescriptor.ControllerType ) )
                {
                    return controllerDescriptor;
                }
            }

            return default;
        }

        static HttpControllerDescriptor? GetDirectRouteController( CandidateAction[]? candidates, ApiVersion apiVersion )
        {
            if ( candidates == null )
            {
                return default;
            }

            var bestMatch = default( HttpActionDescriptor );
            var bestMatches = new HashSet<HttpControllerDescriptor>();
            var implicitMatches = new HashSet<HttpControllerDescriptor>();

            for ( var i = 0; i < candidates.Length; i++ )
            {
                var action = candidates[i].ActionDescriptor;

                switch ( action.MappingTo( apiVersion ) )
                {
                    case Explicit:
                        bestMatch = action;
                        bestMatches.Add( action.ControllerDescriptor );
                        break;
                    case Implicit:
                        implicitMatches.Add( action.ControllerDescriptor );
                        break;
                }
            }

            switch ( bestMatches.Count )
            {
                case 0:
                    bestMatches.UnionWith( implicitMatches );
                    break;
                case 1:
                    if ( bestMatch!.GetApiVersionModel().IsApiVersionNeutral )
                    {
                        bestMatches.UnionWith( implicitMatches );
                    }

                    break;
            }

            return bestMatches.Count == 1 ? bestMatches.Single() : default;
        }

        /// <summary>
        /// Explores a controller that uses direct routes (aka "attribute" routing).
        /// </summary>
        /// <param name="controllerDescriptor">The <see cref="HttpControllerDescriptor">controller</see> to explore.</param>
        /// <param name="candidateActionDescriptors">The <see cref="IReadOnlyList{T}">read-only list</see> of candidate <see cref="HttpActionDescriptor">actions</see> to explore.</param>
        /// <param name="route">The <see cref="IHttpRoute">route</see> to explore.</param>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> to explore.</param>
        /// <returns>The <see cref="Collection{T}">collection</see> of discovered <see cref="VersionedApiDescription">API descriptions</see>.</returns>
        protected virtual Collection<VersionedApiDescription> ExploreDirectRouteControllers(
            HttpControllerDescriptor controllerDescriptor,
            IReadOnlyList<HttpActionDescriptor> candidateActionDescriptors,
            IHttpRoute route,
            ApiVersion apiVersion )
        {
            if ( controllerDescriptor == null )
            {
                throw new ArgumentNullException( nameof( controllerDescriptor ) );
            }

            if ( candidateActionDescriptors == null )
            {
                throw new ArgumentNullException( nameof( candidateActionDescriptors ) );
            }

            if ( route == null )
            {
                throw new ArgumentNullException( nameof( route ) );
            }

            var descriptions = new Collection<VersionedApiDescription>();

            if ( !ShouldExploreController( controllerDescriptor.ControllerName, controllerDescriptor, route, apiVersion ) )
            {
                return descriptions;
            }

            for ( var i = 0; i < candidateActionDescriptors.Count; i++ )
            {
                var actionDescriptor = candidateActionDescriptors[i];
                var actionName = actionDescriptor.ActionName;

                if ( !ShouldExploreAction( actionName, actionDescriptor, route, apiVersion ) )
                {
                    continue;
                }

                var routeTemplate = route.RouteTemplate;

                // expand {action} variable
                if ( actionVariableRegex.IsMatch( routeTemplate ) )
                {
                    routeTemplate = actionVariableRegex.Replace( routeTemplate, actionName );
                }

                PopulateActionDescriptions( actionDescriptor, route, routeTemplate, descriptions, apiVersion );
            }

            return descriptions;
        }

        /// <summary>
        /// Explores controllers that do not use direct routes (aka "attribute" routing).
        /// </summary>
        /// <param name="controllerMappings">The <see cref="IDictionary{TKey, TValue}">collection</see> of controller mappings.</param>
        /// <param name="route">The <see cref="IHttpRoute">route</see> to explore.</param>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> to explore.</param>
        /// <returns>The <see cref="Collection{T}">collection</see> of discovered <see cref="VersionedApiDescription">API descriptions</see>.</returns>
        protected virtual Collection<VersionedApiDescription> ExploreRouteControllers( IDictionary<string, HttpControllerDescriptor> controllerMappings, IHttpRoute route, ApiVersion apiVersion )
        {
            if ( controllerMappings == null )
            {
                throw new ArgumentNullException( nameof( controllerMappings ) );
            }

            if ( route == null )
            {
                throw new ArgumentNullException( nameof( route ) );
            }

            var apiDescriptions = new Collection<VersionedApiDescription>();
            var routeTemplate = route.RouteTemplate;
            string controllerVariableValue;

            if ( controllerVariableRegex.IsMatch( routeTemplate ) )
            {
                // unbound controller variable {controller}
                foreach ( var controllerMapping in controllerMappings )
                {
                    controllerVariableValue = controllerMapping.Key;

                    foreach ( var controllerDescriptor in controllerMapping.Value.AsEnumerable() )
                    {
                        if ( ShouldExploreController( controllerVariableValue, controllerDescriptor, route, apiVersion ) )
                        {
                            // expand {controller} variable
                            var expandedRouteTemplate = controllerVariableRegex.Replace( routeTemplate, controllerVariableValue );
                            ExploreRouteActions( route, expandedRouteTemplate, controllerDescriptor, apiDescriptions, apiVersion );
                        }
                    }
                }
            }
            else if ( route.Defaults.TryGetValue( RouteValueKeys.Controller, out controllerVariableValue ) )
            {
                // bound controller variable {controller = "controllerName"}
                if ( controllerMappings.TryGetValue( controllerVariableValue, out var controllerDescriptor ) )
                {
                    foreach ( var nestedControllerDescriptor in controllerDescriptor.AsEnumerable() )
                    {
                        if ( ShouldExploreController( controllerVariableValue, nestedControllerDescriptor, route, apiVersion ) )
                        {
                            ExploreRouteActions( route, routeTemplate, nestedControllerDescriptor, apiDescriptions, apiVersion );
                        }
                    }
                }
            }

            return apiDescriptions;
        }

        /// <summary>
        /// Populates the API version parameters for the specified API description.
        /// </summary>
        /// <param name="apiDescription">The <see cref="ApiDescription">API description</see> to populate parameters for.</param>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> used to populate parameters with.</param>
        protected virtual void PopulateApiVersionParameters( ApiDescription apiDescription, ApiVersion apiVersion )
        {
            var parameterSource = Options.ApiVersionParameterSource;
            var context = new ApiVersionParameterDescriptionContext( apiDescription, apiVersion, Options );

            parameterSource.AddParameters( context );
        }

        void ExploreRouteActions(
            IHttpRoute route,
            string localPath,
            HttpControllerDescriptor controllerDescriptor,
            Collection<VersionedApiDescription> apiDescriptions,
            ApiVersion apiVersion )
        {
            if ( controllerDescriptor.IsAttributeRouted() )
            {
                return;
            }

            var controllerServices = controllerDescriptor.Configuration.Services;
            var actionMappings = controllerServices.GetActionSelector().GetActionMapping( controllerDescriptor );

            if ( actionMappings == null )
            {
                return;
            }

            string actionVariableValue;

            if ( actionVariableRegex.IsMatch( localPath ) )
            {
                // unbound action variable, {action}
                foreach ( var actionMapping in actionMappings )
                {
                    // expand {action} variable
                    actionVariableValue = actionMapping.Key;
                    var expandedLocalPath = actionVariableRegex.Replace( localPath, actionVariableValue );
                    PopulateActionDescriptions( actionMapping, actionVariableValue, route, expandedLocalPath, apiDescriptions, apiVersion );
                }
            }
            else if ( route.Defaults.TryGetValue( RouteValueKeys.Action, out actionVariableValue ) )
            {
                // bound action variable, { action = "actionName" }
                PopulateActionDescriptions( actionMappings[actionVariableValue], actionVariableValue, route, localPath, apiDescriptions, apiVersion );
            }
            else
            {
                // no {action} specified, e.g. {controller}/{id}
                foreach ( var actionMapping in actionMappings )
                {
                    PopulateActionDescriptions( actionMapping, null, route, localPath, apiDescriptions, apiVersion );
                }
            }
        }

        void PopulateActionDescriptions(
            IEnumerable<HttpActionDescriptor> actionDescriptors,
            string? actionVariableValue,
            IHttpRoute route,
            string localPath,
            Collection<VersionedApiDescription> apiDescriptions,
            ApiVersion apiVersion )
        {
            foreach ( var actionDescriptor in actionDescriptors )
            {
                if ( ShouldExploreAction( actionVariableValue ?? string.Empty, actionDescriptor, route, apiVersion ) && !actionDescriptor.IsAttributeRouted() )
                {
                    PopulateActionDescriptions( actionDescriptor, route, localPath, apiDescriptions, apiVersion );
                }
            }
        }

        void PopulateActionDescriptions(
            HttpActionDescriptor actionDescriptor,
            IHttpRoute route,
            string localPath,
            Collection<VersionedApiDescription> apiDescriptions,
            ApiVersion apiVersion )
        {
            var parsedRoute = RouteParser.Parse( localPath );
            var parameterDescriptions = CreateParameterDescriptions( actionDescriptor, parsedRoute, route.Defaults );

            if ( !TryExpandUriParameters( route, parsedRoute, parameterDescriptions, out var finalPath ) )
            {
                return;
            }

            var documentation = DocumentationProvider?.GetDocumentation( actionDescriptor );
            var bodyParameter = parameterDescriptions.FirstOrDefault( description => description.Source == FromBody );
            var formatters = actionDescriptor.Configuration.Formatters;
            var supportedRequestBodyFormatters =
                bodyParameter != null ?
                formatters.Where( f => f.CanReadType( bodyParameter.ParameterDescriptor.ParameterType ) ) :
                Enumerable.Empty<MediaTypeFormatter>();

            var responseDescription = CreateResponseDescription( actionDescriptor );
            var returnType = responseDescription.ResponseType ?? responseDescription.DeclaredType;
            var supportedResponseFormatters =
                ( returnType != null && returnType != typeof( void ) ) ?
                formatters.Where( f => f.CanWriteType( returnType ) ) :
                Enumerable.Empty<MediaTypeFormatter>();

            supportedRequestBodyFormatters = GetInnerFormatters( supportedRequestBodyFormatters );
            supportedResponseFormatters = GetInnerFormatters( supportedResponseFormatters );

            var supportedMethods = GetHttpMethodsSupportedByAction( route, actionDescriptor );
            var model = actionDescriptor.GetApiVersionModel();
            var deprecated = !model.IsApiVersionNeutral && model.DeprecatedApiVersions.Contains( apiVersion );

            for ( var i = 0; i < supportedMethods.Count; i++ )
            {
                var apiDescription = new VersionedApiDescription()
                {
                    Documentation = documentation,
                    HttpMethod = supportedMethods[i],
                    RelativePath = finalPath,
                    ActionDescriptor = actionDescriptor,
                    Route = route,
                    ResponseDescription = responseDescription,
                    ApiVersion = apiVersion,
                    IsDeprecated = deprecated,
                };

                apiDescription.SupportedResponseFormatters.AddRange( supportedResponseFormatters );
                apiDescription.SupportedRequestBodyFormatters.AddRange( supportedRequestBodyFormatters );
                apiDescription.ParameterDescriptions.AddRange( parameterDescriptions );
                PopulateApiVersionParameters( apiDescription, apiVersion );
                apiDescriptions.Add( apiDescription );
            }
        }

        /// <summary>
        /// Creates a description for the response of the action.
        /// </summary>
        /// <param name="actionDescriptor">The <see cref="HttpActionDescriptor">action</see> to create a response description for.</param>
        /// <returns>A new <see cref="ResponseDescription">response description</see>.</returns>
        protected virtual ResponseDescription CreateResponseDescription( HttpActionDescriptor actionDescriptor )
        {
            if ( actionDescriptor == null )
            {
                throw new ArgumentNullException( nameof( actionDescriptor ) );
            }

            var responseType = actionDescriptor.GetCustomAttributes<ResponseTypeAttribute>().FirstOrDefault()?.ResponseType;

            return new ResponseDescription()
            {
                DeclaredType = actionDescriptor.ReturnType,
                ResponseType = responseType,
                Documentation = DocumentationProvider?.GetResponseDocumentation( actionDescriptor ),
            };
        }

        static IEnumerable<MediaTypeFormatter> GetInnerFormatters( IEnumerable<MediaTypeFormatter> mediaTypeFormatters ) => mediaTypeFormatters.Select( Decorator.GetInner );

        static bool ShouldEmitPrefixes( ICollection<ApiParameterDescription> parameterDescriptions )
        {
            // Determine if there are two or more complex objects from the Uri so TryExpandUriParameters needs to emit prefixes.
            return parameterDescriptions.Count( parameter =>
                         parameter.Source == FromUri &&
                         parameter.ParameterDescriptor != null &&
                         !parameter.ParameterDescriptor.ParameterType.CanConvertFromString() &&
                         parameter.CanConvertPropertiesFromString() ) > 1;
        }

        static Type GetCollectionElementType( Type collectionType )
        {
            var elementType = collectionType.GetElementType();

            if ( elementType == null )
            {
                elementType = typeof( ICollection<> ).GetGenericBinderTypeArgs( collectionType ).First();
            }

            return elementType;
        }

        static void AddPlaceholderForProperties( Dictionary<string, object> parameterValuesForRoute, IEnumerable<PropertyInfo> properties, string prefix )
        {
            foreach ( var property in properties )
            {
                var queryParameterName = prefix + property.Name;
                AddPlaceholder( parameterValuesForRoute, queryParameterName );
            }
        }

        static bool IsBindableCollection( Type type ) => type.IsArray || new CollectionModelBinderProvider().GetBinder( null, type ) != null;

        static bool IsBindableDictionry( Type type ) => new DictionaryModelBinderProvider().GetBinder( null, type ) != null;

        static bool IsBindableKeyValuePair( Type type ) => type.GetTypeArgumentsIfMatch( typeof( KeyValuePair<,> ) ) != null;

        static void AddPlaceholder( IDictionary<string, object> parameterValuesForRoute, string queryParameterName )
        {
            if ( !parameterValuesForRoute.ContainsKey( queryParameterName ) )
            {
                parameterValuesForRoute.Add( queryParameterName, $"{{{queryParameterName}}}" );
            }
        }

        IList<ApiParameterDescription> CreateParameterDescriptions( HttpActionDescriptor actionDescriptor, IParsedRoute parsedRoute, IDictionary<string, object> routeDefaults )
        {
            IList<ApiParameterDescription> parameterDescriptions = new List<ApiParameterDescription>();
            var actionBinding = GetActionBinding( actionDescriptor );

            // try get parameter binding information if available
            if ( actionBinding != null )
            {
                var parameterBindings = actionBinding.ParameterBindings;

                if ( parameterBindings != null )
                {
                    for ( var i = 0; i < parameterBindings.Length; i++ )
                    {
                        parameterDescriptions.Add( CreateParameterDescriptionFromBinding( parameterBindings[i] ) );
                    }
                }
            }
            else
            {
                var parameters = actionDescriptor.GetParameters();

                if ( parameters != null )
                {
                    for ( var i = 0; i < parameters.Count; i++ )
                    {
                        parameterDescriptions.Add( CreateParameterDescription( parameters[i] ) );
                    }
                }
            }

            // Adding route parameters not declared on the action. We're doing this because route parameters may or
            // may not be part of the action parameters and we want to have them in the description.
            AddUndeclaredRouteParameters( parsedRoute, routeDefaults, parameterDescriptions );

            return parameterDescriptions;
        }

        static void AddUndeclaredRouteParameters( IParsedRoute parsedRoute, IDictionary<string, object> routeDefaults, IList<ApiParameterDescription> parameterDescriptions )
        {
            for ( var i = 0; i < parsedRoute.PathSegments.Count; i++ )
            {
                if ( !( parsedRoute.PathSegments[i] is IPathContentSegment content ) )
                {
                    continue;
                }

                for ( var j = 0; j < content.Subsegments.Count; j++ )
                {
                    if ( content.Subsegments[j] is IPathParameterSubsegment parameter )
                    {
                        var parameterName = parameter.ParameterName;

                        if ( !parameterDescriptions.Any( p => string.Equals( p.Name, parameterName, StringComparison.OrdinalIgnoreCase ) ) &&
                           ( !routeDefaults.TryGetValue( parameterName, out var parameterValue ) ||
                            parameterValue != RouteParameter.Optional ) )
                        {
                            parameterDescriptions.Add( new ApiParameterDescription() { Name = parameterName, Source = FromUri } );
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Creates a parameter description from the specified descriptor.
        /// </summary>
        /// <param name="parameterDescriptor">The <see cref="HttpParameterDescriptor">parameter descriptor</see> to create a description from.</param>
        /// <returns>A new <see cref="ApiParameterDescription">parameter description</see>.</returns>
        protected virtual ApiParameterDescription CreateParameterDescription( HttpParameterDescriptor parameterDescriptor )
        {
            if ( parameterDescriptor == null )
            {
                throw new ArgumentNullException( nameof( parameterDescriptor ) );
            }

            var name = parameterDescriptor.Prefix;

            if ( IsNullOrEmpty( name ) )
            {
                name = parameterDescriptor.ParameterName;
            }

            return new ApiParameterDescription()
            {
                ParameterDescriptor = parameterDescriptor,
                Name = name,
                Documentation = DocumentationProvider?.GetDocumentation( parameterDescriptor ),
                Source = Unknown,
            };
        }

        ApiParameterDescription CreateParameterDescriptionFromBinding( HttpParameterBinding parameterBinding )
        {
            var parameterDescription = CreateParameterDescription( parameterBinding.Descriptor );

            if ( parameterBinding.WillReadBody )
            {
                parameterDescription.Source = FromBody;
            }
            else if ( parameterBinding.WillReadUri() )
            {
                parameterDescription.Source = FromUri;
            }

            return parameterDescription;
        }

        static Collection<VersionedApiDescription> RemoveInvalidApiDescriptions( Collection<VersionedApiDescription> apiDescriptions, ApiVersion apiVersion )
        {
            var filteredDescriptions = new Dictionary<string, VersionedApiDescription>( StringComparer.OrdinalIgnoreCase );

            for ( var i = 0; i < apiDescriptions.Count; i++ )
            {
                var description = apiDescriptions[i];
                var apiDescriptionId = description.GetUniqueID();

                if ( filteredDescriptions.ContainsKey( apiDescriptionId ) )
                {
                    if ( description.ActionDescriptor.MappingTo( apiVersion ) == Explicit )
                    {
                        filteredDescriptions[apiDescriptionId] = description;
                    }
                }
                else
                {
                    filteredDescriptions.Add( apiDescriptionId, description );
                }
            }

            return new Collection<VersionedApiDescription>( filteredDescriptions.Values.ToList() );
        }

        static bool MatchRegexConstraint( IHttpRoute route, string parameterName, string parameterValue )
        {
            var constraints = route.Constraints;

            if ( constraints == null )
            {
                return true;
            }

            if ( !constraints.TryGetValue( parameterName, out var constraint ) )
            {
                return true;
            }

            // note that we don't support custom constraint (IHttpRouteConstraint) because it might rely on the request and some runtime states
            if ( !( constraint is string constraintsRule ) )
            {
                return true;
            }

            if ( parameterValue == null )
            {
                return false;
            }

            return Regex.IsMatch( parameterValue, $"^({constraintsRule})$", CultureInvariant | IgnoreCase );
        }

        static HttpActionBinding? GetActionBinding( HttpActionDescriptor actionDescriptor )
        {
            var controllerDescriptor = actionDescriptor.ControllerDescriptor;

            if ( controllerDescriptor == null )
            {
                return null;
            }

            var controllerServices = controllerDescriptor.Configuration.Services;
            var actionValueBinder = controllerServices.GetActionValueBinder();
            var actionBinding = actionValueBinder?.GetBinding( actionDescriptor );

            return actionBinding;
        }
    }
}