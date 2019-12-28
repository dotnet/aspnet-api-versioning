namespace Microsoft.AspNetCore.Mvc.ApiExplorer
{
    using Microsoft.AspNet.OData;
    using Microsoft.AspNet.OData.Builder;
    using Microsoft.AspNet.OData.Query;
    using Microsoft.AspNet.OData.Routing;
    using Microsoft.AspNet.OData.Routing.Template;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.ActionConstraints;
    using Microsoft.AspNetCore.Mvc.ApplicationModels;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Mvc.Formatters;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Microsoft.AspNetCore.Mvc.Routing;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.AspNetCore.Routing.Template;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Microsoft.OData.Edm;
    using Microsoft.OData.UriParser;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using static Microsoft.AspNet.OData.Routing.ODataRouteActionType;
    using static Microsoft.AspNetCore.Http.StatusCodes;
    using static Microsoft.AspNetCore.Mvc.ModelBinding.BindingSource;
    using static Microsoft.AspNetCore.Mvc.Versioning.ApiVersionMapping;
    using static System.Linq.Enumerable;
    using static System.StringComparison;

    /// <summary>
    /// Represents an API explorer that provides <see cref="ApiDescription">API descriptions</see> for actions represented by
    /// <see cref="ControllerActionDescriptor">controller action descriptors</see> that are defined by
    /// <see cref="ODataController">OData controllers</see> and are <see cref="ApiVersion">API version</see> aware.
    /// </summary>
    [CLSCompliant( false )]
    public class ODataApiDescriptionProvider : IApiDescriptionProvider
    {
        const int AfterApiVersioning = -100;
        static readonly string[] SupportedHttpMethodConventions = new[]
        {
            "GET",
            "PUT",
            "POST",
            "DELETE",
            "PATCH",
            "HEAD",
            "OPTIONS",
        };
        readonly IOptions<ODataApiExplorerOptions> options;
        readonly Lazy<ModelMetadata> modelMetadata;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataApiDescriptionProvider"/> class.
        /// </summary>
        /// <param name="routeCollectionProvider">The <see cref="IODataRouteCollectionProvider">OData route collection provider</see> associated with the description provider.</param>
        /// <param name="inlineConstraintResolver">The <see cref="IInlineConstraintResolver">inline constraint resolver</see> used to parse route template constraints.</param>
        /// <param name="metadataProvider">The <see cref="IModelMetadataProvider">provider</see> used to retrieve model metadata.</param>
        /// <param name="defaultQuerySettings">The OData <see cref="DefaultQuerySettings">default query setting</see>.</param>
        /// <param name="options">The <see cref="IOptions{TOptions}">container</see> of configured <see cref="ODataApiExplorerOptions">API explorer options</see>.</param>
        /// <param name="mvcOptions">A <see cref="IOptions{TOptions}">holder</see> containing the current <see cref="Mvc.MvcOptions">MVC options</see>.</param>
        public ODataApiDescriptionProvider(
            IODataRouteCollectionProvider routeCollectionProvider,
            IInlineConstraintResolver inlineConstraintResolver,
            IModelMetadataProvider metadataProvider,
            DefaultQuerySettings defaultQuerySettings,
            IOptions<ODataApiExplorerOptions> options,
            IOptions<MvcOptions> mvcOptions )
        {
            if ( mvcOptions == null )
            {
                throw new ArgumentNullException( nameof( mvcOptions ) );
            }

            RouteCollectionProvider = routeCollectionProvider;
            ModelTypeBuilder = new DefaultModelTypeBuilder();
            ConstraintResolver = inlineConstraintResolver;
            MetadataProvider = metadataProvider;
            DefaultQuerySettings = defaultQuerySettings;
            this.options = options;
            MvcOptions = mvcOptions.Value;
            modelMetadata = new Lazy<ModelMetadata>( NewModelMetadata );
        }

        /// <summary>
        /// Gets the model type builder used by the API explorer.
        /// </summary>
        /// <value>The associated <see cref="IModelTypeBuilder">mode type builder</see>.</value>
        protected virtual IModelTypeBuilder ModelTypeBuilder { get; }

        /// <summary>
        /// Gets the associated route collection provider.
        /// </summary>
        /// <value>The associated <see cref="IODataRouteCollectionProvider">OData route collection provider</see>.</value>
        protected IODataRouteCollectionProvider RouteCollectionProvider { get; }

        /// <summary>
        /// Gets the model metadata provider associated with the description provider.
        /// </summary>
        /// <value>The <see cref="IModelMetadataProvider">provider</see> used to retrieve model metadata.</value>
        protected IModelMetadataProvider MetadataProvider { get; }

        /// <summary>
        /// Gets the OData default query settings.
        /// </summary>
        /// <value>The OData <see cref="DefaultQuerySettings">default query setting</see>.</value>
        protected DefaultQuerySettings DefaultQuerySettings { get; }

        /// <summary>
        /// Gets the object used to resolve inline constraints.
        /// </summary>
        /// <value>The associated <see cref="IInlineConstraintResolver">inline constraint resolver</see>.</value>
        protected IInlineConstraintResolver ConstraintResolver { get; }

        /// <summary>
        /// Gets the options associated with the API explorer.
        /// </summary>
        /// <value>The current <see cref="ApiExplorerOptions">API explorer options</see>.</value>
        protected ODataApiExplorerOptions Options => options.Value;

        /// <summary>
        /// Gets the current MVC options.
        /// </summary>
        /// <value>The current <see cref="Mvc.MvcOptions">MVC options</see>.</value>
        protected MvcOptions MvcOptions { get; }

        /// <summary>
        /// Gets the order precedence of the current API description provider.
        /// </summary>
        /// <value>The order precedence of the current API description provider. The default value is -100.</value>
        public virtual int Order => AfterApiVersioning;

        /// <summary>
        /// Occurs after the providers have been executed.
        /// </summary>
        /// <param name="context">The current <see cref="ApiDescriptionProviderContext">execution context</see>.</param>
        /// <remarks>The default implementation performs no action.</remarks>
        public virtual void OnProvidersExecuted( ApiDescriptionProviderContext context )
        {
            if ( context == null )
            {
                throw new ArgumentNullException( nameof( context ) );
            }

            var mappings = RouteCollectionProvider.Items;
            var results = context.Results;
            var groupNameFormat = Options.GroupNameFormat;
            var formatProvider = CultureInfo.CurrentCulture;

            foreach ( var action in context.Actions.OfType<ControllerActionDescriptor>() )
            {
                if ( !action.ControllerTypeInfo.IsODataController() ||
                      action.ControllerTypeInfo.IsMetadataController() ||
                     !IsVisible( action ) )
                {
                    continue;
                }

                var model = action.GetApiVersionModel( Explicit | Implicit );

                if ( model.IsApiVersionNeutral )
                {
                    for ( var i = 0; i < mappings.Count; i++ )
                    {
                        var mapping = mappings[i];
                        var descriptions = new List<ApiDescription>();
                        var groupName = mapping.ApiVersion.ToString( groupNameFormat, formatProvider );

                        foreach ( var apiDescription in NewODataApiDescriptions( action, groupName, mapping ) )
                        {
                            results.Add( apiDescription );
                            descriptions.Add( apiDescription );
                        }

                        if ( descriptions.Count > 0 )
                        {
                            ExploreQueryOptions( descriptions, mapping.Services.GetRequiredService<ODataUriResolver>() );
                        }
                    }
                }
                else
                {
                    for ( var i = 0; i < model.DeclaredApiVersions.Count; i++ )
                    {
                        var apiVersion = model.DeclaredApiVersions[i];
                        var groupName = apiVersion.ToString( groupNameFormat, formatProvider );

                        if ( !mappings.TryGetValue( apiVersion, out var mappingsPerApiVersion ) )
                        {
                            continue;
                        }

                        for ( var j = 0; j < mappingsPerApiVersion!.Count; j++ )
                        {
                            var mapping = mappingsPerApiVersion[j];
                            var descriptions = new List<ApiDescription>();

                            foreach ( var apiDescription in NewODataApiDescriptions( action, groupName, mapping ) )
                            {
                                results.Add( apiDescription );
                                descriptions.Add( apiDescription );
                            }

                            if ( descriptions.Count > 0 )
                            {
                                ExploreQueryOptions( descriptions, mapping.Services.GetRequiredService<ODataUriResolver>() );
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Occurs when the providers are being executed.
        /// </summary>
        /// <param name="context">The current <see cref="ApiDescriptionProviderContext">execution context</see>.</param>
        /// <remarks>The default implementation performs no operation.</remarks>
        public virtual void OnProvidersExecuting( ApiDescriptionProviderContext context ) { }

        /// <summary>
        /// Returns a value indicating whether the specified action is visible to the API Explorer.
        /// </summary>
        /// <param name="action">The <see cref="ActionDescriptor">action</see> to evaluate.</param>
        /// <returns>True if the <paramref name="action"/> is visible; otherwise, false.</returns>
        protected bool IsVisible( ActionDescriptor action )
        {
            var ignoreApiExplorerSettings = !Options.UseApiExplorerSettings;

            if ( ignoreApiExplorerSettings )
            {
                return true;
            }

            return action.GetProperty<ApiExplorerModel>()?.IsODataVisible() ??
                   action.GetProperty<ControllerModel>()?.ApiExplorer?.IsODataVisible() ??
                   false;
        }

        /// <summary>
        /// Populates the API version parameters for the specified API description.
        /// </summary>
        /// <param name="apiDescription">The <see cref="ApiDescription">API description</see> to populate parameters for.</param>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> used to populate parameters with.</param>
        protected virtual void PopulateApiVersionParameters( ApiDescription apiDescription, ApiVersion apiVersion )
        {
            var parameterSource = Options.ApiVersionParameterSource;
            var context = new ApiVersionParameterDescriptionContext( apiDescription, apiVersion, modelMetadata.Value, Options );

            parameterSource.AddParameters( context );
        }

        /// <summary>
        /// Explores the OData query options for the specified API descriptions.
        /// </summary>
        /// <param name="apiDescriptions">The <see cref="IEnumerable{T}">sequence</see> of <see cref="ApiDescription">API descriptions</see> to explore.</param>
        /// <param name="uriResolver">The associated <see cref="ODataUriResolver">OData URI resolver</see>.</param>
        protected virtual void ExploreQueryOptions( IEnumerable<ApiDescription> apiDescriptions, ODataUriResolver uriResolver )
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
                DefaultQuerySettings = DefaultQuerySettings,
                ModelMetadataProvider = MetadataProvider,
            };

            queryOptions.ApplyTo( apiDescriptions, settings );
        }

        static IEnumerable<string> GetHttpMethods( ControllerActionDescriptor action )
        {
            var actionConstraints = ( action.ActionConstraints ?? Array.Empty<IActionConstraintMetadata>() ).OfType<HttpMethodActionConstraint>();
            var httpMethods = new HashSet<string>( actionConstraints.SelectMany( ac => ac.HttpMethods ), StringComparer.OrdinalIgnoreCase );

            if ( httpMethods.Count > 0 )
            {
                return httpMethods;
            }

            for ( var i = 0; i < SupportedHttpMethodConventions.Length; i++ )
            {
                var supportedHttpMethod = SupportedHttpMethodConventions[i];

                if ( action.MethodInfo.Name.StartsWith( supportedHttpMethod, OrdinalIgnoreCase ) )
                {
                    httpMethods.Add( supportedHttpMethod );
                }
            }

            if ( httpMethods.Count == 0 )
            {
                httpMethods.Add( "POST" );
            }

            return httpMethods;
        }

        static Type? GetDeclaredReturnType( ControllerActionDescriptor action )
        {
            var declaredReturnType = action.MethodInfo.ReturnType;

            if ( declaredReturnType == typeof( void ) || declaredReturnType == typeof( Task ) )
            {
                return typeof( void );
            }

            var unwrappedType = declaredReturnType;

            if ( declaredReturnType.IsGenericType && declaredReturnType.GetGenericTypeDefinition() == typeof( Task<> ) )
            {
                unwrappedType = declaredReturnType.GetGenericArguments()[0];
            }

            if ( typeof( IActionResult ).IsAssignableFrom( unwrappedType ) )
            {
                return default;
            }

            return unwrappedType;
        }

        static Type? GetRuntimeReturnType( Type declaredReturnType ) => declaredReturnType == typeof( object ) ? default : declaredReturnType;

        static IReadOnlyList<IApiRequestMetadataProvider>? GetRequestMetadataAttributes( ControllerActionDescriptor action )
        {
            if ( action.FilterDescriptors == null )
            {
                return default;
            }

            return action.FilterDescriptors.Select( fd => fd.Filter ).OfType<IApiRequestMetadataProvider>().ToArray();
        }

        static IReadOnlyList<IApiResponseMetadataProvider>? GetResponseMetadataAttributes( ControllerActionDescriptor action )
        {
            if ( action.FilterDescriptors == null )
            {
                return default;
            }

            return action.FilterDescriptors.Select( fd => fd.Filter ).OfType<IApiResponseMetadataProvider>().ToArray();
        }

        static string? BuildRelativePath( ControllerActionDescriptor action, ODataRouteBuilderContext routeContext )
        {
            var relativePath = action.AttributeRouteInfo?.Template;

            // note: if path happens to be built ahead of time, it's expected to be qualified; rebuild it as necessary
            if ( string.IsNullOrEmpty( relativePath ) || !routeContext.Options.UseQualifiedNames )
            {
                var builder = new ODataRouteBuilder( routeContext );
                relativePath = builder.Build();
            }

            return relativePath;
        }

        IEnumerable<ApiDescription> NewODataApiDescriptions( ControllerActionDescriptor action, string groupName, ODataRouteMapping mapping )
        {
            var requestMetadataAttributes = GetRequestMetadataAttributes( action );
            var responseMetadataAttributes = GetResponseMetadataAttributes( action );
            var declaredReturnType = GetDeclaredReturnType( action );
            var runtimeReturnType = GetRuntimeReturnType( declaredReturnType! );
            var apiResponseTypes = GetApiResponseTypes( responseMetadataAttributes!, runtimeReturnType!, mapping.Services );
            var routeContext = new ODataRouteBuilderContext( mapping, action, Options ) { ModelMetadataProvider = MetadataProvider };

            if ( routeContext.IsRouteExcluded )
            {
                yield break;
            }

            var parameterContext = new ApiParameterContext( MetadataProvider, routeContext, ModelTypeBuilder );
            var parameters = GetParameters( parameterContext );

            for ( var i = 0; i < parameters.Count; i++ )
            {
                routeContext.ParameterDescriptions.Add( parameters[i] );
            }

            var relativePath = BuildRelativePath( action, routeContext );

            parameters = routeContext.ParameterDescriptions;

            foreach ( var httpMethod in GetHttpMethods( action ) )
            {
                var apiDescription = new ApiDescription()
                {
                    ActionDescriptor = action,
                    HttpMethod = httpMethod,
                    RelativePath = relativePath,
                    GroupName = groupName,
                    Properties = { [typeof( IEdmModel )] = routeContext.EdmModel },
                };

                if ( routeContext.EntitySet != null )
                {
                    apiDescription.Properties[typeof( IEdmEntitySet )] = routeContext.EntitySet;
                }

                if ( routeContext.Operation != null )
                {
                    apiDescription.Properties[typeof( IEdmOperation )] = routeContext.Operation;
                }

                for ( var i = 0; i < parameters.Count; i++ )
                {
                    apiDescription.ParameterDescriptions.Add( parameters[i] );
                }

                if ( apiDescription.ParameterDescriptions.Count > 0 )
                {
                    var contentTypes = GetDeclaredContentTypes( requestMetadataAttributes );

                    for ( var i = 0; i < apiDescription.ParameterDescriptions.Count; i++ )
                    {
                        var parameter = apiDescription.ParameterDescriptions[i];

                        if ( parameter.Source == Body )
                        {
                            var requestFormats = GetSupportedFormats( contentTypes, parameter.Type );

                            for ( var j = 0; j < requestFormats.Count; j++ )
                            {
                                apiDescription.SupportedRequestFormats.Add( requestFormats[j] );
                            }
                        }
                        else if ( parameter.Source == FormFile )
                        {
                            for ( var j = 0; j < contentTypes.Count; j++ )
                            {
                                apiDescription.SupportedRequestFormats.Add( new ApiRequestFormat() { MediaType = contentTypes[j] } );
                            }
                        }
                    }
                }

                for ( var i = 0; i < apiResponseTypes.Count; i++ )
                {
                    apiDescription.SupportedResponseTypes.Add( apiResponseTypes[i] );
                }

                PopulateApiVersionParameters( apiDescription, mapping.ApiVersion );
                apiDescription.SetApiVersion( mapping.ApiVersion );
                apiDescription.TryUpdateRelativePathAndRemoveApiVersionParameter( Options );
                yield return apiDescription;
            }
        }

        IList<ApiParameterDescription> GetParameters( ApiParameterContext context )
        {
            var action = context.RouteContext.ActionDescriptor;

            if ( action.Parameters != null )
            {
                for ( var i = 0; i < action.Parameters.Count; i++ )
                {
                    var actionParameter = action.Parameters[i];
                    var metadata = MetadataProvider.GetMetadataForType( actionParameter.ParameterType );

                    UpdateBindingInfo( context, actionParameter, metadata );

                    var visitor = new PseudoModelBindingVisitor( context, actionParameter );
                    var bindingContext = new ApiParameterDescriptionContext( metadata, actionParameter.BindingInfo, propertyName: actionParameter.Name );

                    visitor.WalkParameter( bindingContext );
                }
            }

            if ( action.BoundProperties != null )
            {
                for ( var i = 0; i < action.BoundProperties.Count; i++ )
                {
                    var actionParameter = action.BoundProperties[i];
                    var visitor = new PseudoModelBindingVisitor( context, actionParameter );
                    var modelMetadata = context.MetadataProvider.GetMetadataForProperty(
                        containerType: action.ControllerTypeInfo.AsType(),
                        propertyName: actionParameter.Name );
                    var bindingContext = new ApiParameterDescriptionContext(
                        modelMetadata,
                        actionParameter.BindingInfo,
                        propertyName: actionParameter.Name );

                    visitor.WalkParameter( bindingContext );
                }
            }

            for ( var i = context.Results.Count - 1; i >= 0; i-- )
            {
                if ( !context.Results[i].Source.IsFromRequest )
                {
                    context.Results.RemoveAt( i );
                }
            }

            ProcessRouteParameters( context );

            return context.Results;
        }

        static MediaTypeCollection GetDeclaredContentTypes( IReadOnlyList<IApiRequestMetadataProvider>? requestMetadataAttributes )
        {
            var contentTypes = new MediaTypeCollection();

            if ( requestMetadataAttributes != null )
            {
                for ( var i = 0; i < requestMetadataAttributes.Count; i++ )
                {
                    requestMetadataAttributes[i].SetContentTypes( contentTypes );
                }
            }

            return contentTypes;
        }

        IReadOnlyList<ApiRequestFormat> GetSupportedFormats( MediaTypeCollection contentTypes, Type type )
        {
            if ( contentTypes.Count == 0 )
            {
                contentTypes = new MediaTypeCollection() { default( string ) };
            }

            var results = new List<ApiRequestFormat>( capacity: contentTypes.Count );

            for ( var i = 0; i < contentTypes.Count; i++ )
            {
                for ( var j = 0; j < MvcOptions.InputFormatters.Count; j++ )
                {
                    var formatter = MvcOptions.InputFormatters[j];

                    if ( !( formatter is IApiRequestFormatMetadataProvider requestFormatMetadataProvider ) )
                    {
                        continue;
                    }

                    IReadOnlyList<string>? supportedTypes;

                    try
                    {
                        supportedTypes = requestFormatMetadataProvider.GetSupportedContentTypes( contentTypes[i], type );
                    }
                    catch ( InvalidOperationException )
                    {
                        // BUG: https://github.com/OData/WebApi/issues/1750
                        supportedTypes = null;
                    }

                    if ( supportedTypes == null )
                    {
                        continue;
                    }

                    for ( var k = 0; k < supportedTypes.Count; k++ )
                    {
                        results.Add( new ApiRequestFormat() { Formatter = formatter, MediaType = supportedTypes[k], } );
                    }
                }
            }

            return results.ToArray();
        }

        static void UpdateBindingInfo( ApiParameterContext context, ParameterDescriptor parameter, ModelMetadata metadata )
        {
            var parameterType = parameter.ParameterType;
            var bindingInfo = parameter.BindingInfo;

            static bool IsSpecialBindingSource( BindingInfo info, Type type )
            {
                if ( info == null )
                {
                    return false;
                }

                if ( ( type.IsODataQueryOptions() || type.IsODataPath() ) && info.BindingSource == Custom )
                {
                    info.BindingSource = Special;
                    return true;
                }

                return false;
            }

            if ( IsSpecialBindingSource( bindingInfo, parameterType ) )
            {
                return;
            }

            if ( bindingInfo == null )
            {
                parameter.BindingInfo = bindingInfo = new BindingInfo() { BindingSource = metadata.BindingSource };

                if ( IsSpecialBindingSource( bindingInfo, parameterType ) )
                {
                    return;
                }
            }

            var key = default( IEdmNamedElement );
            var paramName = parameter.Name;
            var source = bindingInfo.BindingSource;

            switch ( context.RouteContext.ActionType )
            {
                case EntitySet:
                    var keys = context.RouteContext.EntitySet.EntityType().Key().ToArray();

                    key = keys.FirstOrDefault( k => k.Name.Equals( paramName, OrdinalIgnoreCase ) );

                    if ( key == null )
                    {
                        var template = context.PathTemplate;

                        if ( template != null )
                        {
                            var segments = template.Segments.OfType<KeySegmentTemplate>();

                            if ( segments.SelectMany( s => s.ParameterMappings.Values ).Any( name => name.Equals( paramName, OrdinalIgnoreCase ) ) )
                            {
                                source = Path;
                            }
                        }
                    }
                    else
                    {
                        source = Path;
                    }

                    break;
                case BoundOperation:
                case UnboundOperation:
                    var operation = context.RouteContext.Operation;

                    if ( operation == null )
                    {
                        break;
                    }

                    key = operation.Parameters.FirstOrDefault( p => p.Name.Equals( paramName, OrdinalIgnoreCase ) );

                    if ( key == null )
                    {
                        if ( operation.IsBound )
                        {
                            goto case EntitySet;
                        }
                    }
                    else
                    {
                        source = Path;
                    }

                    break;
                default:
                    source = Query;
                    break;
            }

            bindingInfo.BindingSource = source ?? Query;
            parameter.BindingInfo = bindingInfo;
        }

        IReadOnlyList<ApiResponseType> GetApiResponseTypes( IReadOnlyList<IApiResponseMetadataProvider> responseMetadataAttributes, Type responseType, IServiceProvider serviceProvider )
        {
            var results = new List<ApiResponseType>();
            var objectTypes = new Dictionary<int, Type>();
            var contentTypes = new MediaTypeCollection();

            if ( responseMetadataAttributes != null )
            {
                for ( var i = 0; i < responseMetadataAttributes.Count; i++ )
                {
                    var metadataAttribute = responseMetadataAttributes[i];

                    metadataAttribute.SetContentTypes( contentTypes );

                    var canInferResponseType = metadataAttribute.Type == typeof( void ) &&
                                               responseType != null &&
                                               ( metadataAttribute.StatusCode == Status200OK || metadataAttribute.StatusCode == Status201Created );

                    if ( canInferResponseType )
                    {
                        objectTypes[metadataAttribute.StatusCode] = responseType!;
                    }
                    else if ( metadataAttribute.Type != null )
                    {
                        objectTypes[metadataAttribute.StatusCode] = metadataAttribute.Type;
                    }
                }
            }

            if ( objectTypes.Count == 0 && responseType != null )
            {
                objectTypes[Status200OK] = responseType;
            }

            var responseTypeMetadataProviders = MvcOptions.OutputFormatters.OfType<IApiResponseTypeMetadataProvider>();

            foreach ( var objectType in objectTypes )
            {
                Type type;

                if ( objectType.Value == typeof( void ) )
                {
                    type = objectType.Value.SubstituteIfNecessary( new TypeSubstitutionContext( serviceProvider, ModelTypeBuilder ) );

                    results.Add( new ApiResponseType()
                    {
                        StatusCode = objectType.Key,
                        Type = type,
                        ModelMetadata = MetadataProvider.GetMetadataForType( objectType.Value ).SubstituteIfNecessary( type ),
                    } );

                    continue;
                }

                type = objectType.Value.SubstituteIfNecessary( new TypeSubstitutionContext( serviceProvider, ModelTypeBuilder ) );

                var apiResponseType = new ApiResponseType()
                {
                    StatusCode = objectType.Key,
                    Type = type,
                    ModelMetadata = MetadataProvider.GetMetadataForType( objectType.Value ).SubstituteIfNecessary( type ),
                };

                for ( var i = 0; i < contentTypes.Count; i++ )
                {
                    foreach ( var responseTypeMetadataProvider in responseTypeMetadataProviders )
                    {
                        IReadOnlyList<string>? formatterSupportedContentTypes;

                        try
                        {
                            formatterSupportedContentTypes = responseTypeMetadataProvider.GetSupportedContentTypes( contentTypes[i], objectType.Value );
                        }
                        catch ( InvalidOperationException )
                        {
                            // BUG: https://github.com/OData/WebApi/issues/1750
                            formatterSupportedContentTypes = null;
                        }

                        if ( formatterSupportedContentTypes == null )
                        {
                            continue;
                        }

                        for ( var j = 0; j < formatterSupportedContentTypes.Count; j++ )
                        {
                            var responseFormat = new ApiResponseFormat()
                            {
                                Formatter = (IOutputFormatter) responseTypeMetadataProvider,
                                MediaType = formatterSupportedContentTypes[j],
                            };

                            apiResponseType.ApiResponseFormats.Add( responseFormat );
                        }
                    }
                }

                results.Add( apiResponseType );
            }

            return results;
        }

        ModelMetadata NewModelMetadata() => new ApiVersionModelMetadata( MetadataProvider, Options.DefaultApiVersionParameterDescription );

        void ProcessRouteParameters( ApiParameterContext context )
        {
            var prefix = context.RouteContext.Route.RoutePrefix;

            if ( string.IsNullOrEmpty( prefix ) )
            {
                return;
            }

            var routeTemplate = TemplateParser.Parse( prefix );
            var routeParameters = new Dictionary<string, ApiParameterRouteInfo>( StringComparer.OrdinalIgnoreCase );

            for ( var i = 0; i < routeTemplate.Parameters.Count; i++ )
            {
                var routeParameter = routeTemplate.Parameters[i];
                routeParameters.Add( routeParameter.Name, CreateRouteInfo( routeParameter ) );
            }

            for ( var i = 0; i < context.Results.Count; i++ )
            {
                var parameter = context.Results[i];

                if ( parameter.Source == Path || parameter.Source == ModelBinding || parameter.Source == Custom )
                {
                    if ( routeParameters.TryGetValue( parameter.Name, out var routeInfo ) )
                    {
                        parameter.RouteInfo = routeInfo;
                        routeParameters.Remove( parameter.Name );

                        if ( parameter.Source == ModelBinding && !parameter.RouteInfo.IsOptional )
                        {
                            parameter.Source = Path;
                        }
                    }
                }
            }

            foreach ( var routeParameter in routeParameters )
            {
                var result = new ApiParameterDescription()
                {
                    Name = routeParameter.Key,
                    RouteInfo = routeParameter.Value,
                    Source = Path,
                };

                context.Results.Add( result );

                if ( !routeParameter.Value.Constraints.OfType<ApiVersionRouteConstraint>().Any() )
                {
                    continue;
                }

                var metadata = NewModelMetadata();

                result.ModelMetadata = metadata;
                result.Type = metadata.ModelType;
            }
        }

        ApiParameterRouteInfo CreateRouteInfo( TemplatePart routeParameter )
        {
            var constraints = new List<IRouteConstraint>();

            if ( routeParameter.InlineConstraints != null )
            {
                foreach ( var constraint in routeParameter.InlineConstraints )
                {
                    constraints.Add( ConstraintResolver.ResolveConstraint( constraint.Constraint ) );
                }
            }

            return new ApiParameterRouteInfo()
            {
                Constraints = constraints,
                DefaultValue = routeParameter.DefaultValue,
                IsOptional = routeParameter.IsOptional || routeParameter.DefaultValue != null,
            };
        }
    }
}