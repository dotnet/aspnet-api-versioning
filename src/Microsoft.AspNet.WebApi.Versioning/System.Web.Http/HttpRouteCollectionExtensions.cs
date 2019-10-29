namespace System.Web.Http
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Web.Http.Routing;
    using static System.Reflection.BindingFlags;

    /// <summary>
    /// Provides extension methods for the <see cref="HttpRouteCollection"/>.
    /// </summary>
    public static class HttpRouteCollectionExtensions
    {
        /// <summary>
        /// Returns the route collection as a read-only dictionary mapping configured names to routes.
        /// </summary>
        /// <param name="routes">The <see cref="HttpRouteCollection">route collection</see> to convert.</param>
        /// <returns>A new <see cref="IReadOnlyDictionary{TKey, TValue}">read-only dictonary</see> of
        /// <see cref="IHttpRoute">routes</see> mapped to their name.</returns>
        public static IReadOnlyDictionary<string, IHttpRoute> ToDictionary( this HttpRouteCollection routes )
        {
            if ( routes == null )
            {
                throw new ArgumentNullException( nameof( routes ) );
            }

            const string HostedHttpRouteCollection = "System.Web.Http.WebHost.Routing.HostedHttpRouteCollection";

            try
            {
                return routes.CopyToDictionary();
            }
            catch ( NotSupportedException ) when ( routes.GetType().FullName == HostedHttpRouteCollection )
            {
                return routes.BuildDictionaryFromKeys();
            }
        }

        static IReadOnlyDictionary<string, IHttpRoute> CopyToDictionary( this HttpRouteCollection routes )
        {
            var items = new KeyValuePair<string, IHttpRoute>[routes.Count];

            routes.CopyTo( items, 0 );

            var dictionary = new Dictionary<string, IHttpRoute>( routes.Count, StringComparer.OrdinalIgnoreCase );

            for ( var i = 0; i < items.Length; i++ )
            {
                var item = items[i];
                dictionary[item.Key] = item.Value;
            }

            return dictionary;
        }

        static IReadOnlyDictionary<string, IHttpRoute> BuildDictionaryFromKeys( this HttpRouteCollection routes )
        {
            var keys = routes.Keys();
            var dictionary = new Dictionary<string, IHttpRoute>( routes.Count, StringComparer.OrdinalIgnoreCase );

            for ( var i = 0; i < keys.Count; i++ )
            {
                var key = keys[i];
                dictionary[key] = routes[key];
            }

            return dictionary;
        }

        static IReadOnlyList<string> Keys( this HttpRouteCollection routes )
        {
            var collection = GetDictionaryKeys( routes );
            var keys = new string[collection.Count];

            collection.CopyTo( keys, 0 );

            return keys;
        }

        static ICollection<string> GetDictionaryKeys( HttpRouteCollection routes )
        {
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