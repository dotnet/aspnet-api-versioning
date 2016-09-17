namespace Microsoft.Web.Http.Dispatcher
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Reflection;
    using System.Web.Http;
    using System.Web.Http.Dispatcher;

    internal sealed class HttpControllerTypeCache
    {
        private readonly HttpConfiguration configuration;
        private readonly Lazy<Dictionary<string, ILookup<string, Type>>> cache;

        internal HttpControllerTypeCache( HttpConfiguration configuration )
        {
            Contract.Requires( configuration != null );

            this.configuration = configuration;
            cache = new Lazy<Dictionary<string, ILookup<string, Type>>>( InitializeCache );
        }

        private static string GetControllerName( Type type )
        {
            Contract.Requires( type != null );
            Contract.Ensures( !string.IsNullOrEmpty( Contract.Result<string>() ) );

            // allow authors to specify a controller name via an attribute. this is required for controllers that
            // do not use attribute-based routing, but support versioning. in the pure Convention-Over-Configuration
            // model, this is not otherwise possible because each controller type maps to a different route
            var attribute = type.GetCustomAttributes<ControllerNameAttribute>( false ).SingleOrDefault();

            if ( attribute != null )
            {
                return attribute.Name;
            }

            // use standard convention for the controller name
            var name = type.Name;
            var suffixLength = DefaultHttpControllerSelector.ControllerSuffix.Length;

            return name.Substring( 0, name.Length - suffixLength );
        }

        private Dictionary<string, ILookup<string, Type>> InitializeCache()
        {
            Contract.Ensures( Contract.Result<Dictionary<string, ILookup<string, Type>>>() != null );

            var services = configuration.Services;
            var assembliesResolver = services.GetAssembliesResolver();
            var typeResolver = services.GetHttpControllerTypeResolver();
            var comparer = StringComparer.OrdinalIgnoreCase;

            return typeResolver.GetControllerTypes( assembliesResolver )
                               .GroupBy( GetControllerName, comparer )
                               .ToDictionary( g => g.Key, g => g.ToLookup( t => t.Namespace ?? string.Empty, comparer ), comparer );
        }

        internal Dictionary<string, ILookup<string, Type>> Cache
        {
            get
            {
                Contract.Ensures( Contract.Result<Dictionary<string, ILookup<string, Type>>>() != null );
                return cache.Value;
            }
        }

        internal ICollection<Type> GetControllerTypes( string controllerName )
        {
            Contract.Requires( !string.IsNullOrEmpty( controllerName ) );
            Contract.Ensures( Contract.Result<ICollection<Type>>() != null );

            var set = new HashSet<Type>();
            ILookup<string, Type> lookup;

            if ( cache.Value.TryGetValue( controllerName, out lookup ) )
            {
                foreach ( var grouping in lookup )
                {
                    set.UnionWith( grouping );
                }
            }

            return set;
        }
    }
}
