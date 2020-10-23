namespace Microsoft.AspNet.OData.Routing
{
    using Microsoft.AspNet.OData;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Abstractions;
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
        internal ODataRouteBuilderContext(
            ODataRouteMapping routeMapping,
            ApiVersion apiVersion,
            ControllerActionDescriptor actionDescriptor,
            ODataApiExplorerOptions options )
        {
            ApiVersion = apiVersion;
            Services = routeMapping.Services;
            routeAttribute = actionDescriptor.MethodInfo.GetCustomAttributes<ODataRouteAttribute>().FirstOrDefault();
            RouteTemplate = routeAttribute?.PathTemplate;
            RoutePrefix = routeMapping.RoutePrefix;
            ActionDescriptor = actionDescriptor;
            ParameterDescriptions = new List<ApiParameterDescription>();
            Options = options;
            UrlKeyDelimiter = UrlKeyDelimiterOrDefault( Services.GetRequiredService<ODataOptions>().UrlKeyDelimiter );

            var selector = Services.GetRequiredService<IEdmModelSelector>();
            var model = selector.SelectModel( apiVersion );
            var container = model?.EntityContainer;

            if ( model == null || container == null )
            {
                EdmModel = Services.GetRequiredService<IEdmModel>();
                IsRouteExcluded = true;
                return;
            }

            EdmModel = model;
            Services = new FixedEdmModelServiceProviderDecorator( Services, model );
            EntitySet = container.FindEntitySet( actionDescriptor.ControllerName );
            Singleton = container.FindSingleton( actionDescriptor.ControllerName );
            Operation = ResolveOperation( container, actionDescriptor.ActionName );
            ActionType = GetActionType( actionDescriptor );
            IsRouteExcluded = ActionType == ODataRouteActionType.Unknown;
        }

        internal IODataPathTemplateHandler PathTemplateHandler =>
            templateHandler ??= Services.GetRequiredService<IODataPathTemplateHandler>();

        static IEnumerable<string> GetHttpMethods( ControllerActionDescriptor action ) => action.GetHttpMethods();

        internal IModelMetadataProvider? ModelMetadataProvider { get; set; }
    }
}