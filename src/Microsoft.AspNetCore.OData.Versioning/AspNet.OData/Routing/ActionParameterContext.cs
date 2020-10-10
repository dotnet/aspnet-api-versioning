namespace Microsoft.AspNet.OData.Routing
{
    using System;
    using System.Collections.Generic;

    sealed class ActionParameterContext
    {
        internal ActionParameterContext( ODataRouteBuilder routeBuilder, ODataRouteBuilderContext routeContext )
        {
            RouteContext = routeContext;

            var template = routeBuilder.BuildPath( includePrefix: false );
            var prefix = routeBuilder.GetRoutePrefix();

            if ( routeBuilder.IsNavigationPropertyLink )
            {
                var routeTemplates = routeBuilder.ExpandNavigationPropertyLinkTemplate( template );
                var templates = new List<ActionTemplates>( capacity: routeTemplates.Count );

                for ( var i = 0; i < routeTemplates.Count; i++ )
                {
                    var routeTemplate = routeTemplates[i];
                    var pathTemplate = routeContext.PathTemplateHandler.SafeParseTemplate( routeTemplate, Services );

                    if ( pathTemplate == null )
                    {
                        continue;
                    }

                    if ( !string.IsNullOrEmpty( prefix ) )
                    {
                        routeTemplate = string.Concat( prefix, "/", routeTemplate );
                    }

                    templates.Add( new ActionTemplates( routeTemplate, pathTemplate ) );
                }

                Templates = templates.ToArray();
            }
            else
            {
                var pathTemplate = routeContext.PathTemplateHandler.SafeParseTemplate( template, Services );

                if ( pathTemplate == null )
                {
                    Templates = Array.Empty<ActionTemplates>();
                }
                else
                {
                    if ( !string.IsNullOrEmpty( prefix ) )
                    {
                        template = string.Concat( prefix, "/", template );
                    }

                    Templates = new[] { new ActionTemplates( template, pathTemplate ) };
                }
            }
        }

        internal ODataRouteBuilderContext RouteContext { get; }

        internal IServiceProvider Services => RouteContext.Services;

        internal IReadOnlyList<ActionTemplates> Templates { get; }

        internal bool IsSupported => Templates.Count > 0;
    }
}