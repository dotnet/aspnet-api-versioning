﻿namespace Microsoft.AspNetCore.Mvc.ApiExplorer
{
    using Microsoft.AspNet.OData;
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
    using Microsoft.AspNetCore.Mvc.Versioning;
    using Microsoft.Extensions.Options;
    using Microsoft.OData.Edm;
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
        /// <param name="partManager">The <see cref="ApplicationPartManager">application part manager</see> containing the configured application parts.</param>
        /// <param name="metadataProvider">The <see cref="IModelMetadataProvider">provider</see> used to retrieve model metadata.</param>
        /// <param name="options">The <see cref="IOptions{TOptions}">container</see> of configured <see cref="ODataApiExplorerOptions">API explorer options</see>.</param>
        /// <param name="mvcOptions">A <see cref="IOptions{TOptions}">holder</see> containing the current <see cref="Mvc.MvcOptions">MVC options</see>.</param>
        public ODataApiDescriptionProvider(
            IODataRouteCollectionProvider routeCollectionProvider,
            ApplicationPartManager partManager,
            IModelMetadataProvider metadataProvider,
            IOptions<ODataApiExplorerOptions> options,
            IOptions<MvcOptions> mvcOptions )
        {
            Arg.NotNull( routeCollectionProvider, nameof( routeCollectionProvider ) );
            Arg.NotNull( partManager, nameof( partManager ) );
            Arg.NotNull( metadataProvider, nameof( metadataProvider ) );
            Arg.NotNull( options, nameof( options ) );
            Arg.NotNull( mvcOptions, nameof( mvcOptions ) );

            RouteCollectionProvider = routeCollectionProvider;
            Assemblies = partManager.ApplicationParts.OfType<AssemblyPart>().Select( p => p.Assembly ).ToArray();
            TypeBuilder = new ModelTypeBuilder( Assemblies );
            MetadataProvider = metadataProvider;
            this.options = options;
            MvcOptions = mvcOptions.Value;
            modelMetadata = new Lazy<ModelMetadata>( NewModelMetadata );
        }

        IEnumerable<Assembly> Assemblies { get; }

        ModelTypeBuilder TypeBuilder { get; }

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
                if ( !action.ControllerTypeInfo.IsODataController() || action.ControllerTypeInfo.IsMetadataController() )
                {
                    continue;
                }

                var controller = action.GetProperty<ControllerModel>();
                var apiExplorer = controller?.ApiExplorer;
                var visible = ignoreApiExplorerSettings || ( apiExplorer?.IsVisible ?? true );

                if ( !visible )
                {
                    continue;
                }

                var model = GetModel( action );

                if ( model.IsApiVersionNeutral )
                {
                    foreach ( var mapping in mappings )
                    {
                        var groupName = mapping.ApiVersion.ToString( groupNameFormat, formatProvider );

                        foreach ( var apiDescription in NewODataApiDescriptions( action, groupName, mapping ) )
                        {
                            results.Add( apiDescription );
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
                            foreach ( var apiDescription in NewODataApiDescriptions( action, groupName, mapping ) )
                            {
                                results.Add( apiDescription );
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
        /// Populates the API version parameters for the specified API description.
        /// </summary>
        /// <param name="apiDescription">The <see cref="ApiDescription">API description</see> to populate parameters for.</param>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> used to populate parameters with.</param>
        protected virtual void PopulateApiVersionParameters( ApiDescription apiDescription, ApiVersion apiVersion )
        {
            Arg.NotNull( apiDescription, nameof( apiDescription ) );
            Arg.NotNull( apiVersion, nameof( apiVersion ) );

            var action = apiDescription.ActionDescriptor;
            var model = action.GetProperty<ApiVersionModel>();

            if ( model.IsApiVersionNeutral )
            {
                return;
            }
            else if ( model.DeclaredApiVersions.Count == 0 )
            {
                model = action.GetProperty<ControllerModel>()?.GetProperty<ApiVersionModel>();

                if ( model?.IsApiVersionNeutral == true )
                {
                    return;
                }
            }

            var parameterSource = Options.ApiVersionParameterSource;
            var context = new ApiVersionParameterDescriptionContext( apiDescription, apiVersion, modelMetadata.Value, Options );

            parameterSource.AddParameters( context );
        }

        static ApiVersionModel GetModel( ControllerActionDescriptor action )
        {
            Contract.Requires( action != null );

            var model = action.GetProperty<ApiVersionModel>();

            if ( model == null || model.DeclaredApiVersions.Count == 0 )
            {
                model = action.GetProperty<ControllerModel>()?.GetProperty<ApiVersionModel>();
            }

            return model;
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
            var routeContext = new ODataRouteBuilderContext( mapping, action, Assemblies, Options );
            var parameterContext = new ApiParameterContext( MetadataProvider, routeContext, TypeBuilder );
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
                };

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
                    UpdateBindingInfo( context, actionParameter );

                    var visitor = new PseudoModelBindingVisitor( context, actionParameter );
                    var metadata = MetadataProvider.GetMetadataForType( actionParameter.ParameterType );
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

            return context.Results;
        }

        void UpdateBindingInfo( ApiParameterContext context, ParameterDescriptor parameter )
        {
            Contract.Requires( context != null );
            Contract.Requires( parameter != null );

            if ( parameter.BindingInfo != null )
            {
                return;
            }

            var paramType = parameter.ParameterType;

            if ( paramType.IsODataQueryOptions() || paramType.IsODataPath() )
            {
                return;
            }
            else if ( paramType.IsDelta() )
            {
                parameter.BindingInfo = new BindingInfo() { BindingSource = Body };
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

                    if ( paramType.IsODataActionParameters() )
                    {
                        source = Body;
                    }
                    else
                    {
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
                    }

                    break;
            }

            parameter.BindingInfo = new BindingInfo() { BindingSource = source };
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
                        Type = objectType.Value.SubstituteIfNecessary( serviceProvider, Assemblies, TypeBuilder ),
                    } );

                    continue;
                }

                var apiResponseType = new ApiResponseType()
                {
                    StatusCode = objectType.Key,
                    Type = objectType.Value.SubstituteIfNecessary( serviceProvider, Assemblies, TypeBuilder ),
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
    }
}