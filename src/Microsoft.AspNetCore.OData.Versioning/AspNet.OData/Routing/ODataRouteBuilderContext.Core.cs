namespace Microsoft.AspNet.OData.Routing
{
    using Microsoft.AspNet.OData;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Mvc.Versioning;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.OData;
    using Microsoft.OData.Edm;
    using System;
    using System.Diagnostics.Contracts;
    using System.Reflection;
    using static System.Linq.Enumerable;

    partial class ODataRouteBuilderContext
    {
        private IODataPathTemplateHandler templateHandler;

        internal ODataRouteBuilderContext(
            ODataRouteMapping routeMapping,
            ControllerActionDescriptor actionDescriptor,
            ODataApiVersioningOptions options )
        {
            Contract.Requires( routeMapping != null );
            Contract.Requires( actionDescriptor != null );
            Contract.Requires( options != null );

            ApiVersion = routeMapping.ApiVersion;
            serviceProvider = routeMapping.Services;
            EdmModel = serviceProvider.GetRequiredService<IEdmModel>();
            routeAttribute = actionDescriptor.MethodInfo.GetCustomAttributes<ODataRouteAttribute>().FirstOrDefault();
            RouteTemplate = routeAttribute?.PathTemplate;
            Route = routeMapping.Route;
            ActionDescriptor = actionDescriptor;
            Options = options;
            UrlKeyDelimiter = UrlKeyDelimiterOrDefault( serviceProvider.GetRequiredService<ODataOptions>().UrlKeyDelimiter );

            var container = EdmModel.EntityContainer;

            if ( container == null )
            {
                IsRouteExcluded = true;
                return;
            }

            var controllerName = actionDescriptor.ControllerName;
            var actionName = actionDescriptor.ActionName;

            EntitySet = container.FindEntitySet( controllerName );
            Operation = container.FindOperationImports( controllerName ).FirstOrDefault()?.Operation ??
                        EdmModel.FindDeclaredOperations( string.Concat( container.Namespace, ".", actionName ) ).FirstOrDefault();
            ActionType = GetActionType( EntitySet, Operation );
        }

        internal IServiceProvider Services => serviceProvider;

        internal IODataPathTemplateHandler PathTemplateHandler =>
            templateHandler ?? ( templateHandler = serviceProvider.GetRequiredService<IODataPathTemplateHandler>() );
    }
}