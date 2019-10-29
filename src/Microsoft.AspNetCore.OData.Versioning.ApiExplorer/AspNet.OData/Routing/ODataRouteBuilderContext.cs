namespace Microsoft.AspNet.OData.Routing
{
    using Microsoft.AspNet.OData;
    using Microsoft.AspNetCore.Mvc.ApiExplorer;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.OData;
    using Microsoft.OData.Edm;
    using System.Collections.Generic;
    using System.Reflection;
    using static System.Linq.Enumerable;

    partial class ODataRouteBuilderContext
    {
        private IODataPathTemplateHandler? templateHandler;

        internal ODataRouteBuilderContext(
            ODataRouteMapping routeMapping,
            ControllerActionDescriptor actionDescriptor,
            ODataApiExplorerOptions options )
        {
            ApiVersion = routeMapping.ApiVersion;
            Services = routeMapping.Services;
            EdmModel = Services.GetRequiredService<IEdmModel>();
            routeAttribute = actionDescriptor.MethodInfo.GetCustomAttributes<ODataRouteAttribute>().FirstOrDefault();
            RouteTemplate = routeAttribute?.PathTemplate;
            Route = routeMapping.Route;
            ActionDescriptor = actionDescriptor;
            ParameterDescriptions = new List<ApiParameterDescription>();
            Options = options;
            UrlKeyDelimiter = UrlKeyDelimiterOrDefault( Services.GetRequiredService<ODataOptions>().UrlKeyDelimiter );

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

        internal IODataPathTemplateHandler PathTemplateHandler =>
            templateHandler ??= Services.GetRequiredService<IODataPathTemplateHandler>();

        internal IModelMetadataProvider? ModelMetadataProvider { get; set; }
    }
}