namespace System.Web.Http
{
    using System.Web.Http.Routing;

    static class HttpRouteCollectionExtensions
    {
        internal static string? GetRouteName( this HttpRouteCollection routes, IHttpRoute route )
        {
            foreach ( var item in routes.ToDictionary() )
            {
                if ( Equals( item.Value, route ) )
                {
                    return item.Key;
                }
            }

            return default;
        }
    }
}