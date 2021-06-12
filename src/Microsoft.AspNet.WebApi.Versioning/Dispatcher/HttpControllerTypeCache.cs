namespace Microsoft.Web.Http.Dispatcher
{
    using Microsoft.Web.Http.Versioning.Conventions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Web.Http;

    sealed class HttpControllerTypeCache
    {
        readonly HttpConfiguration configuration;
        readonly Lazy<Dictionary<string, ILookup<string, Type>>> cache;

        internal HttpControllerTypeCache( HttpConfiguration configuration )
        {
            this.configuration = configuration;
            cache = new Lazy<Dictionary<string, ILookup<string, Type>>>( InitializeCache );
        }

        static string GetControllerName( Type type, IControllerNameConvention convention )
        {
            var name = type.GetCustomAttribute<ControllerNameAttribute>( false ) is ControllerNameAttribute attribute ?
                attribute.Name :
                type.Name;

            return convention.GroupName( convention.NormalizeName( name ) );
        }

        Dictionary<string, ILookup<string, Type>> InitializeCache()
        {
            var services = configuration.Services;
            var assembliesResolver = services.GetAssembliesResolver();
            var typeResolver = services.GetHttpControllerTypeResolver();
            var convention = configuration.GetApiVersioningOptions().ControllerNameConvention;
            var comparer = StringComparer.OrdinalIgnoreCase;

            return typeResolver.GetControllerTypes( assembliesResolver )
                               .GroupBy( type => GetControllerName( type, convention ), comparer )
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