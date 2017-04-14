namespace System.Web.Http
{
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Web.Http.Routing;

    static class HttpRouteCollectionExtensions
    {
        internal static string GetRouteName( this HttpRouteCollection routes, IHttpRoute route )
        {
            Contract.Requires( routes != null );
            Contract.Requires( route != null );

            var items = new KeyValuePair<string, IHttpRoute>[routes.Count];

            routes.CopyTo( items, 0 );

            foreach ( var item in items )
            {
                if ( Equals( item.Value, route ) )
                {
                    return item.Key;
                }
            }

            return null;
        }
    }
}