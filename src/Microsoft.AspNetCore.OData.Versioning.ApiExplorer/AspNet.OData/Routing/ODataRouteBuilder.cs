namespace Microsoft.AspNet.OData.Routing
{
    using Microsoft.AspNetCore.Routing.Template;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using static System.String;

    partial class ODataRouteBuilder
    {
        internal string BuildPath()
        {
            var segments = new List<string>();
            AppendEntitySetOrOperation( segments );
            return Join( "/", segments );
        }

        static string RemoveRouteConstraints( string routePrefix )
        {
            Contract.Requires( !IsNullOrEmpty( routePrefix ) );
            Contract.Ensures( !IsNullOrEmpty( Contract.Result<string>() ) );

            var parsedTemplate = TemplateParser.Parse( routePrefix );
            var segments = new List<string>( parsedTemplate.Segments.Count );

            foreach ( var segment in parsedTemplate.Segments )
            {
                var currentSegment = Empty;

                foreach ( var part in segment.Parts )
                {
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