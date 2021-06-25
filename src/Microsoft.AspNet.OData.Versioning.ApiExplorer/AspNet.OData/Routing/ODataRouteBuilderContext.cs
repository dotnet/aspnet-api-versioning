namespace Microsoft.AspNet.OData.Routing
{
    using Microsoft.AspNet.OData;
    using Microsoft.AspNet.OData.Routing.Conventions;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.OData;
    using Microsoft.OData.Edm;
    using Microsoft.Web.Http;
    using Microsoft.Web.Http.Description;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Web.Http;
    using System.Web.Http.Controllers;
    using System.Web.Http.Description;
    using static System.Linq.Enumerable;

    partial class ODataRouteBuilderContext
    {
        readonly ODataRoute route;

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
            UrlKeyDelimiter = UrlKeyDelimiterOrDefault( configuration.GetUrlKeyDelimiter() ?? Services.GetService<IODataPathHandler>()?.UrlKeyDelimiter );

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
            IsRouteExcluded = ActionType == ODataRouteActionType.Unknown;

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

        IEnumerable<string> GetHttpMethods( HttpActionDescriptor action ) => action.GetHttpMethods( route ).Select( m => m.Method );

        void ConvertODataActionParametersToTypedModel( IModelTypeBuilder modelTypeBuilder, IEdmAction action, string controllerName )
        {
            for ( var i = 0; i < ParameterDescriptions.Count; i++ )
            {
                var description = ParameterDescriptions[i];
                var parameter = description.ParameterDescriptor;

                if ( parameter != null && parameter.ParameterType.IsODataActionParameters() )
                {
                    var parameterType = modelTypeBuilder.NewActionParameters( Services, action, ApiVersion, controllerName );
                    description.ParameterDescriptor = new ODataModelBoundParameterDescriptor( parameter, parameterType );
                    break;
                }
            }
        }

        static IList<HttpParameterDescriptor> FilterParameters( HttpActionDescriptor action )
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
    }
}