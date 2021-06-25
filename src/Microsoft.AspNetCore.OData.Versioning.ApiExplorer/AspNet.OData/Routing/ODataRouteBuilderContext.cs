namespace Microsoft.AspNet.OData.Routing
{
    using Microsoft.AspNet.OData;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Abstractions;
#if API_EXPLORER
    using Microsoft.AspNetCore.Mvc.ApiExplorer;
#endif
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Microsoft.AspNetCore.Mvc.Routing;
#if !API_EXPLORER
    using Microsoft.AspNetCore.Mvc.Versioning;
#endif
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.OData;
    using Microsoft.OData.Edm;
    using System.Collections.Generic;
    using System.Linq;
    using static Microsoft.AspNetCore.Mvc.ModelBinding.BindingSource;

    partial class ODataRouteBuilderContext
    {
        internal ODataRouteBuilderContext(
            ODataRouteMapping routeMapping,
            ApiVersion apiVersion,
            ControllerActionDescriptor actionDescriptor,
#if API_EXPLORER
            ODataApiExplorerOptions options )
#else
            ODataApiVersioningOptions options )
#endif
        {
            ApiVersion = apiVersion;
            Services = routeMapping.Services;
            (RouteTemplateProvider, routeAttribute) = TryGetRouteAttribute( actionDescriptor );
            RouteTemplate = routeAttribute?.PathTemplate ?? RouteTemplateProvider?.Template;
            RoutePrefix = routeMapping.RoutePrefix;
            ActionDescriptor = actionDescriptor;
#if API_EXPLORER
            ParameterDescriptions = new List<ApiParameterDescription>();
#endif
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
            Operation = ResolveOperation( container, actionDescriptor );
            ActionType = GetActionType( actionDescriptor );
            IsRouteExcluded = ActionType == ODataRouteActionType.Unknown;
        }

        internal IODataPathTemplateHandler PathTemplateHandler =>
            templateHandler ??= Services.GetRequiredService<IODataPathTemplateHandler>();

        internal IModelMetadataProvider? ModelMetadataProvider { get; set; }

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

        static IList<ParameterDescriptor> FilterParameters( ControllerActionDescriptor action )
        {
            var parameters = action.Parameters.ToList();

            for ( var i = parameters.Count - 1; i >= 0; i-- )
            {
                if ( parameters[i].BindingInfo is not BindingInfo info )
                {
                    continue;
                }

                if ( info.BindingSource == Special )
                {
                    parameters.RemoveAt( i );
                }
                else if ( info.BindingSource == Custom )
                {
                    var type = parameters[i].ParameterType;

                    if ( type.IsODataQueryOptions() || type.IsODataActionParameters() || type.IsODataPath() )
                    {
                        parameters.RemoveAt( i );
                    }
                }
            }

            return parameters;
        }
    }
}