// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Dispatcher;

using Asp.Versioning.Conventions;
using System.Reflection;
using System.Web.Http;

internal sealed class HttpControllerTypeCache
{
    private readonly HttpConfiguration configuration;
    private readonly Lazy<Dictionary<string, ILookup<string, Type>>> cache;

    internal HttpControllerTypeCache( HttpConfiguration configuration )
    {
        this.configuration = configuration;
        cache = new( InitializeCache );
    }

    private static string GetControllerName( Type type, IControllerNameConvention convention )
    {
        var name = type.GetCustomAttribute<ControllerNameAttribute>( false ) switch
        {
            ControllerNameAttribute attribute => attribute.Name,
            _ => type.Name,
        };

        return convention.GroupName( convention.NormalizeName( name ) );
    }

    private Dictionary<string, ILookup<string, Type>> InitializeCache()
    {
        var services = configuration.Services;
        var assembliesResolver = services.GetAssembliesResolver();
        var typeResolver = services.GetHttpControllerTypeResolver();
        var convention = configuration.GetControllerNameConvention();
        var comparer = StringComparer.OrdinalIgnoreCase;

        return typeResolver.GetControllerTypes( assembliesResolver )
                           .GroupBy( type => GetControllerName( type, convention ), comparer )
                           .ToDictionary( g => g.Key, g => g.ToLookup( t => t.Namespace ?? string.Empty, comparer ), comparer );
    }

    internal Dictionary<string, ILookup<string, Type>> Cache => cache.Value;

    internal ICollection<Type> GetControllerTypes( string? controllerName )
    {
        if ( string.IsNullOrEmpty( controllerName ) || !cache.Value.TryGetValue( controllerName!, out var lookup ) )
        {
            return Type.EmptyTypes;
        }

        var set = new HashSet<Type>();

        foreach ( var grouping in lookup )
        {
            set.UnionWith( grouping );
        }

        return set;
    }
}