namespace Microsoft.Web.Http.Description
{
    using Routing;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Formatting;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web.Http;
    using System.Web.Http.Controllers;
    using System.Web.Http.Description;
    using System.Web.Http.ModelBinding.Binders;
    using System.Web.Http.Routing;
    using System.Web.Http.Services;
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
        IDocumentationProvider documentationProvider;

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
            Arg.NotNull( configuration, nameof( configuration ) );
            Arg.NotNull( options, nameof( options ) );

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
        /// Get a collection of descriptions grouped by API version.
        /// </summary>
        /// <value>An <see cref="ApiDescriptionGroupCollection">API description group collection</see>.</value>
        public virtual ApiDescriptionGroupCollection ApiDescriptions => apiDescriptions.Value;

        /// <summary>
        /// Gets or sets the documentation provider. The provider will be responsible for documenting the API.
        /// </summary>
        /// <value>The <see cref="IDocumentationProvider">documentation provider</see> used to document APIs.</value>
        public IDocumentationProvider DocumentationProvider
        {
            get => documentationProvider ?? ( documentationProvider = Configuration.Services.GetDocumentationProvider() );
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
            Arg.NotNull( route, nameof( route ) );
            Arg.NotNull( actionDescriptor, nameof( actionDescriptor ) );

            IList<HttpMethod> supportedMethods = new List<HttpMethod>();
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
            Arg.NotNull( actionDescriptor, nameof( actionDescriptor ) );
            Arg.NotNull( route, nameof( route ) );
            Arg.NotNull( apiVersion, nameof( apiVersion ) );

            var setting = actionDescriptor.GetCustomAttributes<ApiExplorerSettingsAttribute>().FirstOrDefault();

            if ( ( setting == null || !setting.IgnoreApi ) && MatchRegexConstraint( route, RouteValueKeys.Action, actionRouteParameterValue ) )
            {
                var model = actionDescriptor.GetApiVersionModel();

                if ( model.IsApiVersionNeutral || model.DeclaredApiVersions.Contains( apiVersion ) )
                {
                    return true;
                }

                if ( model.DeclaredApiVersions.Count == 0 )
                {
                    model = actionDescriptor.ControllerDescriptor.GetApiVersionModel();
                    return model.IsApiVersionNeutral || model.DeclaredApiVersions.Contains( apiVersion );
                }
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
            Arg.NotNull( controllerDescriptor, nameof( controllerDescriptor ) );
            Arg.NotNull( route, nameof( route ) );
            Arg.NotNull( apiVersion, nameof( apiVersion ) );

            var setting = controllerDescriptor.GetCustomAttributes<ApiExplorerSettingsAttribute>().FirstOrDefault();

            if ( ( setting == null || !setting.IgnoreApi ) && MatchRegexConstraint( route, RouteValueKeys.Controller, controllerRouteParameterValue ) )
            {
                var model = controllerDescriptor.GetApiVersionModel();
                return model.IsApiVersionNeutral || model.DeclaredApiVersions.Contains( apiVersion );
            }

            return false;
        }

        /// <summary>
        /// Returns the group name for the specified API version.
        /// </summary>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> to retrieve a group name for.</param>
        /// <returns>The group name for the specified <paramref name="apiVersion">API version</paramref>.</returns>
        protected virtual string GetGroupName( ApiVersion apiVersion )
        {
            Arg.NotNull( apiVersion, nameof( apiVersion ) );
            Contract.Ensures( !IsNullOrEmpty( Contract.Result<string>() ) );

            return apiVersion.ToString( Options.GroupNameFormat, InvariantCulture );
        }

        /// <summary>
        /// Initializes the API descriptions to explore.
        /// </summary>
        /// <returns>A new <see cref="ApiDescriptionGroupCollection">collection</see> of
        /// <see cref="ApiDescriptionGroup">API description groups</see>.</returns>
        protected virtual ApiDescriptionGroupCollection InitializeApiDescriptions()
        {
            Contract.Ensures( Contract.Result<ApiDescriptionGroupCollection>() != null );

            Configuration.EnsureInitialized();

            var newApiDescriptions = new ApiDescriptionGroupCollection();
            var controllerSelector = Configuration.Services.GetHttpControllerSelector();
            var controllerMappings = controllerSelector.GetControllerMapping();

            if ( controllerMappings == null )
            {
                return newApiDescriptions;
            }

            var routes = FlattenRoutes( Configuration.Routes ).ToArray();

            foreach ( var apiVersion in FlattenApiVersions() )
            {
                foreach ( var route in routes )
                {
                    var directRouteCandidates = route.GetDirectRouteCandidates();
                    var directRouteController = GetDirectRouteController( directRouteCandidates, apiVersion );
                    var apiDescriptionGroup = newApiDescriptions.GetOrAdd( apiVersion, GetGroupName );
                    var descriptionsFromRoute = ( directRouteController != null && directRouteCandidates != null ) ?
                            ExploreDirectRouteControllers( directRouteController, directRouteCandidates.Select( c => c.ActionDescriptor ).ToArray(), route, apiVersion ) :
                            ExploreRouteControllers( controllerMappings, route, apiVersion );

                    // Remove ApiDescription that will lead to ambiguous action matching.
                    // E.g. a controller with Post() and PostComment(). When the route template is {controller}, it produces POST /controller and POST /controller.
                    descriptionsFromRoute = RemoveInvalidApiDescriptions( descriptionsFromRoute, apiVersion );

                    foreach ( var description in descriptionsFromRoute )
                    {
                        // Do not add the description if the previous route has a matching description with the same HTTP method and relative path.
                        // E.g. having two routes with the templates "api/Values/{id}" and "api/{controller}/{id}" can potentially produce the same
                        // relative path "api/Values/{id}" but only the first one matters.

                        var index = apiDescriptionGroup.ApiDescriptions.IndexOf( description, Comparer );

                        if ( index < 0 )
                        {
                            description.GroupName = apiDescriptionGroup.Name;
                            apiDescriptionGroup.ApiDescriptions.Add( description );
                        }
                        else
                        {
                            var model = description.ActionDescriptor.GetApiVersionModel();
                            var overrideImplicitlyMappedApiDescription = model.DeclaredApiVersions.Contains( apiVersion );

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

            foreach ( var apiDescriptionGroup in newApiDescriptions )
            {
                SortApiDescriptionGroup( apiDescriptionGroup );
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
            Arg.NotNull( apiDescriptionGroup, nameof( apiDescriptionGroup ) );

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
        protected virtual bool TryExpandUriParameters( IHttpRoute route, IParsedRoute parsedRoute, ICollection<ApiParameterDescription> parameterDescriptions, out string expandedRouteTemplate )
        {
            Arg.NotNull( route, nameof( route ) );
            Arg.NotNull( parsedRoute, nameof( parsedRoute ) );
            Arg.NotNull( parameterDescriptions, nameof( parameterDescriptions ) );

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
                    else if ( TypeHelper.CanConvertFromString( parameterDescription.ParameterDescriptor.ParameterType ) )
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
                            AddPlaceholderForProperties( parameterValuesForRoute,
                                                        innerTypeProperties,
                                                        parameterName + "[0]." );
                            AddPlaceholderForProperties( parameterValuesForRoute,
                                                        innerTypeProperties,
                                                        parameterName + "[1]." );
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
            Contract.Requires( routes != null );
            Contract.Ensures( Contract.Result<IEnumerable<IHttpRoute>>() != null );

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

        IEnumerable<ApiVersion> FlattenApiVersions()
        {
            Contract.Ensures( Contract.Result<IEnumerable<ApiVersion>>() != null );

            var services = Configuration.Services;
            var assembliesResolver = services.GetAssembliesResolver();
            var typeResolver = services.GetHttpControllerTypeResolver();
            var controllerTypes = typeResolver.GetControllerTypes( assembliesResolver );
            var options = Configuration.GetApiVersioningOptions();
            var declared = new HashSet<ApiVersion>();
            var supported = new HashSet<ApiVersion>();
            var deprecated = new HashSet<ApiVersion>();
            var advertisedSupported = new HashSet<ApiVersion>();
            var advertisedDeprecated = new HashSet<ApiVersion>();

            foreach ( var controllerType in controllerTypes )
            {
                var descriptor = new HttpControllerDescriptor( Configuration, Empty, controllerType );

                options.Conventions.ApplyTo( descriptor );

                var model = descriptor.GetApiVersionModel();

                foreach ( var version in model.DeclaredApiVersions )
                {
                    declared.Add( version );
                }

                foreach ( var version in model.SupportedApiVersions )
                {
                    supported.Add( version );
                    advertisedSupported.Add( version );
                }

                foreach ( var version in model.DeprecatedApiVersions )
                {
                    deprecated.Add( version );
                    advertisedDeprecated.Add( version );
                }
            }

            advertisedSupported.ExceptWith( declared );
            advertisedDeprecated.ExceptWith( declared );
            supported.ExceptWith( advertisedSupported );
            deprecated.ExceptWith( supported.Concat( advertisedDeprecated ) );
            supported.UnionWith( deprecated );

            if ( supported.Count == 0 )
            {
                supported.Add( options.DefaultApiVersion );
                return supported;
            }

            return supported.OrderBy( v => v );
        }

        static HttpControllerDescriptor GetDirectRouteController( CandidateAction[] directRouteCandidates, ApiVersion apiVersion )
        {
            Contract.Requires( apiVersion != null );

            if ( directRouteCandidates == null )
            {
                return null;
            }

            var controllerDescriptor = directRouteCandidates[0].ActionDescriptor.ControllerDescriptor;

            if ( directRouteCandidates.Length == 1 )
            {
                var model = controllerDescriptor.GetApiVersionModel();

                if ( !model.IsApiVersionNeutral && !model.DeclaredApiVersions.Contains( apiVersion ) )
                {
                    return null;
                }
            }
            else
            {
                var matches = from candidate in directRouteCandidates
                              let controller = candidate.ActionDescriptor.ControllerDescriptor
                              let model = controller.GetApiVersionModel()
                              where model.IsApiVersionNeutral || model.DeclaredApiVersions.Contains( apiVersion )
                              select controller;

                using ( var iterator = matches.GetEnumerator() )
                {
                    if ( !iterator.MoveNext() )
                    {
                        return null;
                    }

                    controllerDescriptor = iterator.Current;

                    while ( iterator.MoveNext() )
                    {
                        if ( iterator.Current != controllerDescriptor )
                        {
                            return null;
                        }
                    }
                }
            }

            return controllerDescriptor;
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
            Arg.NotNull( controllerDescriptor, nameof( controllerDescriptor ) );
            Arg.NotNull( candidateActionDescriptors, nameof( candidateActionDescriptors ) );
            Arg.NotNull( route, nameof( route ) );
            Arg.NotNull( apiVersion, nameof( apiVersion ) );
            Contract.Ensures( Contract.Result<Collection<VersionedApiDescription>>() != null );

            var descriptions = new Collection<VersionedApiDescription>();

            if ( !ShouldExploreController( controllerDescriptor.ControllerName, controllerDescriptor, route, apiVersion ) )
            {
                return descriptions;
            }

            foreach ( var actionDescriptor in candidateActionDescriptors )
            {
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
        /// Explores controllers that do not use direct routes (aka "attribute" routing)
        /// </summary>
        /// <param name="controllerMappings">The <see cref="IDictionary{TKey, TValue}">collection</see> of controller mappings.</param>
        /// <param name="route">The <see cref="IHttpRoute">route</see> to explore.</param>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> to explore.</param>
        /// <returns>The <see cref="Collection{T}">collection</see> of discovered <see cref="VersionedApiDescription">API descriptions</see>.</returns>
        protected virtual Collection<VersionedApiDescription> ExploreRouteControllers( IDictionary<string, HttpControllerDescriptor> controllerMappings, IHttpRoute route, ApiVersion apiVersion )
        {
            Arg.NotNull( controllerMappings, nameof( controllerMappings ) );
            Arg.NotNull( route, nameof( route ) );
            Arg.NotNull( apiVersion, nameof( apiVersion ) );
            Contract.Ensures( Contract.Result<Collection<VersionedApiDescription>>() != null );

            var apiDescriptions = new Collection<VersionedApiDescription>();
            var routeTemplate = route.RouteTemplate;
            var controllerVariableValue = default( string );

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
            Arg.NotNull( apiDescription, nameof( apiDescription ) );
            Arg.NotNull( apiVersion, nameof( apiVersion ) );

            var parameterSource = Options.ApiVersionParameterSource;
            var context = new ApiVersionParameterDescriptionContext( apiDescription, apiVersion, Options );

            parameterSource.AddParmeters( context );
        }

        void ExploreRouteActions(
            IHttpRoute route,
            string localPath,
            HttpControllerDescriptor controllerDescriptor,
            Collection<VersionedApiDescription> apiDescriptions,
            ApiVersion apiVersion )
        {
            Contract.Requires( route != null );
            Contract.Requires( localPath != null );
            Contract.Requires( controllerDescriptor != null );
            Contract.Requires( apiDescriptions != null );
            Contract.Requires( apiVersion != null );

            if ( controllerDescriptor.IsAttributeRouted() )
            {
                return;
            }

            var controllerServices = controllerDescriptor.Configuration.Services;
            var actionMappings = controllerServices.GetActionSelector().GetActionMapping( controllerDescriptor );
            var actionVariableValue = default( string );

            if ( actionMappings == null )
            {
                return;
            }

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
            string actionVariableValue,
            IHttpRoute route,
            string localPath,
            Collection<VersionedApiDescription> apiDescriptions,
            ApiVersion apiVersion )
        {
            Contract.Requires( actionDescriptors != null );
            Contract.Requires( route != null );
            Contract.Requires( apiDescriptions != null );
            Contract.Requires( apiVersion != null );

            foreach ( var actionDescriptor in actionDescriptors )
            {
                if ( ShouldExploreAction( actionVariableValue, actionDescriptor, route, apiVersion ) && !actionDescriptor.IsAttributeRouted() )
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
            Contract.Requires( actionDescriptor != null );
            Contract.Requires( route != null );
            Contract.Requires( localPath != null );
            Contract.Requires( apiDescriptions != null );
            Contract.Requires( apiVersion != null );

            var parsedRoute = RouteParser.Parse( localPath );
            var parameterDescriptions = CreateParameterDescriptions( actionDescriptor, parsedRoute, route.Defaults );

            if ( !TryExpandUriParameters( route, parsedRoute, parameterDescriptions, out var finalPath ) )
            {
                return;
            }

            var documentation = DocumentationProvider?.GetDocumentation( actionDescriptor );
            var bodyParameter = parameterDescriptions.FirstOrDefault( description => description.Source == FromBody );
            var supportedRequestBodyFormatters =
                bodyParameter != null ?
                Configuration.Formatters.Where( f => f.CanReadType( bodyParameter.ParameterDescriptor.ParameterType ) ) :
                Enumerable.Empty<MediaTypeFormatter>();

            var responseDescription = CreateResponseDescription( actionDescriptor );
            var returnType = responseDescription.ResponseType ?? responseDescription.DeclaredType;
            var supportedResponseFormatters =
                ( returnType != null && returnType != typeof( void ) ) ?
                Configuration.Formatters.Where( f => f.CanWriteType( returnType ) ) :
                Enumerable.Empty<MediaTypeFormatter>();

            supportedRequestBodyFormatters = GetInnerFormatters( supportedRequestBodyFormatters );
            supportedResponseFormatters = GetInnerFormatters( supportedResponseFormatters );

            var supportedMethods = GetHttpMethodsSupportedByAction( route, actionDescriptor );
            var deprecated = actionDescriptor.ControllerDescriptor.GetApiVersionModel().DeprecatedApiVersions.Contains( apiVersion );

            foreach ( var method in supportedMethods )
            {
                var apiDescription = new VersionedApiDescription()
                {
                    Documentation = documentation,
                    HttpMethod = method,
                    RelativePath = finalPath,
                    ActionDescriptor = actionDescriptor,
                    Route = route,
                    ResponseDescription = responseDescription,
                    ApiVersion = apiVersion,
                    IsDeprecated = deprecated
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
            Arg.NotNull( actionDescriptor, nameof( actionDescriptor ) );
            Contract.Ensures( Contract.Result<ResponseDescription>() != null );

            var responseType = actionDescriptor.GetCustomAttributes<ResponseTypeAttribute>().FirstOrDefault()?.ResponseType;

            return new ResponseDescription()
            {
                DeclaredType = actionDescriptor.ReturnType,
                ResponseType = responseType,
                Documentation = DocumentationProvider?.GetResponseDocumentation( actionDescriptor )
            };
        }

        static IEnumerable<MediaTypeFormatter> GetInnerFormatters( IEnumerable<MediaTypeFormatter> mediaTypeFormatters ) => mediaTypeFormatters.Select( Decorator.GetInner );

        static bool ShouldEmitPrefixes( ICollection<ApiParameterDescription> parameterDescriptions )
        {
            Contract.Requires( parameterDescriptions != null );

            // Determine if there are two or more complex objects from the Uri so TryExpandUriParameters needs to emit prefixes.
            return parameterDescriptions.Count( parameter =>
                         parameter.Source == FromUri &&
                         parameter.ParameterDescriptor != null &&
                         !TypeHelper.CanConvertFromString( parameter.ParameterDescriptor.ParameterType ) &&
                         parameter.CanConvertPropertiesFromString() ) > 1;
        }

        static Type GetCollectionElementType( Type collectionType )
        {
            Contract.Requires( collectionType != null );
            Contract.Assert( !typeof( IDictionary ).IsAssignableFrom( collectionType ) );
            Contract.Ensures( Contract.Result<Type>() != null );

            var elementType = collectionType.GetElementType();

            if ( elementType == null )
            {
                elementType = typeof( ICollection<> ).GetGenericBinderTypeArgs( collectionType ).First();
            }

            return elementType;
        }

        static void AddPlaceholderForProperties( Dictionary<string, object> parameterValuesForRoute, IEnumerable<PropertyInfo> properties, string prefix )
        {
            Contract.Requires( parameterValuesForRoute != null );
            Contract.Requires( properties != null );

            foreach ( var property in properties )
            {
                var queryParameterName = prefix + property.Name;
                AddPlaceholder( parameterValuesForRoute, queryParameterName );
            }
        }

        static bool IsBindableCollection( Type type ) => type.IsArray || new CollectionModelBinderProvider().GetBinder( null, type ) != null;

        static bool IsBindableDictionry( Type type ) => new DictionaryModelBinderProvider().GetBinder( null, type ) != null;

        static bool IsBindableKeyValuePair( Type type ) => TypeHelper.GetTypeArgumentsIfMatch( type, typeof( KeyValuePair<,> ) ) != null;

        static void AddPlaceholder( IDictionary<string, object> parameterValuesForRoute, string queryParameterName )
        {
            Contract.Requires( parameterValuesForRoute != null );

            if ( !parameterValuesForRoute.ContainsKey( queryParameterName ) )
            {
                parameterValuesForRoute.Add( queryParameterName, $"{{{queryParameterName}}}" );
            }
        }

        IList<ApiParameterDescription> CreateParameterDescriptions( HttpActionDescriptor actionDescriptor, IParsedRoute parsedRoute, IDictionary<string, object> routeDefaults )
        {
            Contract.Requires( actionDescriptor != null );
            Contract.Requires( parsedRoute != null );
            Contract.Requires( routeDefaults != null );
            Contract.Ensures( Contract.Result<IList<ApiParameterDescription>>() != null );

            IList<ApiParameterDescription> parameterDescriptions = new List<ApiParameterDescription>();
            var actionBinding = GetActionBinding( actionDescriptor );

            // try get parameter binding information if available
            if ( actionBinding != null )
            {
                var parameterBindings = actionBinding.ParameterBindings;

                if ( parameterBindings != null )
                {
                    foreach ( var parameter in parameterBindings )
                    {
                        parameterDescriptions.Add( CreateParameterDescriptionFromBinding( parameter ) );
                    }
                }
            }
            else
            {
                var parameters = actionDescriptor.GetParameters();

                if ( parameters != null )
                {
                    foreach ( var parameter in parameters )
                    {
                        parameterDescriptions.Add( CreateParameterDescription( parameter ) );
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
            Contract.Requires( parsedRoute != null );
            Contract.Requires( routeDefaults != null );
            Contract.Requires( parameterDescriptions != null );

            foreach ( var content in parsedRoute.PathSegments.OfType<IPathContentSegment>() )
            {
                foreach ( var subSegment in content.Subsegments )
                {
                    if ( subSegment is IPathParameterSubsegment parameter )
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
        /// Creates a parameter description from the speicfied descriptor.
        /// </summary>
        /// <param name="parameterDescriptor">The <see cref="HttpParameterDescriptor">parameter descriptor</see> to create a description from.</param>
        /// <returns>A new <see cref="ApiParameterDescription">parameter description</see>.</returns>
        protected virtual ApiParameterDescription CreateParameterDescription( HttpParameterDescriptor parameterDescriptor )
        {
            Arg.NotNull( parameterDescriptor, nameof( parameterDescriptor ) );
            Contract.Ensures( Contract.Result<ApiParameterDescription>() != null );

            return new ApiParameterDescription()
            {
                ParameterDescriptor = parameterDescriptor,
                Name = parameterDescriptor.Prefix ?? parameterDescriptor.ParameterName,
                Documentation = DocumentationProvider?.GetDocumentation( parameterDescriptor ),
                Source = Unknown,
            };
        }

        ApiParameterDescription CreateParameterDescriptionFromBinding( HttpParameterBinding parameterBinding )
        {
            Contract.Requires( parameterBinding != null );
            Contract.Ensures( Contract.Result<ApiParameterDescription>() != null );

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
            Contract.Requires( apiDescriptions != null );
            Contract.Requires( apiVersion != null );
            Contract.Ensures( Contract.Result<Collection<VersionedApiDescription>>() != null );

            var filteredDescriptions = new Dictionary<string, VersionedApiDescription>( StringComparer.OrdinalIgnoreCase );

            foreach ( var description in apiDescriptions )
            {
                var apiDescriptionId = description.GetUniqueID();

                if ( filteredDescriptions.ContainsKey( apiDescriptionId ) )
                {
                    var model = description.ActionDescriptor.GetApiVersionModel();

                    if ( model.DeclaredApiVersions.Contains( apiVersion ) )
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
            Contract.Requires( route != null );
            Contract.Requires( !string.IsNullOrEmpty( parameterName ) );

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
            var constraintsRule = constraint as string;

            if ( constraintsRule == null )
            {
                return true;
            }

            if ( parameterValue == null )
            {
                return false;
            }

            return Regex.IsMatch( parameterValue, $"^({constraintsRule})$", CultureInvariant | IgnoreCase );
        }

        static HttpActionBinding GetActionBinding( HttpActionDescriptor actionDescriptor )
        {
            Contract.Requires( actionDescriptor != null );

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