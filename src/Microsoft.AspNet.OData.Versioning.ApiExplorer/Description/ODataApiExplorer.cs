namespace Microsoft.Web.Http.Description
{
    using Microsoft.OData.Edm;
    using Microsoft.Web.Http.Routing;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Net.Http.Formatting;
    using System.Text.RegularExpressions;
    using System.Web.Http;
    using System.Web.Http.Controllers;
    using System.Web.Http.Description;
    using System.Web.Http.ModelBinding;
    using System.Web.Http.Routing;
    using System.Web.Http.Services;
    using System.Web.Http.ValueProviders;
    using System.Web.OData;
    using System.Web.OData.Formatter;
    using System.Web.OData.Routing;
    using static System.Text.RegularExpressions.RegexOptions;
    using static System.Web.Http.Description.ApiParameterSource;

    /// <summary>
    /// Explores the URI space of versioned OData services based on routes, controllers, and actions available in the system.
    /// </summary>
    public class ODataApiExplorer : VersionedApiExplorer
    {
        static readonly Regex odataVariableRegex = new Regex( $"{{\\*{ODataRouteConstants.ODataPath}}}", CultureInvariant | Compiled | IgnoreCase );

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataApiExplorer"/> class.
        /// </summary>
        /// <param name="configuration">The current <see cref="HttpConfiguration">HTTP configuration</see>.</param>
        public ODataApiExplorer( HttpConfiguration configuration ) : this( configuration, new ODataApiExplorerOptions( configuration ) ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataApiExplorer"/> class.
        /// </summary>
        /// <param name="configuration">The current <see cref="HttpConfiguration">HTTP configuration</see>.</param>
        /// <param name="options">The associated <see cref="ODataApiExplorerOptions">API explorer options</see>.</param>
        public ODataApiExplorer( HttpConfiguration configuration, ODataApiExplorerOptions options ) : base( configuration, options )
        {
            Options = options;
        }

        /// <summary>
        /// Gets the options associated with the API explorer.
        /// </summary>
        /// <value>The <see cref="ODataApiExplorerOptions">API explorer options</see>.</value>
        new protected virtual ODataApiExplorerOptions Options { get; }

        /// <summary>
        /// Determines whether the action should be considered.
        /// </summary>
        /// <param name="actionRouteParameterValue">The action route parameter value.</param>
        /// <param name="actionDescriptor">The associated <see cref="HttpActionDescriptor">action descriptor</see>.</param>
        /// <param name="route">The associated <see cref="IHttpRoute">route</see>.</param>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> to consider the controller for.</param>
        /// <returns>True if the action should be explored; otherwise, false.</returns>
        protected override bool ShouldExploreAction( string actionRouteParameterValue, HttpActionDescriptor actionDescriptor, IHttpRoute route, ApiVersion apiVersion )
        {
            Arg.NotNull( actionDescriptor, nameof( actionDescriptor ) );
            Arg.NotNull( route, nameof( route ) );
            Arg.NotNull( apiVersion, nameof( apiVersion ) );

            if ( !( route is ODataRoute ) )
            {
                return base.ShouldExploreAction( actionRouteParameterValue, actionDescriptor, route, apiVersion );
            }

            if ( Options.UseApiExplorerSettings )
            {
                var setting = actionDescriptor.GetCustomAttributes<ApiExplorerSettingsAttribute>().FirstOrDefault();

                if ( setting?.IgnoreApi == true )
                {
                    return false;
                }
            }

            var versions = actionDescriptor.GetApiVersions();

            if ( versions.Contains( apiVersion ) )
            {
                return true;
            }

            return versions.Count == 0 && actionDescriptor.ControllerDescriptor.GetDeclaredApiVersions().Contains( apiVersion );
        }

        /// <summary>
        /// Determines whether the controller should be considered.
        /// </summary>
        /// <param name="controllerRouteParameterValue">The controller route parameter value.</param>
        /// <param name="controllerDescriptor">The associated <see cref="HttpControllerDescriptor">controller descriptor</see>.</param>
        /// <param name="route">The associated <see cref="IHttpRoute">route</see>.</param>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> to consider the controller for.</param>
        /// <returns>True if the controller should be explored; otherwise, false.</returns>
        protected override bool ShouldExploreController( string controllerRouteParameterValue, HttpControllerDescriptor controllerDescriptor, IHttpRoute route, ApiVersion apiVersion )
        {
            Arg.NotNull( controllerDescriptor, nameof( controllerDescriptor ) );
            Arg.NotNull( route, nameof( route ) );
            Arg.NotNull( apiVersion, nameof( apiVersion ) );

            if ( typeof( MetadataController ).IsAssignableFrom( controllerDescriptor.ControllerType ) )
            {
                return false;
            }

            var routeTemplate = route.RouteTemplate;

            if ( !odataVariableRegex.IsMatch( routeTemplate ) )
            {
                return base.ShouldExploreController( controllerRouteParameterValue, controllerDescriptor, route, apiVersion );
            }

            if ( Options.UseApiExplorerSettings )
            {
                var setting = controllerDescriptor.GetCustomAttributes<ApiExplorerSettingsAttribute>().FirstOrDefault();

                if ( setting?.IgnoreApi == true )
                {
                    return false;
                }
            }

            return controllerDescriptor.GetDeclaredApiVersions().Contains( apiVersion );
        }

        /// <summary>
        /// Explores controllers that do not use direct routes (aka "attribute" routing)
        /// </summary>
        /// <param name="controllerMappings">The <see cref="IDictionary{TKey, TValue}">collection</see> of controller mappings.</param>
        /// <param name="route">The <see cref="IHttpRoute">route</see> to explore.</param>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> to explore.</param>
        /// <returns>The <see cref="Collection{T}">collection</see> of discovered <see cref="VersionedApiDescription">API descriptions</see>.</returns>
        protected override Collection<VersionedApiDescription> ExploreRouteControllers( IDictionary<string, HttpControllerDescriptor> controllerMappings, IHttpRoute route, ApiVersion apiVersion )
        {
            if ( !( route is ODataRoute ) )
            {
                return base.ExploreRouteControllers( controllerMappings, route, apiVersion );
            }

            var apiDescriptions = new Collection<VersionedApiDescription>();
            var actionSelector = Configuration.Services.GetActionSelector();

            foreach ( var controllerMapping in controllerMappings )
            {
                var controllerVariableValue = controllerMapping.Key;

                foreach ( var controllerDescriptor in controllerMapping.Value.AsEnumerable() )
                {
                    if ( ShouldExploreController( controllerVariableValue, controllerDescriptor, route, apiVersion ) )
                    {
                        var localPath = odataVariableRegex.Replace( route.RouteTemplate, controllerVariableValue );
                        ExploreRouteActions( route, localPath, controllerDescriptor, actionSelector, apiDescriptions, apiVersion );
                    }
                }
            }

            return apiDescriptions;
        }

        void ExploreRouteActions(
            IHttpRoute route,
            string localPath,
            HttpControllerDescriptor controllerDescriptor,
            IHttpActionSelector actionSelector,
            Collection<VersionedApiDescription> apiDescriptions,
            ApiVersion apiVersion )
        {
            Contract.Requires( route != null );
            Contract.Requires( controllerDescriptor != null );
            Contract.Requires( actionSelector != null );
            Contract.Requires( apiDescriptions != null );
            Contract.Requires( apiVersion != null );

            var actionMapping = actionSelector.GetActionMapping( controllerDescriptor );

            if ( actionMapping == null )
            {
                return;
            }

            const string ActionRouteParameterName = null;

            foreach ( var grouping in actionMapping )
            {
                foreach ( var action in grouping )
                {
                    if ( ShouldExploreAction( ActionRouteParameterName, action, route, apiVersion ) )
                    {
                        PopulateActionDescriptions( action, route, localPath, apiDescriptions, apiVersion );
                    }
                }
            }
        }

        static HttpActionBinding GetActionBinding( HttpActionDescriptor actionDescriptor )
        {
            Contract.Requires( actionDescriptor != null );

            var controllerDescriptor = actionDescriptor.ControllerDescriptor;

            if ( controllerDescriptor == null )
            {
                return null;
            }

            var actionValueBinder = controllerDescriptor.Configuration.Services.GetActionValueBinder();

            return actionValueBinder?.GetBinding( actionDescriptor );
        }

        static bool WillReadUri( HttpParameterBinding parameterBinding )
        {
            if ( !( parameterBinding is IValueProviderParameterBinding binding ) )
            {
                return false;
            }

            var valueProviderFactories = binding.ValueProviderFactories;
            var willReadUri = valueProviderFactories.Any() && valueProviderFactories.All( factory => factory is IUriValueProviderFactory );

            return willReadUri;
        }

        ApiParameterDescription CreateParameterDescriptionFromBinding( HttpParameterBinding parameterBinding )
        {
            Contract.Requires( parameterBinding != null );
            Contract.Ensures( Contract.Result<ApiParameterDescription>() != null );

            var description = CreateParameterDescription( parameterBinding.Descriptor );

            if ( parameterBinding.WillReadBody )
            {
                description.Source = FromBody;
                return description;
            }

            if ( WillReadUri( parameterBinding ) )
            {
                description.Source = FromUri;
            }

            return description;
        }

        IReadOnlyList<ApiParameterDescription> CreateParameterDescriptions( HttpActionDescriptor actionDescriptor, IHttpRoute route )
        {
            Contract.Requires( actionDescriptor != null );
            Contract.Requires( route != null );
            Contract.Ensures( Contract.Result<IList<ApiParameterDescription>>() != null );

            var list = new List<ApiParameterDescription>();
            var actionBinding = GetActionBinding( actionDescriptor );

            if ( actionBinding != null )
            {
                var parameterBindings = actionBinding.ParameterBindings;

                if ( parameterBindings != null )
                {
                    foreach ( var binding in parameterBindings )
                    {
                        list.Add( CreateParameterDescriptionFromBinding( binding ) );
                    }
                }
            }
            else
            {
                var parameters = actionDescriptor.GetParameters();

                if ( parameters != null )
                {
                    foreach ( var descriptor in parameters )
                    {
                        list.Add( CreateParameterDescription( descriptor ) );
                    }
                }
            }

            foreach ( var entry in route.Constraints )
            {
                if ( entry.Value is ApiVersionRouteConstraint constraint )
                {
                    list.Add( new ApiParameterDescription() { Name = entry.Key, Source = FromUri } );
                    break;
                }
            }

            return list;
        }

        static IEnumerable<MediaTypeFormatter> GetInnerFormatters( IEnumerable<MediaTypeFormatter> mediaTypeFormatters ) => mediaTypeFormatters.Select( Decorator.GetInner );

        void PopulateMediaTypeFormatters(
           HttpActionDescriptor actionDescriptor,
           IReadOnlyList<ApiParameterDescription> parameterDescriptions,
           IHttpRoute route,
           Type responseType,
           IList<MediaTypeFormatter> requestFormatters,
           IList<MediaTypeFormatter> responseFormatters )
        {
            Contract.Requires( actionDescriptor != null );
            Contract.Requires( parameterDescriptions != null );
            Contract.Requires( route != null );
            Contract.Requires( requestFormatters != null );
            Contract.Requires( responseFormatters != null );

            if ( route is ODataRoute )
            {
                foreach ( var formatter in actionDescriptor.Configuration.Formatters.OfType<ODataMediaTypeFormatter>() )
                {
                    requestFormatters.Add( formatter );
                    responseFormatters.Add( formatter );
                }

                return;
            }

            var bodyParameter = parameterDescriptions.FirstOrDefault( p => p.Source == FromBody );

            if ( bodyParameter != null )
            {
                var paramType = bodyParameter.ParameterDescriptor.ParameterType;
                requestFormatters.AddRange( GetInnerFormatters( actionDescriptor.Configuration.Formatters.Where( f => f.CanReadType( paramType ) ) ) );
            }

            if ( responseType != null )
            {
                responseFormatters.AddRange( GetInnerFormatters( actionDescriptor.Configuration.Formatters.Where( f => f.CanWriteType( responseType ) ) ) );
            }
        }

        void PopulateActionDescriptions( HttpActionDescriptor actionDescriptor, IHttpRoute route, string localPath, Collection<VersionedApiDescription> apiDescriptions, ApiVersion apiVersion )
        {
            var parameterDescriptions = CreateParameterDescriptions( actionDescriptor, route );
            var context = new ODataRouteBuilderContext( Configuration, localPath, (ODataRoute) route, actionDescriptor, parameterDescriptions );

            if ( context.EdmModel.EntityContainer == null )
            {
                return;
            }

            var relativePath = new ODataRouteBuilder( context ).Build();
            var documentation = DocumentationProvider?.GetDocumentation( actionDescriptor );
            var responseDescription = CreateResponseDescription( actionDescriptor );
            var responseType = responseDescription.ResponseType ?? responseDescription.DeclaredType;
            var requestFormatters = new List<MediaTypeFormatter>();
            var responseFormatters = new List<MediaTypeFormatter>();
            var supportedMethods = GetHttpMethodsSupportedByAction( route, actionDescriptor );
            var deprecated = actionDescriptor.ControllerDescriptor.GetApiVersionModel().DeprecatedApiVersions.Contains( apiVersion );

            PopulateMediaTypeFormatters( actionDescriptor, parameterDescriptions, route, responseType, requestFormatters, responseFormatters );

            foreach ( var method in supportedMethods )
            {
                var apiDescription = new VersionedApiDescription()
                {
                    Documentation = documentation,
                    HttpMethod = method,
                    RelativePath = relativePath,
                    ActionDescriptor = actionDescriptor,
                    Route = route,
                    ResponseDescription = responseDescription,
                    ApiVersion = apiVersion,
                    IsDeprecated = deprecated,
                    Properties =
                    {
                        [typeof( IEdmModel )] = context.EdmModel
                    }
                };

                apiDescription.ParameterDescriptions.AddRange( parameterDescriptions );
                apiDescription.SupportedRequestBodyFormatters.AddRange( requestFormatters );
                apiDescription.SupportedResponseFormatters.AddRange( responseFormatters );
                PopulateApiVersionParameters( apiDescription, apiVersion );
                apiDescriptions.Add( apiDescription );
            }
        }
    }
}