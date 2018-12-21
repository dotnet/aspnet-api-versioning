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
    using Microsoft.AspNetCore.Mvc.ApplicationParts;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Mvc.Formatters;
    using Microsoft.AspNetCore.Mvc.Internal;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
    using Microsoft.AspNetCore.Mvc.Routing;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.AspNetCore.Routing.Template;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Microsoft.OData.Edm;
    using Microsoft.OData.UriParser;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
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
            Arg.NotNull( routeCollectionProvider, nameof( routeCollectionProvider ) );
            Arg.NotNull( metadataProvider, nameof( metadataProvider ) );
            Arg.NotNull( options, nameof( options ) );
            Arg.NotNull( mvcOptions, nameof( mvcOptions ) );

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
        /// Gets the order prescendence of the current API description provider.
        /// </summary>
        /// <value>The order prescendence of the current API description provider. The default value is -100.</value>
        public virtual int Order => AfterApiVersioning;

        /// <summary>
        /// Occurs after the providers have been executed.
        /// </summary>
        /// <param name="context">The current <see cref="ApiDescriptionProviderContext">execution context</see>.</param>
        /// <remarks>The default implementation performs no action.</remarks>
        public virtual void OnProvidersExecuted( ApiDescriptionProviderContext context )
        {
            var ignoreApiExplorerSettings = !Options.UseApiExplorerSettings;
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
                    foreach ( var mapping in mappings )
                    {
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
                    foreach ( var apiVersion in model.DeclaredApiVersions )
                    {
                        var groupName = apiVersion.ToString( groupNameFormat, formatProvider );

                        if ( !mappings.TryGetValue( apiVersion, out var mappingsPerApiVersion ) )
                        {
                            continue;
                        }

                        foreach ( var mapping in mappingsPerApiVersion )
                        {
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
            Arg.NotNull( action, nameof( action ) );

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
            Arg.NotNull( apiDescription, nameof( apiDescription ) );
            Arg.NotNull( apiVersion, nameof( apiVersion ) );

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
            Arg.NotNull( apiDescriptions, nameof( apiDescriptions ) );
            Arg.NotNull( uriResolver, nameof( uriResolver ) );

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
            Contract.Requires( action != null );

            var actionConstraints = ( action.ActionConstraints ?? Array.Empty<IActionConstraintMetadata>() ).OfType<HttpMethodActionConstraint>();
            var httpMethods = new HashSet<string>( actionConstraints.SelectMany( ac => ac.HttpMethods ), StringComparer.OrdinalIgnoreCase );

            if ( httpMethods.Count > 0 )
            {
                return httpMethods;
            }

            foreach ( var supportedHttpMethod in SupportedHttpMethodConventions )
            {
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

        static Type GetDeclaredReturnType( ControllerActionDescriptor action )
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

        static Type GetRuntimeReturnType( Type declaredReturnType ) => declaredReturnType == typeof( object ) ? default : declaredReturnType;

        static IReadOnlyList<IApiRequestMetadataProvider> GetRequestMetadataAttributes( ControllerActionDescriptor action )
        {
            Contract.Requires( action != null );

            if ( action.FilterDescriptors == null )
            {
                return default;
            }

            return action.FilterDescriptors.Select( fd => fd.Filter ).OfType<IApiRequestMetadataProvider>().ToArray();
        }

        static IReadOnlyList<IApiResponseMetadataProvider> GetResponseMetadataAttributes( ControllerActionDescriptor action )
        {
            Contract.Requires( action != null );

            if ( action.FilterDescriptors == null )
            {
                return default;
            }

            return action.FilterDescriptors.Select( fd => fd.Filter ).OfType<IApiResponseMetadataProvider>().ToArray();
        }

        static string BuildRelativePath( ControllerActionDescriptor action, ODataRouteBuilderContext routeContext )
        {
            Contract.Requires( action != null );
            Contract.Requires( routeContext != null );
            Contract.Ensures( !string.IsNullOrEmpty( Contract.Result<string>() ) );

            var relativePath = action.AttributeRouteInfo?.Template;

            // note: if path happens to be built adhead of time, it's expected to be qualified; rebuild it as necessary
            if ( string.IsNullOrEmpty( relativePath ) || !routeContext.Options.UseQualifiedOperationNames )
            {
                var builder = new ODataRouteBuilder( routeContext );
                relativePath = builder.Build();
            }

            return relativePath;
        }

        IEnumerable<ApiDescription> NewODataApiDescriptions( ControllerActionDescriptor action, string groupName, ODataRouteMapping mapping )
        {
            Contract.Requires( action != null );
            Contract.Requires( mapping != null );
            Contract.Ensures( Contract.Result<ApiDescription>() != null );

            var requestMetadataAttributes = GetRequestMetadataAttributes( action );
            var responseMetadataAttributes = GetResponseMetadataAttributes( action );
            var declaredReturnType = GetDeclaredReturnType( action );
            var runtimeReturnType = GetRuntimeReturnType( declaredReturnType );
            var apiResponseTypes = GetApiResponseTypes( responseMetadataAttributes, runtimeReturnType, mapping.Services );
            var routeContext = new ODataRouteBuilderContext( mapping, action, Options );
            var parameterContext = new ApiParameterContext( MetadataProvider, routeContext, ModelTypeBuilder );
            var parameters = GetParameters( parameterContext );

            if ( routeContext.IsRouteExcluded )
            {
                yield break;
            }

            foreach ( var parameter in parameters )
            {
                routeContext.ParameterDescriptions.Add( parameter );
            }

            var relativePath = BuildRelativePath( action, routeContext );

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

                foreach ( var parameter in parameters )
                {
                    apiDescription.ParameterDescriptions.Add( parameter );
                }

                foreach ( var apiResponseType in apiResponseTypes )
                {
                    apiDescription.SupportedResponseTypes.Add( apiResponseType );
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
                foreach ( var actionParameter in action.Parameters )
                {
                    var metadata = MetadataProvider.GetMetadataForType( actionParameter.ParameterType );

                    UpdateBindingInfo( context, actionParameter, metadata );

                    var visitor = new PseudoModelBindingVisitor( context, actionParameter );
                    var bindingContext = new ApiParameterDescriptionContext( metadata, actionParameter.BindingInfo, propertyName: actionParameter.Name );

                    visitor.WalkParameter( bindingContext );
                }
            }

            if ( action.BoundProperties != null )
            {
                foreach ( var actionParameter in action.BoundProperties )
                {
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

        void UpdateBindingInfo( ApiParameterContext context, ParameterDescriptor parameter, ModelMetadata metadata )
        {
            Contract.Requires( context != null );
            Contract.Requires( parameter != null );
            Contract.Requires( metadata != null );

            var parameterType = parameter.ParameterType;
            var bindingInfo = parameter.BindingInfo;

            if ( bindingInfo != null )
            {
                if ( ( parameterType.IsODataQueryOptions() || parameterType.IsODataPath() ) && bindingInfo.BindingSource == Custom )
                {
                    bindingInfo.BindingSource = Special;
                }

                return;
            }

            parameter.BindingInfo = bindingInfo = new BindingInfo() { BindingSource = metadata.BindingSource };

            if ( bindingInfo.BindingSource != null )
            {
                if ( ( parameterType.IsODataQueryOptions() || parameterType.IsODataPath() ) && bindingInfo.BindingSource == Custom )
                {
                    bindingInfo.BindingSource = Special;
                }

                return;
            }

            var key = default( IEdmNamedElement );
            var paramName = parameter.Name;
            var source = Query;

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
            }

            bindingInfo.BindingSource = source;
            parameter.BindingInfo = bindingInfo;
        }

        IReadOnlyList<ApiResponseType> GetApiResponseTypes( IReadOnlyList<IApiResponseMetadataProvider> responseMetadataAttributes, Type responseType, IServiceProvider serviceProvider )
        {
            Contract.Requires( responseMetadataAttributes != null );
            Contract.Requires( responseType != null );
            Contract.Requires( serviceProvider != null );
            Contract.Ensures( Contract.Result<IReadOnlyList<ApiResponseType>>() != null );

            var results = new List<ApiResponseType>();
            var objectTypes = new Dictionary<int, Type>();
            var contentTypes = new MediaTypeCollection();

            if ( responseMetadataAttributes != null )
            {
                foreach ( var metadataAttribute in responseMetadataAttributes )
                {
                    metadataAttribute.SetContentTypes( contentTypes );

                    var canInferResponseType = metadataAttribute.Type == typeof( void ) &&
                                               responseType != null &&
                                               ( metadataAttribute.StatusCode == Status200OK || metadataAttribute.StatusCode == Status201Created );

                    if ( canInferResponseType )
                    {
                        objectTypes[metadataAttribute.StatusCode] = responseType;
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
                if ( objectType.Value == typeof( void ) )
                {
                    results.Add( new ApiResponseType()
                    {
                        StatusCode = objectType.Key,
                        Type = objectType.Value.SubstituteIfNecessary( new TypeSubstitutionContext( serviceProvider, ModelTypeBuilder ) ),
                    } );

                    continue;
                }

                var apiResponseType = new ApiResponseType()
                {
                    StatusCode = objectType.Key,
                    Type = objectType.Value.SubstituteIfNecessary( new TypeSubstitutionContext( serviceProvider, ModelTypeBuilder ) ),
                    ModelMetadata = MetadataProvider.GetMetadataForType( objectType.Value ),
                };

                foreach ( var contentType in contentTypes )
                {
                    foreach ( var responseTypeMetadataProvider in responseTypeMetadataProviders )
                    {
                        var formatterSupportedContentTypes = default( IReadOnlyList<string> );

                        try
                        {
                            formatterSupportedContentTypes = responseTypeMetadataProvider.GetSupportedContentTypes( contentType, objectType.Value );
                        }
                        catch ( InvalidOperationException )
                        {
                        }

                        if ( formatterSupportedContentTypes == null )
                        {
                            continue;
                        }

                        foreach ( var formatterSupportedContentType in formatterSupportedContentTypes )
                        {
                            apiResponseType.ApiResponseFormats.Add( new ApiResponseFormat()
                            {
                                Formatter = (IOutputFormatter) responseTypeMetadataProvider,
                                MediaType = formatterSupportedContentType,
                            } );
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

            foreach ( var routeParameter in routeTemplate.Parameters )
            {
                routeParameters.Add( routeParameter.Name, CreateRouteInfo( routeParameter ) );
            }

            foreach ( var parameter in context.Results )
            {
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