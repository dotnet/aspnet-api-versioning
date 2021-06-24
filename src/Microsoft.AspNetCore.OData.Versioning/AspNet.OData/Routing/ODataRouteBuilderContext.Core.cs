namespace Microsoft.AspNet.OData.Routing
{
    using Microsoft.AspNet.OData;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Mvc.Routing;
    using Microsoft.AspNetCore.Mvc.Versioning;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.OData;
    using Microsoft.OData.Edm;
    using System.Collections.Generic;

    partial class ODataRouteBuilderContext
    {
        internal ODataRouteBuilderContext(
            ApiVersion apiVersion,
            ODataRouteMapping routeMapping,
            ControllerActionDescriptor actionDescriptor,
            ODataApiVersioningOptions options )
        {
            ApiVersion = apiVersion;
            Services = routeMapping.Services;
            (RouteTemplateProvider, routeAttribute) = TryGetRouteAttribute( actionDescriptor );
            RouteTemplate = routeAttribute?.PathTemplate ?? RouteTemplateProvider?.Template;
            RoutePrefix = routeMapping.RoutePrefix;
            ActionDescriptor = actionDescriptor;
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

        static (IRouteTemplateProvider? RouteTemplateProvider, ODataRouteAttribute? RouteAttribute) TryGetRouteAttribute( ControllerActionDescriptor actionDescriptor )
        {
            var attributes = actionDescriptor.MethodInfo.GetCustomAttributes( inherit: false );
            var templateProvider = default( IRouteTemplateProvider );

            for ( var i = 0; i < attributes.Length; i++ )
            {
                var attribute = attributes[i];

                if ( attribute is ODataRouteAttribute routeAttribute )
                {
                    return (default, routeAttribute);
                }

                if ( templateProvider is null )
                {
                    templateProvider = attribute as IRouteTemplateProvider;
                }
            }

            return templateProvider is null ? default : (templateProvider, new ODataRouteAttribute( templateProvider.Template ));
        }
    }
}