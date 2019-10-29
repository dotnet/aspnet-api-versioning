namespace Microsoft.AspNet.OData.Routing
{
    using Microsoft.AspNetCore.Routing.Template;
    using System.Collections.Generic;
    using static System.String;

    partial class ODataRouteBuilder
    {
        internal string BuildPath( bool includePrefix )
        {
            var segments = new List<string>();

            if ( includePrefix )
            {
                AppendRoutePrefix( segments );
            }

            AppendEntitySetOrOperation( segments );

            return Join( "/", segments );
        }

        static string RemoveRouteConstraints( string routePrefix )
        {
            var parsedTemplate = TemplateParser.Parse( routePrefix );
            var segments = new List<string>( parsedTemplate.Segments.Count );

            for ( var i = 0; i < parsedTemplate.Segments.Count; i++ )
            {
                var currentSegment = Empty;
                var parts = parsedTemplate.Segments[i].Parts;

                for ( var j = 0; j < parts.Count; j++ )
                {
                    var part = parts[j];

                    if ( part.IsLiteral )
                    {
                        currentSegment += part.Text;
                    }
                    else if ( part.IsParameter )
                    {
                        currentSegment += Concat( "{", part.Name, "}" );
                    }
                }

                segments.Add( currentSegment );
            }

            return Join( "/", segments );
        }
    }
}