namespace Microsoft.AspNet.OData.Routing
{
    using Microsoft.AspNet.OData;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.OData;
    using Microsoft.OData.Edm;
    using Microsoft.Web.Http;
    using Microsoft.Web.Http.Description;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Web.Http;
    using System.Web.Http.Controllers;
    using System.Web.Http.Description;
    using static Microsoft.OData.ODataUrlKeyDelimiter;
    using static System.Linq.Enumerable;

    partial class ODataRouteBuilderContext
    {
        internal ODataRouteBuilderContext(
            HttpConfiguration configuration,
            ApiVersion apiVersion,
            ODataRoute route,
            HttpActionDescriptor actionDescriptor,
            IList<ApiParameterDescription> parameterDescriptions,
            IModelTypeBuilder modelTypeBuilder,
            ODataApiExplorerOptions options )
        {
            Contract.Requires( configuration != null );
            Contract.Requires( apiVersion != null );
            Contract.Requires( route != null );
            Contract.Requires( actionDescriptor != null );
            Contract.Requires( parameterDescriptions != null );
            Contract.Requires( modelTypeBuilder != null );
            Contract.Requires( options != null );

            ApiVersion = apiVersion;
            serviceProvider = configuration.GetODataRootContainer( route );
            EdmModel = serviceProvider.GetRequiredService<IEdmModel>();
            Assemblies = configuration.Services.GetAssembliesResolver().GetAssemblies();
            routeAttribute = actionDescriptor.GetCustomAttributes<ODataRouteAttribute>().FirstOrDefault();
            RouteTemplate = routeAttribute?.PathTemplate;
            Route = route;
            ActionDescriptor = actionDescriptor;
            ParameterDescriptions = parameterDescriptions;
            Options = options;
            UrlKeyDelimiter = configuration.GetUrlKeyDelimiter() ?? Parentheses;

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

        void ConvertODataActionParametersToTypedModel( IModelTypeBuilder modelTypeBuilder, IEdmAction action )
        {
            Contract.Requires( modelTypeBuilder != null );
            Contract.Requires( action != null );

            var apiVersion = new Lazy<ApiVersion>( () => EdmModel.GetAnnotationValue<ApiVersionAnnotation>( EdmModel ).ApiVersion );

            for ( var i = 0; i < ParameterDescriptions.Count; i++ )
            {
                var description = ParameterDescriptions[i];
                var parameter = description.ParameterDescriptor;

                if ( parameter.ParameterType.IsODataActionParameters() )
                {
                    description.ParameterDescriptor = new ODataModelBoundParameterDescriptor( parameter, modelTypeBuilder.NewActionParameters( serviceProvider, action, apiVersion.Value ) );
                    break;
                }
            }
        }
    }
}