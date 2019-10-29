namespace Microsoft.Web.Http.Dispatcher
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Web.Http;
    using static System.Web.Http.Dispatcher.DefaultHttpControllerSelector;

    sealed class HttpControllerTypeCache
    {
        readonly HttpConfiguration configuration;
        readonly Lazy<Dictionary<string, ILookup<string, Type>>> cache;

        internal HttpControllerTypeCache( HttpConfiguration configuration )
        {
            this.configuration = configuration;
            cache = new Lazy<Dictionary<string, ILookup<string, Type>>>( InitializeCache );
        }

        static string GetControllerName( Type type )
        {
            // allow authors to specify a controller name via an attribute. this is required for controllers that
            // do not use attribute-based routing, but support versioning. in the pure Convention-Over-Configuration
            // model, this is not otherwise possible because each controller type maps to a different route
            var attribute = type.GetCustomAttributes<ControllerNameAttribute>( false ).SingleOrDefault();

            if ( attribute != null )
            {
                return attribute.Name;
            }

            // use standard convention for the controller name (ex: ValuesController -> Values)
            var name = type.Name;
            var suffixLength = ControllerSuffix.Length;

            name = name.Substring( 0, name.Length - suffixLength );

            // trim trailing numbers to enable grouping by convention (ex: Values1Controller -> Values, Values2Controller -> Values)
            return TrimTrailingNumbers( name );
        }

        static string TrimTrailingNumbers( string name )
        {
            if ( string.IsNullOrEmpty( name ) )
            {
                return name;
            }

            var last = name.Length - 1;

            for ( var i = last; i >= 0; i-- )
            {
                if ( !char.IsNumber( name[i] ) )
                {
                    if ( i < last )
                    {
                        return name.Substring( 0, i + 1 );
                    }

                    return name;
                }
            }

            return name;
        }

        Dictionary<string, ILookup<string, Type>> InitializeCache()
        {
            var services = configuration.Services;
            var assembliesResolver = services.GetAssembliesResolver();
            var typeResolver = services.GetHttpControllerTypeResolver();
            var comparer = StringComparer.OrdinalIgnoreCase;

            return typeResolver.GetControllerTypes( assembliesResolver )
                               .GroupBy( GetControllerName, comparer )
                               .ToDictionary( g => g.Key, g => g.ToLookup( t => t.Namespace ?? string.Empty, comparer ), comparer );
        }

        internal Dictionary<string, ILookup<string, Type>> Cache => cache.Value;

        internal ICollection<Type> GetControllerTypes( string? controllerName )
        {
            if ( string.IsNullOrEmpty( controllerName ) )
            {
                return Type.EmptyTypes;
            }

            var set = new HashSet<Type>();

            if ( cache.Value.TryGetValue( controllerName!, out var lookup ) )
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