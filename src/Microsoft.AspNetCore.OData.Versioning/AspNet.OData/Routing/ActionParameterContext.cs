namespace Microsoft.AspNet.OData.Routing
{
    using Microsoft.AspNet.OData.Routing.Template;
    using System;

    sealed class ActionParameterContext
    {
        internal ActionParameterContext( ODataRouteBuilder routeBuilder, ODataRouteBuilderContext routeContext )
        {
            var odataPathTemplate = routeBuilder.BuildPath( includePrefix: false );
            RouteContext = routeContext;
            PathTemplate = RouteContext.PathTemplateHandler.ParseTemplate( odataPathTemplate, Services );
        }

        internal ODataRouteBuilderContext RouteContext { get; }

        internal IServiceProvider Services => RouteContext.Services;

        internal ODataPathTemplate PathTemplate { get; }
    }
}