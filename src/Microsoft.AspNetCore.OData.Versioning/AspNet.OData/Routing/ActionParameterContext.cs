namespace Microsoft.AspNet.OData.Routing
{
    using Microsoft.AspNet.OData.Routing.Template;
    using System;
    using System.Diagnostics.Contracts;

    sealed class ActionParameterContext
    {
        internal ActionParameterContext( ODataRouteBuilder routeBuilder, ODataRouteBuilderContext routeContext )
        {
            Contract.Requires( routeBuilder != null );
            Contract.Requires( routeContext != null );

            var odataPathTemplate = routeBuilder.BuildPath( includePrefix: false );
            RouteContext = routeContext;
            PathTemplate = RouteContext.PathTemplateHandler.ParseTemplate( odataPathTemplate, Services );
        }

        internal ODataRouteBuilderContext RouteContext { get; }

        internal IServiceProvider Services => RouteContext.Services;

        internal ODataPathTemplate PathTemplate { get; }
    }
}