namespace System.Web.Http
{
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Reflection;
    using System.Web.Http.Routing;
    using static System.Reflection.BindingFlags;

    static class HttpRouteCollectionExtensions
    {
        internal static string GetRouteName( this HttpRouteCollection routes, IHttpRoute route )
        {
            Contract.Requires( routes != null );
            Contract.Requires( route != null );

            var items = CopyRouteEntries( routes );

            foreach ( var item in items )
            {
                if ( Equals( item.Value, route ) )
                {
                    return item.Key;
                }
            }

            return null;
        }

        static KeyValuePair<string, IHttpRoute>[] CopyRouteEntries( HttpRouteCollection routes )
        {
            Contract.Requires( routes != null );
            Contract.Ensures( Contract.Result<KeyValuePair<string, IHttpRoute>[]>() != null );

            var items = new KeyValuePair<string, IHttpRoute>[routes.Count];

            try
            {
                routes.CopyTo( items, 0 );
            }
            catch ( NotSupportedException ) when ( routes.GetType().FullName == "System.Web.Http.WebHost.Routing.HostedHttpRouteCollection" )
            {
                var keys = GetRouteKeys( routes );

                for ( var i = 0; i < keys.Count; i++ )
                {
                    var key = keys[i];
                    var route = routes[key];

                    items[i] = new KeyValuePair<string, IHttpRoute>( key, route );
                }
            }

            return items;
        }

        static IReadOnlyList<string> GetRouteKeys( HttpRouteCollection routes )
        {
            Contract.Requires( routes != null );
            Contract.Ensures( Contract.Result<IReadOnlyList<string>>() != null );

            var collection = GetKeys( routes );
            var keys = new string[collection.Count];

            collection.CopyTo( keys, 0 );

            return keys;
        }

        static ICollection<string> GetKeys( HttpRouteCollection routes )
        {
            Contract.Requires( routes != null );
            Contract.Ensures( Contract.Result<ICollection<string>>() != null );

            // HACK: System.Web.Routing.RouteCollection doesn't expose the names associated with registered routes. The
            // HostedHttpRouteCollection could have provided an adapter to support it, but didn't. Instead, it always throws
            // NotSupportedException for the HttpRouteCollection.CopyTo method. This only happens when hosted on IIS. The
            // only way to get the keys is use reflection to poke at the underlying dictionary.
            var routeCollection = routes.GetType().GetField( "_routeCollection", Instance | NonPublic ).GetValue( routes );
            var dictionary = routeCollection.GetType().GetField( "_namedMap", Instance | NonPublic ).GetValue( routeCollection );
            var keys = (ICollection<string>) dictionary.GetType().GetRuntimeProperty( "Keys" ).GetValue( dictionary, null );

            return keys;
        }
    }
}