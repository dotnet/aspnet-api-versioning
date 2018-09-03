namespace System.Web.Http
{
    using System.Diagnostics.Contracts;
    using System.Web.Http.Routing;

    static class HttpRouteCollectionExtensions
    {
        internal static string GetRouteName( this HttpRouteCollection routes, IHttpRoute route )
        {
            Contract.Requires( routes != null );
            Contract.Requires( route != null );

            foreach ( var item in routes.ToDictionary() )
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