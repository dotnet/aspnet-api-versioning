// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.ApiExplorer;

using Asp.Versioning.Conventions;
using Asp.Versioning.Description;
using Asp.Versioning.OData;
using Asp.Versioning.Routing;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Template;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using System.Collections.ObjectModel;
using System.Net.Http.Formatting;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Description;
using System.Web.Http.ModelBinding;
using System.Web.Http.Routing;
using System.Web.Http.Services;
using System.Web.Http.ValueProviders;
using static System.StringComparison;
using static System.Text.RegularExpressions.RegexOptions;
using static System.Web.Http.Description.ApiParameterSource;

/// <summary>
/// Explores the URI space of versioned OData services based on routes, controllers, and actions available in the system.
/// </summary>
public class ODataApiExplorer : VersionedApiExplorer
{
    private static readonly Regex odataVariableRegex = new( $"{{\\*{ODataRouteConstants.ODataPath}}}", CultureInvariant | Compiled | IgnoreCase );
    private readonly ODataApiExplorerOptions options;
    private IModelTypeBuilder? modelTypeBuilder;

    /// <summary>
    /// Initializes a new instance of the <see cref="ODataApiExplorer"/> class.
    /// </summary>
    /// <param name="configuration">The current <see cref="HttpConfiguration">HTTP configuration</see>.</param>
    public ODataApiExplorer( HttpConfiguration configuration )
        : this( configuration, new ODataApiExplorerOptions( configuration ) ) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ODataApiExplorer"/> class.
    /// </summary>
    /// <param name="configuration">The current <see cref="HttpConfiguration">HTTP configuration</see>.</param>
    /// <param name="options">The associated <see cref="ODataApiExplorerOptions">API explorer options</see>.</param>
    public ODataApiExplorer( HttpConfiguration configuration, ODataApiExplorerOptions options )
        : base( configuration, options )
    {
        this.options = options;
        options.AdHocModelBuilder.OnModelCreated += MarkAsAdHoc;
    }

    /// <summary>
    /// Gets the options associated with the API explorer.
    /// </summary>
    /// <value>The <see cref="ODataApiExplorerOptions">API explorer options</see>.</value>
    protected new virtual ODataApiExplorerOptions Options => options;

    /// <summary>
    /// Gets the model type builder used by the API explorer.
    /// </summary>
    /// <value>The associated <see cref="IModelTypeBuilder">mode type builder</see>.</value>
    protected virtual IModelTypeBuilder ModelTypeBuilder =>
        modelTypeBuilder ??= Configuration.DependencyResolver.GetModelTypeBuilder();

    /// <inheritdoc />
    protected override bool ShouldExploreAction(
        string actionRouteParameterValue,
        HttpActionDescriptor actionDescriptor,
        IHttpRoute route,
        ApiVersion apiVersion )
    {
        ArgumentNullException.ThrowIfNull( actionDescriptor );

        if ( route is not ODataRoute )
        {
            return base.ShouldExploreAction( actionRouteParameterValue, actionDescriptor, route, apiVersion );
        }

        if ( actionDescriptor.ControllerDescriptor.ControllerType.IsMetadataController() )
        {
            if ( actionDescriptor.ActionName == nameof( MetadataController.GetServiceDocument ) )
            {
                if ( !Options.MetadataOptions.HasFlag( ODataMetadataOptions.ServiceDocument ) )
                {
                    return false;
                }
            }
            else
            {
                if ( !Options.MetadataOptions.HasFlag( ODataMetadataOptions.Metadata ) )
                {
                    return false;
                }
            }
        }

        if ( Options.UseApiExplorerSettings )
        {
            var setting = actionDescriptor.GetCustomAttributes<ApiExplorerSettingsAttribute>().FirstOrDefault();

            if ( setting?.IgnoreApi == true )
            {
                return false;
            }
        }

        return actionDescriptor.GetApiVersionMetadata().IsMappedTo( apiVersion );
    }

    /// <inheritdoc />
    protected override bool ShouldExploreController(
        string controllerRouteParameterValue,
        HttpControllerDescriptor controllerDescriptor,
        IHttpRoute route,
        ApiVersion apiVersion )
    {
        ArgumentNullException.ThrowIfNull( controllerDescriptor );
        ArgumentNullException.ThrowIfNull( route );

        if ( controllerDescriptor.ControllerType.IsMetadataController() )
        {
            controllerDescriptor.ControllerName = "OData";
            return Options.MetadataOptions > ODataMetadataOptions.None;
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

    /// <inheritdoc />
    protected override Collection<VersionedApiDescription> ExploreRouteControllers(
        IDictionary<string, HttpControllerDescriptor> controllerMappings,
        IHttpRoute route,
        ApiVersion apiVersion )
    {
        ArgumentNullException.ThrowIfNull( controllerMappings );

        Collection<VersionedApiDescription> apiDescriptions;

        if ( route is not ODataRoute )
        {
            apiDescriptions = base.ExploreRouteControllers( controllerMappings, route, apiVersion );

            if ( Options.AdHocModelBuilder.ModelConfigurations.Count == 0 )
            {
                ExploreQueryOptions( route, apiDescriptions );
            }
            else if ( apiDescriptions.Count > 0 )
            {
                using ( new AdHocEdmScope( apiDescriptions, Options.AdHocModelBuilder ) )
                {
                    ExploreQueryOptions( route, apiDescriptions );
                }
            }

            return apiDescriptions;
        }

        apiDescriptions = [];
        var modelSelector = Configuration.GetODataRootContainer( route ).GetRequiredService<IEdmModelSelector>();
        var edmModel = modelSelector.SelectModel( apiVersion );

        if ( edmModel == null )
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

        ExploreQueryOptions( route, apiDescriptions );
        return apiDescriptions;
    }

    /// <inheritdoc />
    protected override Collection<VersionedApiDescription> ExploreDirectRouteControllers(
        HttpControllerDescriptor controllerDescriptor,
        IReadOnlyList<HttpActionDescriptor> candidateActionDescriptors,
        IHttpRoute route,
        ApiVersion apiVersion )
    {
        var apiDescriptions = base.ExploreDirectRouteControllers( controllerDescriptor, candidateActionDescriptors, route, apiVersion );

        if ( apiDescriptions.Count == 0 )
        {
            return apiDescriptions;
        }

        if ( Options.AdHocModelBuilder.ModelConfigurations.Count == 0 )
        {
            ExploreQueryOptions( route, apiDescriptions );
        }
        else if ( apiDescriptions.Count > 0 )
        {
            using ( new AdHocEdmScope( apiDescriptions, Options.AdHocModelBuilder ) )
            {
                ExploreQueryOptions( route, apiDescriptions );
            }
        }

        return apiDescriptions;
    }

    /// <summary>
    /// Explores the OData query options for the specified API descriptions.
    /// </summary>
    /// <param name="apiDescriptions">The <see cref="IEnumerable{T}">sequence</see> of <see cref="VersionedApiDescription">API descriptions</see> to explore.</param>
    /// <param name="uriResolver">The associated <see cref="ODataUriResolver">OData URI resolver</see>.</param>
    protected virtual void ExploreQueryOptions(
        IEnumerable<VersionedApiDescription> apiDescriptions,
        ODataUriResolver uriResolver )
    {
        ArgumentNullException.ThrowIfNull( uriResolver );

        var queryOptions = Options.QueryOptions;
        var settings = new ODataQueryOptionSettings()
        {
            NoDollarPrefix = uriResolver.EnableNoDollarQueryOptions,
            DescriptionProvider = queryOptions.DescriptionProvider,
            DefaultQuerySettings = Configuration.GetDefaultQuerySettings(),
        };

        queryOptions.ApplyTo( apiDescriptions, settings );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static void MarkAsAdHoc( ODataModelBuilder builder, IEdmModel model ) =>
        model.SetAnnotationValue( model, AdHocAnnotation.Instance );

    private void ExploreQueryOptions( IHttpRoute route, Collection<VersionedApiDescription> apiDescriptions )
    {
        if ( apiDescriptions.Count == 0 )
        {
            return;
        }

        var uriResolver = Configuration.GetODataRootContainer( route ).GetRequiredService<ODataUriResolver>();

        ExploreQueryOptions( apiDescriptions, uriResolver );
    }

    private ResponseDescription CreateResponseDescriptionWithRoute(
        HttpActionDescriptor actionDescriptor,
        IHttpRoute route,
        ApiVersion apiVersion )
    {
        var description = CreateResponseDescription( actionDescriptor );
        var serviceProvider = actionDescriptor.Configuration.GetODataRootContainer( route );
        var returnType = description.ResponseType ?? description.DeclaredType;
        var selector = serviceProvider.GetRequiredService<IEdmModelSelector>();
        var model = selector.SelectModel( apiVersion )!;
        var context = new TypeSubstitutionContext( model, ModelTypeBuilder );

        description.ResponseType = returnType.SubstituteIfNecessary( context );

        return description;
    }

    private void ExploreRouteActions(
        IHttpRoute route,
        HttpControllerDescriptor controllerDescriptor,
        IHttpActionSelector actionSelector,
        Collection<VersionedApiDescription> apiDescriptions,
        ApiVersion apiVersion )
    {
        var actionMapping = actionSelector.GetActionMapping( controllerDescriptor );

        if ( actionMapping == null || actionMapping.Count == 0 )
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

                var parameterDescriptions = CreateParameterDescriptions( action, route, apiVersion );
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

                var routeBuilder = new ODataRouteBuilder( context );
                var relativePath = routeBuilder.Build();

                if ( routeBuilder.IsNavigationPropertyLink )
                {
                    var routeTemplates = routeBuilder.ExpandNavigationPropertyLinkTemplate( relativePath );
                    var afterPrefix = string.IsNullOrEmpty( context.RoutePrefix ) ? 0 : context.RoutePrefix!.Length + 1;

                    for ( var i = 0; i < routeTemplates.Count; i++ )
                    {
                        relativePath = routeTemplates[i];

                        var queryParamAdded = false;

                        if ( action.ActionName.StartsWith( "DeleteRef", Ordinal ) )
                        {
                            var handler = context.PathTemplateHandler;
                            var pathTemplate = handler.ParseTemplate( relativePath.Substring( afterPrefix ), context.Services );
                            var template = pathTemplate?.Segments.OfType<NavigationPropertyLinkSegmentTemplate>().FirstOrDefault();

                            if ( template != null )
                            {
                                var property = template.Segment.NavigationProperty;

                                if ( property.TargetMultiplicity() == EdmMultiplicity.Many )
                                {
                                    routeBuilder.AddOrReplaceRefIdQueryParameter();
                                    queryParamAdded = true;
                                }
                            }
                        }

                        PopulateActionDescriptions( action, route, context, relativePath, apiDescriptions, apiVersion );

                        if ( queryParamAdded )
                        {
                            for ( var j = 0; j < context.ParameterDescriptions.Count; j++ )
                            {
                                var parameter = context.ParameterDescriptions[j];

                                if ( parameter.Name == "$id" || parameter.Name == "id" )
                                {
                                    context.ParameterDescriptions.RemoveAt( j );
                                    break;
                                }
                            }
                        }
                    }
                }
                else
                {
                    PopulateActionDescriptions( action, route, context, relativePath, apiDescriptions, apiVersion );
                }
            }
        }
    }

    private static HttpActionBinding? GetActionBinding( HttpActionDescriptor actionDescriptor )
    {
        var controllerDescriptor = actionDescriptor.ControllerDescriptor;

        if ( controllerDescriptor == null )
        {
            return null;
        }

        var actionValueBinder = controllerDescriptor.Configuration.Services.GetActionValueBinder();

        return actionValueBinder?.GetBinding( actionDescriptor );
    }

    private static bool WillReadUri( HttpParameterBinding parameterBinding )
    {
        if ( parameterBinding is not IValueProviderParameterBinding binding )
        {
            return false;
        }

        var valueProviderFactories = binding.ValueProviderFactories;
        var willReadUri = valueProviderFactories.Any() && valueProviderFactories.All( factory => factory is IUriValueProviderFactory );

        return willReadUri;
    }

    private ApiParameterDescription CreateParameterDescriptionFromBinding(
        HttpParameterBinding parameterBinding,
        IServiceProvider serviceProvider,
        ApiVersion apiVersion )
    {
        var descriptor = parameterBinding.Descriptor;
        var description = CreateParameterDescription( descriptor );

        if ( parameterBinding.WillReadBody )
        {
            description.Source = FromBody;

            var parameterType = descriptor.ParameterType;
            var selector = serviceProvider.GetRequiredService<IEdmModelSelector>();
            var model = selector.SelectModel( apiVersion )!;
            var context = new TypeSubstitutionContext( model, ModelTypeBuilder );
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

    private IList<ApiParameterDescription> CreateParameterDescriptions(
        HttpActionDescriptor actionDescriptor,
        IHttpRoute route,
        ApiVersion apiVersion )
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
                    list.Add( CreateParameterDescriptionFromBinding( binding, serviceProvider, apiVersion ) );
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
            if ( entry.Value is ApiVersionRouteConstraint )
            {
                list.Add( new ApiParameterDescription() { Name = entry.Key, Source = FromUri } );
                break;
            }
        }

        return list;
    }

    private static IEnumerable<MediaTypeFormatter> GetInnerFormatters( IEnumerable<MediaTypeFormatter> mediaTypeFormatters ) =>
        mediaTypeFormatters.Select( Decorator.GetInner );

    private static void PopulateMediaTypeFormatters(
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

    private void PopulateActionDescriptions(
        HttpActionDescriptor actionDescriptor,
        IHttpRoute route,
        ODataRouteBuilderContext routeBuilderContext,
        string relativePath,
        Collection<VersionedApiDescription> apiDescriptions,
        ApiVersion apiVersion )
    {
        var documentation = DocumentationProvider?.GetDocumentation( actionDescriptor );
        var responseDescription = CreateResponseDescriptionWithRoute( actionDescriptor, route, apiVersion );
        var responseType = responseDescription.ResponseType ?? responseDescription.DeclaredType;
        var requestFormatters = new List<MediaTypeFormatter>();
        var responseFormatters = new List<MediaTypeFormatter>();
        var supportedMethods = GetHttpMethodsSupportedByAction( route, actionDescriptor );
        var metadata = actionDescriptor.GetApiVersionMetadata();
        var model = metadata.Map( ApiVersionMapping.Explicit );
        var deprecated = !model.IsApiVersionNeutral && model.DeprecatedApiVersions.Contains( apiVersion );

        PopulateMediaTypeFormatters( actionDescriptor, routeBuilderContext.ParameterDescriptions, route, responseType, requestFormatters, responseFormatters );

        for ( var i = 0; i < supportedMethods.Count; i++ )
        {
            var method = supportedMethods[i];
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
                SunsetPolicy = SunsetPolicyManager.ResolvePolicyOrDefault( metadata.Name, apiVersion ),
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