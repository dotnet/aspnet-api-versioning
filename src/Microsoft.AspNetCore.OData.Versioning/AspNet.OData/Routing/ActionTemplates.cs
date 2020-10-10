namespace Microsoft.AspNet.OData.Routing
{
    using Microsoft.AspNet.OData.Routing.Template;

    sealed class ActionTemplates
    {
        internal ActionTemplates( string routeTemplate, ODataPathTemplate pathTemplate )
        {
            RouteTemplate = routeTemplate;
            PathTemplate = pathTemplate;
        }

        internal string RouteTemplate { get; }

        internal ODataPathTemplate PathTemplate { get; }
    }
}