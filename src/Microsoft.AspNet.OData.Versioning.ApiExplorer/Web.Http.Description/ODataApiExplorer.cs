namespace Microsoft.Web.Http.Description
{
    using Microsoft.AspNet.OData;
    using Microsoft.AspNet.OData.Builder;
    using Microsoft.AspNet.OData.Formatter;
    using Microsoft.AspNet.OData.Routing;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.OData.Edm;
    using Microsoft.OData.UriParser;
    using Microsoft.Web.Http.Routing;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
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
            ModelTypeBuilder = new DefaultModelTypeBuilder();
        }

        /// <summary>
        /// Gets the options associated with the API explorer.
        /// </summary>
        /// <value>The <see cref="ODataApiExplorerOptions">API explorer options</see>.</value>
        protected new virtual ODataApiExplorerOptions Options { get; }

        /// <summary>
        /// Gets the model type builder used by the API explorer.
        /// </summary>
        /// <value>The associated <see cref="IModelTypeBuilder">mode type builder</see>.</value>
        protected virtual IModelTypeBuilder ModelTypeBuilder { get; }

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
            if ( actionDescriptor == null )
            {
                throw new ArgumentNullException( nameof( actionDescriptor ) );
            }

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

            return actionDescriptor.IsMappedTo( apiVersion );
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
            if ( controllerDescriptor == null )
            {
                throw new ArgumentNullException( nameof( controllerDescriptor ) );
            }

            if ( route == null )
            {
                throw new ArgumentNullException( nameof( route ) );
            }

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

            return true;
        }

        /// <summary>
        /// Explores controllers that do not use direct routes (aka "attribute" routing).
        /// </summary>
        /// <param name="controllerMappings">The <see cref="IDictionary{TKey, TValue}">collection</see> of controller mappings.</param>
        /// <param name="route">The <see cref="IHttpRoute">route</see> to explore.</param>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> to explore.</param>
        /// <returns>The <see cref="Collection{T}">collection</see> of discovered <see cref="VersionedApiDescription">API descriptions</see>.</returns>
        protected override Collection<VersionedApiDescription> ExploreRouteControllers( IDictionary<string, HttpControllerDescriptor> controllerMappings, IHttpRoute route, ApiVersion apiVersion )
        {
            if ( controllerMappings == null )
            {
                throw new ArgumentNullException( nameof( controllerMappings ) );
            }

            if ( !( route is ODataRoute ) )
            {
                return base.ExploreRouteControllers( controllerMappings, route, apiVersion );
            }

            var apiDescriptions = new Collection<VersionedApiDescription>();
            var edmModel = Configuration.GetODataRootContainer( route ).GetRequiredService<IEdmModel>();
            var routeApiVersion = edmModel.GetAnnotationValue<ApiVersionAnnotation>( edmModel )?.ApiVersion;

            if ( routeApiVersion != apiVersion )
            {
                return apiDescriptions;
            }

            var actionSelector = Configuration.Services.GetActionSelector();

            foreach ( var controllerMapping in controllerMappings )
            {
                var controllerVariableValue = controllerMapping.Key;

                foreach ( var controllerDescriptor in controllerMapping.Value.AsEnumerable() )
                {
                    if ( ShouldExploreController( controllerVariableValue, controllerDescriptor, route, apiVersion ) )
                    {
                        ExploreRouteActions( route, controllerDescriptor, actionSelector, apiDescriptions, apiVersion );
                    }
                }
            }

            ExploreQueryOptions( apiDescriptions, Configuration.GetODataRootContainer( route ).GetRequiredService<ODataUriResolver>() );

            return apiDescriptions;
        }

        /// <summary>
        /// Explores the OData query options for the specified API descriptions.
        /// </summary>
        /// <param name="apiDescriptions">The <see cref="IEnumerable{T}">sequence</see> of <see cref="VersionedApiDescription">API descriptions</see> to explore.</param>
        /// <param name="uriResolver">The associated <see cref="ODataUriResolver">OData URI resolver</see>.</param>
        protected virtual void ExploreQueryOptions( IEnumerable<VersionedApiDescription> apiDescriptions, ODataUriResolver uriResolver )
        {
            if ( uriResolver == null )
            {
                throw new ArgumentNullException( nameof( uriResolver ) );
            }

            var queryOptions = Options.QueryOptions;
            var settings = new ODataQueryOptionSettings()
            {
                NoDollarPrefix = uriResolver.EnableNoDollarQueryOptions,
                DescriptionProvider = queryOptions.DescriptionProvider,
            };

            queryOptions.ApplyTo( apiDescriptions, settings );
        }

        ResponseDescription CreateResponseDescriptionWithRoute( HttpActionDescriptor actionDescriptor, IHttpRoute route )
        {
            var description = CreateResponseDescription( actionDescriptor );
            var serviceProvider = actionDescriptor.Configuration.GetODataRootContainer( route );
            var returnType = description.ResponseType ?? description.DeclaredType;
            var context = new TypeSubstitutionContext( serviceProvider, ModelTypeBuilder );

            description.ResponseType = returnType.SubstituteIfNecessary( context );

            return description;
        }

        void ExploreRouteActions(
            IHttpRoute route,
            HttpControllerDescriptor controllerDescriptor,
            IHttpActionSelector actionSelector,
            Collection<VersionedApiDescription> apiDescriptions,
            ApiVersion apiVersion )
        {
            var actionMapping = actionSelector.GetActionMapping( controllerDescriptor );

            if ( actionMapping == null )
            {
                return;
            }

            foreach ( var grouping in actionMapping )
            {
                foreach ( var action in grouping )
                {
                    if ( !ShouldExploreAction( actionRouteParameterValue: string.Empty, action, route, apiVersion ) )
                    {
                        continue;
                    }

                    var parameterDescriptions = CreateParameterDescriptions( action, route );
                    var context = new ODataRouteBuilderContext(
                                    Configuration,
                                    apiVersion,
                                    (ODataRoute) route,
                                    action,
                                    parameterDescriptions,
                                    ModelTypeBuilder,
                                    Options );

                    if ( context.IsRouteExcluded )
                    {
                        continue;
                    }

                    var relativePath = new ODataRouteBuilder( context ).Build();

                    PopulateActionDescriptions( action, route, context, relativePath, apiDescriptions, apiVersion );
                }
            }
        }

        static HttpActionBinding? GetActionBinding( HttpActionDescriptor actionDescriptor )
        {
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

        ApiParameterDescription CreateParameterDescriptionFromBinding( HttpParameterBinding parameterBinding, IServiceProvider serviceProvider )
        {
            var descriptor = parameterBinding.Descriptor;
            var description = CreateParameterDescription( descriptor );

            if ( parameterBinding.WillReadBody )
            {
                description.Source = FromBody;

                var parameterType = descriptor.ParameterType;
                var context = new TypeSubstitutionContext( serviceProvider, ModelTypeBuilder );
                var substitutedType = parameterType.SubstituteIfNecessary( context );

                if ( parameterType != substitutedType )
                {
                    description.ParameterDescriptor = new ODataModelBoundParameterDescriptor( descriptor, substitutedType );
                }

                return description;
            }

            if ( WillReadUri( parameterBinding ) )
            {
                description.Source = FromUri;
            }

            return description;
        }

        IList<ApiParameterDescription> CreateParameterDescriptions( HttpActionDescriptor actionDescriptor, IHttpRoute route )
        {
            var list = new List<ApiParameterDescription>();
            var actionBinding = GetActionBinding( actionDescriptor );

            if ( actionBinding != null )
            {
                var configuration = actionDescriptor.Configuration;
                var serviceProvider = configuration.GetODataRootContainer( route );
                var parameterBindings = actionBinding.ParameterBindings;

                if ( parameterBindings != null )
                {
                    foreach ( var binding in parameterBindings )
                    {
                        list.Add( CreateParameterDescriptionFromBinding( binding, serviceProvider ) );
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

        static void PopulateMediaTypeFormatters(
           HttpActionDescriptor actionDescriptor,
           IList<ApiParameterDescription> parameterDescriptions,
           IHttpRoute route,
           Type responseType,
           IList<MediaTypeFormatter> requestFormatters,
           IList<MediaTypeFormatter> responseFormatters )
        {
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

        void PopulateActionDescriptions(
            HttpActionDescriptor actionDescriptor,
            IHttpRoute route,
            ODataRouteBuilderContext routeBuilderContext,
            string relativePath,
            Collection<VersionedApiDescription> apiDescriptions,
            ApiVersion apiVersion )
        {
            var documentation = DocumentationProvider?.GetDocumentation( actionDescriptor );
            var responseDescription = CreateResponseDescriptionWithRoute( actionDescriptor, route );
            var responseType = responseDescription.ResponseType ?? responseDescription.DeclaredType;
            var requestFormatters = new List<MediaTypeFormatter>();
            var responseFormatters = new List<MediaTypeFormatter>();
            var supportedMethods = GetHttpMethodsSupportedByAction( route, actionDescriptor );
            var model = actionDescriptor.GetApiVersionModel();
            var deprecated = !model.IsApiVersionNeutral && model.DeprecatedApiVersions.Contains( apiVersion );

            PopulateMediaTypeFormatters( actionDescriptor, routeBuilderContext.ParameterDescriptions, route, responseType, requestFormatters, responseFormatters );

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
                    Properties = { [typeof( IEdmModel )] = routeBuilderContext.EdmModel },
                };

                if ( routeBuilderContext.EntitySet != null )
                {
                    apiDescription.Properties[typeof( IEdmEntitySet )] = routeBuilderContext.EntitySet;
                }

                if ( routeBuilderContext.Operation != null )
                {
                    apiDescription.Properties[typeof( IEdmOperation )] = routeBuilderContext.Operation;
                }

                apiDescription.ParameterDescriptions.AddRange( routeBuilderContext.ParameterDescriptions );
                apiDescription.SupportedRequestBodyFormatters.AddRange( requestFormatters );
                apiDescription.SupportedResponseFormatters.AddRange( responseFormatters );
                PopulateApiVersionParameters( apiDescription, apiVersion );
                apiDescriptions.Add( apiDescription );
            }
        }
    }
}