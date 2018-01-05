namespace Microsoft.Web.Http.Description
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.OData;
    using Microsoft.OData.Edm;
    using Microsoft.OData.UriParser;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Web.Http;
    using System.Web.Http.Controllers;
    using System.Web.Http.Description;
    using System.Web.Http.Dispatcher;
    using System.Web.OData;
    using System.Web.OData.Routing;
    using static System.Linq.Enumerable;

    sealed class ODataRouteBuilderContext
    {
        readonly IServiceProvider serviceProvider;
        readonly ODataRouteAttribute routeAttribute;

        internal ODataRouteBuilderContext(
            HttpConfiguration configuration,
            ODataRoute route,
            HttpActionDescriptor actionDescriptor,
            IReadOnlyList<ApiParameterDescription> parameterDescriptions,
            ModelTypeBuilder modelTypeBuilder,
            ODataApiExplorerOptions options )
        {
            Contract.Requires( configuration != null );
            Contract.Requires( route != null );
            Contract.Requires( actionDescriptor != null );
            Contract.Requires( parameterDescriptions != null );
            Contract.Requires( modelTypeBuilder != null );
            Contract.Requires( options != null );

            serviceProvider = configuration.GetODataRootContainer( route );
            EdmModel = serviceProvider.GetRequiredService<IEdmModel>();
            AssembliesResolver = configuration.Services.GetAssembliesResolver();
            routeAttribute = actionDescriptor.GetCustomAttributes<ODataRouteAttribute>().FirstOrDefault();
            RouteTemplate = routeAttribute?.PathTemplate;
            Route = route;
            ActionDescriptor = actionDescriptor;
            ParameterDescriptions = parameterDescriptions;
            Options = options;
            UrlKeyDelimiter = configuration.GetUrlKeyDelimiter();

            var container = EdmModel.EntityContainer;

            if ( container == null )
            {
                IsRouteExcluded = true;
                return;
            }

            EntitySet = container.FindEntitySet( actionDescriptor.ControllerDescriptor.ControllerName );
            Operation = container.FindOperationImports( actionDescriptor.ActionName ).FirstOrDefault()?.Operation ??
                        EdmModel.FindDeclaredOperations( container.Namespace + "." + actionDescriptor.ActionName ).FirstOrDefault();
            ActionType = GetActionType( EntitySet, Operation );

            if ( Operation?.IsAction() == true )
            {
                ConvertODataActionParametersToTypedModel( modelTypeBuilder, (IEdmAction) Operation );
            }
        }

        internal IAssembliesResolver AssembliesResolver { get; }

        internal IEdmModel EdmModel { get; }

        internal string RouteTemplate { get; }

        internal ODataRoute Route { get; }

        internal HttpActionDescriptor ActionDescriptor { get; }

        internal IReadOnlyList<ApiParameterDescription> ParameterDescriptions { get; }

        internal IEdmEntitySet EntitySet { get; }

        internal IEdmOperation Operation { get; }

        internal ODataRouteActionType ActionType { get; }

        internal ODataApiExplorerOptions Options { get; }

        internal ODataUrlKeyDelimiter UrlKeyDelimiter { get; }

        internal bool IsRouteExcluded { get; }

        internal bool IsAttributeRouted => routeAttribute != null;

        internal bool IsOperation => Operation != null;

        internal bool IsBound => IsOperation && EntitySet != null;

        internal bool AllowUnqualifiedEnum => serviceProvider.GetRequiredService<ODataUriResolver>() is StringAsEnumResolver;

        static ODataRouteActionType GetActionType( IEdmEntitySet entitySet, IEdmOperation operation )
        {
            if ( entitySet == null )
            {
                if ( operation == null )
                {
                    return ODataRouteActionType.Unknown;
                }
                else if ( !operation.IsBound )
                {
                    return ODataRouteActionType.UnboundOperation;
                }
            }
            else
            {
                if ( operation == null )
                {
                    return ODataRouteActionType.EntitySet;
                }
                else if ( operation.IsBound )
                {
                    return ODataRouteActionType.BoundOperation;
                }
            }

            return ODataRouteActionType.Unknown;
        }

        void ConvertODataActionParametersToTypedModel( ModelTypeBuilder modelTypeBuilder, IEdmAction action )
        {
            Contract.Requires( modelTypeBuilder != null );
            Contract.Requires( action != null );

            var actionParameters = typeof( ODataActionParameters );
            var apiVersion = new Lazy<ApiVersion>( () => EdmModel.GetAnnotationValue<ApiVersionAnnotation>( EdmModel ).ApiVersion );

            for ( var i = 0; i < ParameterDescriptions.Count; i++ )
            {
                var description = ParameterDescriptions[i];
                var parameter = description.ParameterDescriptor;

                if ( actionParameters.IsAssignableFrom( parameter.ParameterType ) )
                {
                    description.ParameterDescriptor = new ODataModelBoundParameterDescriptor( parameter, modelTypeBuilder.NewActionParameters( action, apiVersion.Value ) );
                    break;
                }
            }
        }
    }
}