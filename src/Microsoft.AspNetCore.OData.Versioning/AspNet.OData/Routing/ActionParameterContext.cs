namespace Microsoft.AspNet.OData.Routing
{
    using Microsoft.AspNet.OData.Routing.Template;
    using System;

    sealed class ActionParameterContext
    {
        readonly ODataPathTemplate? pathTemplate;

        internal ActionParameterContext( ODataRouteBuilder routeBuilder, ODataRouteBuilderContext routeContext )
        {
            var odataPathTemplate = routeBuilder.BuildPath( includePrefix: false );

            RouteContext = routeContext;
            pathTemplate = RouteContext.PathTemplateHandler.SafeParseTemplate( odataPathTemplate, Services );
        }

        internal ODataRouteBuilderContext RouteContext { get; }

        internal IServiceProvider Services => RouteContext.Services;

        internal ODataPathTemplate PathTemplate => pathTemplate ?? throw new NotSupportedException();

        internal bool IsSupported => pathTemplate != null;
    }
}