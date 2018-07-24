namespace Microsoft.AspNet.OData.Routing
{
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

        static string UpdateRoutePrefixAndRemoveApiVersionParameterIfNecessary( string routePrefix ) => routePrefix;
    }
}