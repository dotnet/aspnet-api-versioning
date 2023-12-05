// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Routing;

using Asp.Versioning.ApiExplorer;
using Asp.Versioning.Description;
using Asp.Versioning.OData;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Description;
using static Microsoft.OData.ODataUrlKeyDelimiter;
using static ODataRouteTemplateGenerationKind;
using static System.Linq.Enumerable;
using static System.StringComparison;

internal sealed class ODataRouteBuilderContext
{
    private readonly ODataRoute route;
    private readonly ODataRouteAttribute? routeAttribute;
    private IODataPathTemplateHandler? templateHandler;

    internal ODataRouteBuilderContext(
        HttpConfiguration configuration,
        ApiVersion apiVersion,
        ODataRoute route,
        HttpActionDescriptor actionDescriptor,
        IList<ApiParameterDescription> parameterDescriptions,
        IModelTypeBuilder modelTypeBuilder,
        ODataApiExplorerOptions options )
    {
        this.route = route;
        ApiVersion = apiVersion;
        Services = configuration.GetODataRootContainer( route );
        routeAttribute = actionDescriptor.GetCustomAttributes<ODataRouteAttribute>().FirstOrDefault();
        RouteTemplate = routeAttribute?.PathTemplate;
        RoutePrefix = route.RoutePrefix?.Trim( '/' );
        ActionDescriptor = actionDescriptor;
        ParameterDescriptions = parameterDescriptions;
        Options = options;
        UrlKeyDelimiter = UrlKeyDelimiterOrDefault(
            configuration.GetUrlKeyDelimiter() ??
            Services.GetService<IODataPathHandler>()?.UrlKeyDelimiter );

        var selector = Services.GetRequiredService<IEdmModelSelector>();
        var model = selector.SelectModel( apiVersion );
        var container = model?.EntityContainer;

        if ( model == null || container == null )
        {
            EdmModel = Services.GetRequiredService<IEdmModel>();
            IsRouteExcluded = true;
            return;
        }

        var controllerName = actionDescriptor.ControllerDescriptor.ControllerName;

        EdmModel = model;
        Services = new FixedEdmModelServiceProviderDecorator( Services, model );
        EntitySet = container.FindEntitySet( controllerName );
        Singleton = container.FindSingleton( controllerName );
        Operation = ResolveOperation( container, actionDescriptor );
        ActionType = GetActionType( actionDescriptor );
        IsRouteExcluded = ActionType == ODataRouteActionType.Unknown &&
                         !actionDescriptor.ControllerDescriptor.ControllerType.IsMetadataController();

        if ( Operation?.IsAction() == true )
        {
            ConvertODataActionParametersToTypedModel( modelTypeBuilder, (IEdmAction) Operation, controllerName );
        }
    }

    internal IODataPathTemplateHandler PathTemplateHandler
    {
        get
        {
            if ( templateHandler == null )
            {
                var conventions = Services.GetRequiredService<IEnumerable<IODataRoutingConvention>>();
                var attribute = conventions.OfType<AttributeRoutingConvention>().FirstOrDefault();
                templateHandler = attribute?.ODataPathTemplateHandler ?? new DefaultODataPathHandler();
            }

            return templateHandler;
        }
    }

    private IEnumerable<string> GetHttpMethods( HttpActionDescriptor action ) =>
        action.GetHttpMethods( route ).Select( m => m.Method );

    private void ConvertODataActionParametersToTypedModel( IModelTypeBuilder modelTypeBuilder, IEdmAction action, string controllerName )
    {
        for ( var i = 0; i < ParameterDescriptions.Count; i++ )
        {
            var description = ParameterDescriptions[i];
            var parameter = description.ParameterDescriptor;

            if ( parameter != null && parameter.ParameterType.IsODataActionParameters() )
            {
                var selector = Services.GetRequiredService<IEdmModelSelector>();
                var model = selector.SelectModel( ApiVersion )!;
                var parameterType = modelTypeBuilder.NewActionParameters( model, action, controllerName, ApiVersion );
                description.ParameterDescriptor = new ODataModelBoundParameterDescriptor( parameter, parameterType );
                break;
            }
        }
    }

    private static IList<HttpParameterDescriptor> FilterParameters( HttpActionDescriptor action )
    {
        var parameters = action.GetParameters().ToList();
        var cancellationToken = typeof( CancellationToken );

        for ( var i = parameters.Count - 1; i >= 0; i-- )
        {
            var type = parameters[i].ParameterType;

            if ( type.IsODataQueryOptions() ||
                 type.IsODataActionParameters() ||
                 type.IsODataPath() ||
                 type.Equals( cancellationToken ) )
            {
                parameters.RemoveAt( i );
            }
        }

        return parameters;
    }

    internal IServiceProvider Services { get; }

    internal ApiVersion ApiVersion { get; }

    internal ODataApiExplorerOptions Options { get; }

    internal IList<ApiParameterDescription> ParameterDescriptions { get; }

    internal ODataRouteTemplateGenerationKind RouteTemplateGeneration => Client;

    internal IEdmModel EdmModel { get; }

    internal string? RouteTemplate { get; }

    internal string? RoutePrefix { get; }

    internal HttpActionDescriptor ActionDescriptor { get; }

    internal IEdmEntitySet? EntitySet { get; }

    internal IEdmSingleton? Singleton { get; }

    internal IEdmOperation? Operation { get; }

    internal ODataRouteActionType ActionType { get; }

    internal ODataUrlKeyDelimiter UrlKeyDelimiter { get; }

    internal bool IsRouteExcluded { get; }

    internal bool IsAttributeRouted => routeAttribute != null;

    internal bool IsOperation => Operation != null;

    internal bool IsBound => IsOperation && ( EntitySet != null || Singleton != null );

    internal bool AllowUnqualifiedEnum => Services.GetRequiredService<ODataUriResolver>() is StringAsEnumResolver;

    internal ODataRouteActionType GetActionType( HttpActionDescriptor action )
    {
        if ( EntitySet == null && Singleton == null )
        {
            if ( Operation == null )
            {
                return ODataRouteActionType.Unknown;
            }
            else if ( !Operation.IsBound )
            {
                return ODataRouteActionType.UnboundOperation;
            }
        }
        else if ( Operation == null )
        {
            var httpMethods = GetHttpMethods( action );

            if ( IsCast( EdmModel, EntitySet, action.ActionName, httpMethods ) )
            {
                return ODataRouteActionType.EntitySet;
            }
            else if ( IsActionOrFunction( EntitySet, Singleton, action.ActionName, httpMethods ) )
            {
                return ODataRouteActionType.Unknown;
            }
            else if ( Singleton == null )
            {
                return ODataRouteActionType.EntitySet;
            }
            else
            {
                return ODataRouteActionType.Singleton;
            }
        }

        if ( Operation.IsBound )
        {
            return ODataRouteActionType.BoundOperation;
        }

        return ODataRouteActionType.UnboundOperation;
    }

    private static bool IsCast( IEdmModel model, IEdmEntitySet? entitySet, string actionName, IEnumerable<string> methods )
    {
        using var iterator = methods.GetEnumerator();

        if ( !iterator.MoveNext() )
        {
            return false;
        }

        var method = iterator.Current;

        if ( iterator.MoveNext() )
        {
            return false;
        }

        if ( entitySet == null )
        {
            return false;
        }

        var entity = entitySet.EntityType();

        const string ActionMethod = "Post";
        const string FunctionMethod = "Get";

        if ( ( FunctionMethod.Equals( method, OrdinalIgnoreCase ) ||
               ActionMethod.Equals( method, OrdinalIgnoreCase ) ) &&
             actionName != ActionMethod )
        {
            foreach ( var derivedType in model.FindAllDerivedTypes( entity ).OfType<EdmEntityType>() )
            {
                var fromTypeName = "From" + derivedType.Name;

                if ( actionName.StartsWith( method + fromTypeName, OrdinalIgnoreCase ) ||
                     actionName.StartsWith( method + entitySet.Name + fromTypeName, OrdinalIgnoreCase ) ||
                     actionName.StartsWith( method + derivedType.Name, OrdinalIgnoreCase ) )
                {
                    return true;
                }
            }
        }

        return false;
    }

    // Slash became the default 4/18/2018
    // REF: https://github.com/OData/WebApi/pull/1393
    private static ODataUrlKeyDelimiter UrlKeyDelimiterOrDefault( ODataUrlKeyDelimiter? urlKeyDelimiter ) => urlKeyDelimiter ?? Slash;

    // REF: https://github.com/OData/WebApi/blob/master/src/Microsoft.AspNet.OData.Shared/Routing/Conventions/ActionRoutingConvention.cs
    // REF: https://github.com/OData/WebApi/blob/master/src/Microsoft.AspNet.OData.Shared/Routing/Conventions/FunctionRoutingConvention.cs
    // REF: https://github.com/OData/WebApi/blob/master/src/Microsoft.AspNet.OData.Shared/Routing/Conventions/EntitySetRoutingConvention.cs
    // REF: https://github.com/OData/WebApi/blob/master/src/Microsoft.AspNet.OData.Shared/Routing/Conventions/EntityRoutingConvention.cs
    // REF: https://github.com/OData/WebApi/blob/master/src/Microsoft.AspNet.OData.Shared/Routing/Conventions/SingletonRoutingConvention.cs
    private static bool IsActionOrFunction( IEdmEntitySet? entitySet, IEdmSingleton? singleton, string actionName, IEnumerable<string> methods )
    {
        using var iterator = methods.GetEnumerator();

        if ( !iterator.MoveNext() )
        {
            return false;
        }

        var method = iterator.Current;

        if ( iterator.MoveNext() )
        {
            return false;
        }

        if ( entitySet == null && singleton == null )
        {
            return true;
        }

        const string ActionMethod = "Post";
        const string AddNavigationLink = ActionMethod + "To";
        const string FunctionMethod = "Get";

        if ( ActionMethod.Equals( method, OrdinalIgnoreCase ) && actionName != ActionMethod )
        {
            if ( actionName.StartsWith( "CreateRef", Ordinal ) ||
               ( entitySet != null && actionName == ( ActionMethod + entitySet.Name ) ) )
            {
                return false;
            }

            return !IsNavigationPropertyLink( entitySet, singleton, actionName, ActionMethod, AddNavigationLink );
        }
        else if ( FunctionMethod.Equals( method, OrdinalIgnoreCase ) && actionName != FunctionMethod )
        {
            if ( actionName.StartsWith( "GetRef", Ordinal ) ||
               ( entitySet != null && actionName == ( FunctionMethod + entitySet.Name ) ) )
            {
                return false;
            }

            return !IsNavigationPropertyLink( entitySet, singleton, actionName, FunctionMethod );
        }

        return false;
    }

    private static bool IsNavigationPropertyLink( IEdmEntitySet? entitySet, IEdmSingleton? singleton, string actionName, params string[] methods )
    {
        var entities = new List<IEdmEntityType>( capacity: 2 );

        if ( entitySet != null )
        {
            entities.Add( entitySet.EntityType() );
        }

        if ( singleton != null )
        {
            var entity = singleton.EntityType();

            if ( entities.Count == 0 || !entities[0].Equals( entity ) )
            {
                entities.Add( entity );
            }
        }

        var propertyNames = default( List<string> );

        for ( var i = 0; i < entities.Count; i++ )
        {
            var entity = entities[i];

            for ( var j = 0; j < methods.Length; j++ )
            {
                var method = methods[j];

                if ( actionName == ( method + entity.Name ) )
                {
                    return true;
                }

                if ( j == 0 )
                {
                    if ( propertyNames is null )
                    {
                        propertyNames = [];
                    }
                    else
                    {
                        propertyNames.Clear();
                    }

                    foreach ( var property in entity.NavigationProperties() )
                    {
                        if ( actionName.StartsWith( method + property.Name, OrdinalIgnoreCase ) )
                        {
                            return true;
                        }

                        propertyNames.Add( property.Name );
                    }
                }
                else if ( propertyNames is not null )
                {
                    for ( var k = 0; k < propertyNames.Count; k++ )
                    {
                        if ( actionName.StartsWith( method + propertyNames[k], OrdinalIgnoreCase ) )
                        {
                            return true;
                        }
                    }
                }
            }
        }

        return false;
    }

    private IEdmOperation? ResolveOperation( IEdmEntityContainer container, HttpActionDescriptor action )
    {
        if ( container.FindOperationImports( action.ActionName ).SingleOrDefault() is IEdmOperationImport import )
        {
            return import.Operation;
        }

        var qualifiedName = container.Namespace + "." + action.ActionName;

        if ( Singleton is not null )
        {
            return EdmModel.FindBoundOperations( qualifiedName, Singleton.EntityType() ).SingleOrDefault();
        }

        if ( EntitySet is null )
        {
            return default;
        }

        var operation = EdmModel.FindBoundOperations( qualifiedName, EntitySet.Type ).SingleOrDefault();

        if ( operation is not null && HasNoEntityKeysRemaining( operation, FilterParameters( action ) ) )
        {
            return operation;
        }

        return EdmModel.FindBoundOperations( qualifiedName, EntitySet.EntityType() ).SingleOrDefault();
    }

    private static bool HasNoEntityKeysRemaining( IEdmOperation operation, IList<HttpParameterDescriptor> parameters )
    {
        var actionParamCount = parameters.Count;

        if ( operation.IsAction() )
        {
            return actionParamCount == 0;
        }
        else if ( !operation.IsFunction() )
        {
            return false;
        }

        var operationParamCount = 0;
        var matches = 0;

        foreach ( var parameter in operation.Parameters )
        {
            if ( parameter.Name == "bindingParameter" )
            {
                continue;
            }

            ++operationParamCount;

            for ( var i = 0; i < parameters.Count; i++ )
            {
#if NETFRAMEWORK
                var name = parameters[i].ParameterName;
#else
                var name = parameters[i].Name;
#endif
                if ( name.Equals( parameter.Name, OrdinalIgnoreCase ) )
                {
                    ++matches;
                    parameters.RemoveAt( i );
                    break;
                }
            }
        }

        return operationParamCount == matches &&
               operationParamCount == actionParamCount;
    }

    private sealed class FixedEdmModelServiceProviderDecorator : IServiceProvider
    {
        private readonly IServiceProvider decorated;
        private readonly IEdmModel edmModel;

        internal FixedEdmModelServiceProviderDecorator( IServiceProvider decorated, IEdmModel edmModel )
        {
            this.decorated = decorated;
            this.edmModel = edmModel;
        }

        public object GetService( Type serviceType ) =>
            serviceType == typeof( IEdmModel ) ? edmModel : decorated.GetService( serviceType );
    }
}