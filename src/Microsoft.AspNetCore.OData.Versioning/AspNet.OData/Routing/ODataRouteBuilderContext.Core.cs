namespace Microsoft.AspNet.OData.Routing
{
    using Microsoft.AspNet.OData;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.OData;
    using Microsoft.OData.Edm;
    using System;
    using System.Diagnostics.Contracts;
    using System.Reflection;
    using static Microsoft.OData.ODataUrlKeyDelimiter;
    using static System.Linq.Enumerable;

    partial class ODataRouteBuilderContext
    {
        private IODataPathTemplateHandler templateHandler;

        internal ODataRouteBuilderContext( ODataRouteMapping routeMapping, ControllerActionDescriptor actionDescriptor )
        {
            Contract.Requires( routeMapping != null );
            Contract.Requires( actionDescriptor != null );

            ApiVersion = routeMapping.ApiVersion;
            serviceProvider = routeMapping.Services;
            EdmModel = serviceProvider.GetRequiredService<IEdmModel>();
            routeAttribute = actionDescriptor.MethodInfo.GetCustomAttributes<ODataRouteAttribute>().FirstOrDefault();
            RouteTemplate = routeAttribute?.PathTemplate;
            Route = routeMapping.Route;
            ActionDescriptor = actionDescriptor;
            UrlKeyDelimiter = serviceProvider.GetRequiredService<ODataOptions>().UrlKeyDelimiter ?? Parentheses;

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