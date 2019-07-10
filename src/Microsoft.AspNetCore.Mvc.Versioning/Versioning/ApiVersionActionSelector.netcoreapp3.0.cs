#if NETCOREAPP3_0
namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.ActionConstraints;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.AspNetCore.Mvc.Routing;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using static System.Globalization.CultureInfo;

    /// <content>
    /// Provides additional implementation specific to ASP.NET Core 3.0.
    /// </content>
    public partial class ApiVersionActionSelector
    {
        ActionSelectionTable<ActionDescriptor> cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiVersionActionSelector"/> class.
        /// </summary>
        /// <param name="actionDescriptorCollectionProvider">The <see cref="IActionDescriptorCollectionProvider "/> used to select candidate routes.</param>
        /// <param name="options">The <see cref="ApiVersioningOptions">options</see> associated with the action selector.</param>
        /// <param name="actionConstraintProviders">The <see cref="IEnumerable{T}">sequence</see> of <see cref="IActionConstraintProvider">action constraint providers</see>.</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        /// <param name="routePolicy">The <see cref="IApiVersionRoutePolicy">route policy</see> applied to candidate matches.</param>
        public ApiVersionActionSelector(
            IActionDescriptorCollectionProvider actionDescriptorCollectionProvider,
            IEnumerable<IActionConstraintProvider> actionConstraintProviders,
            IOptions<ApiVersioningOptions> options,
            ILoggerFactory loggerFactory,
            IApiVersionRoutePolicy routePolicy )
        {
            Arg.NotNull( actionDescriptorCollectionProvider, nameof( actionDescriptorCollectionProvider ) );
            Arg.NotNull( actionConstraintProviders, nameof( actionConstraintProviders ) );
            Arg.NotNull( options, nameof( options ) );
            Arg.NotNull( loggerFactory, nameof( loggerFactory ) );
            Arg.NotNull( routePolicy, nameof( routePolicy ) );

            this.actionDescriptorCollectionProvider = actionDescriptorCollectionProvider;
            actionConstraintCache = new ActionConstraintCache( actionDescriptorCollectionProvider, actionConstraintProviders );
            this.options = options;
            Logger = loggerFactory.CreateLogger( GetType() );
            RoutePolicy = routePolicy;
        }

        ActionSelectionTable<ActionDescriptor> Current
        {
            get
            {
                var actions = actionDescriptorCollectionProvider.ActionDescriptors;
                var value = Volatile.Read( ref cache );

                if ( value != null && value.Version == actions.Version )
                {
                    return value;
                }

                value = ActionSelectionTable<ActionDescriptor>.Create( actions );
                Volatile.Write( ref cache, value );

                return value;
            }
        }

        // REF: https://raw.githubusercontent.com/aspnet/AspNetCore/master/src/Mvc/Mvc.Core/src/Infrastructure/ActionSelectionTable.cs
        sealed class ActionSelectionTable<TItem>
        {
            ActionSelectionTable(
                int version,
                string[] routeKeys,
                Dictionary<string[], List<TItem>> ordinalEntries,
                Dictionary<string[], List<TItem>> ordinalIgnoreCaseEntries )
            {
                Version = version;
                RouteKeys = routeKeys;
                OrdinalEntries = ordinalEntries;
                OrdinalIgnoreCaseEntries = ordinalIgnoreCaseEntries;
            }

            public int Version { get; }

            internal string[] RouteKeys { get; }

            internal Dictionary<string[], List<TItem>> OrdinalEntries { get; }

            internal Dictionary<string[], List<TItem>> OrdinalIgnoreCaseEntries { get; }

            public static ActionSelectionTable<ActionDescriptor> Create( ActionDescriptorCollection actions )
            {
                return CreateCore(
                    version: actions.Version,
                    items: actions.Items.Where( a => a.AttributeRouteInfo == null ),
                    getRouteKeys: a => a.RouteValues.Keys,
                    getRouteValue: ( a, key ) =>
                    {
                        a.RouteValues.TryGetValue( key, out var value );
                        return value ?? string.Empty;
                    } );
            }

            public static ActionSelectionTable<Endpoint> Create( IEnumerable<Endpoint> endpoints )
            {
                return CreateCore(
                    version: 0,
                    items: endpoints.Where( e => e.GetType() == typeof( Endpoint ) ),
                    getRouteKeys: e => e.Metadata.GetMetadata<ActionDescriptor>().RouteValues.Keys,
                    getRouteValue: ( e, key ) =>
                    {
                        e.Metadata.GetMetadata<ActionDescriptor>().RouteValues.TryGetValue( key, out var value );
                        return Convert.ToString( value, InvariantCulture ) ?? string.Empty;
                    } );
            }

            static ActionSelectionTable<T> CreateCore<T>(
                int version,
                IEnumerable<T> items,
                Func<T, IEnumerable<string>> getRouteKeys,
                Func<T, string, string> getRouteValue )
            {
                var ordinalEntries = new Dictionary<string[], List<T>>( StringArrayComparer.Ordinal );
                var ordinalIgnoreCaseEntries = new Dictionary<string[], List<T>>( StringArrayComparer.OrdinalIgnoreCase );
                var routeKeys = new SortedSet<string>( StringComparer.OrdinalIgnoreCase );

                foreach ( var item in items )
                {
                    foreach ( var key in getRouteKeys( item ) )
                    {
                        routeKeys.Add( key );
                    }
                }

                foreach ( var item in items )
                {
                    var index = 0;
                    var routeValues = new string[routeKeys.Count];

                    foreach ( var key in routeKeys )
                    {
                        var value = getRouteValue( item, key );
                        routeValues[index++] = value;
                    }

                    if ( !ordinalIgnoreCaseEntries.TryGetValue( routeValues, out var entries ) )
                    {
                        entries = new List<T>();
                        ordinalIgnoreCaseEntries.Add( routeValues, entries );
                    }

                    entries.Add( item );

                    if ( !ordinalEntries.ContainsKey( routeValues ) )
                    {
                        ordinalEntries.Add( routeValues, entries );
                    }
                }

                return new ActionSelectionTable<T>( version, routeKeys.ToArray(), ordinalEntries, ordinalIgnoreCaseEntries );
            }
        }

        // REF: https://github.com/aspnet/AspNetCore/blob/master/src/Mvc/Mvc.Core/src/ActionConstraints/ActionConstraintCache.cs
        sealed class ActionConstraintCache
        {
            readonly IActionDescriptorCollectionProvider collectionProvider;
            readonly IActionConstraintProvider[] actionConstraintProviders;
            volatile InnerCache currentCache;

            public ActionConstraintCache(
                IActionDescriptorCollectionProvider collectionProvider,
                IEnumerable<IActionConstraintProvider> actionConstraintProviders )
            {
                this.collectionProvider = collectionProvider;
                this.actionConstraintProviders = actionConstraintProviders.OrderBy( item => item.Order ).ToArray();
            }

            internal InnerCache CurrentCache
            {
                get
                {
                    var current = currentCache;
                    var actionDescriptors = collectionProvider.ActionDescriptors;

                    if ( current == null || current.Version != actionDescriptors.Version )
                    {
                        current = new InnerCache( actionDescriptors );
                        currentCache = current;
                    }

                    return current;
                }
            }

            public IReadOnlyList<IActionConstraint> GetActionConstraints( HttpContext httpContext, ActionDescriptor action )
            {
                var cache = CurrentCache;

                if ( cache.Entries.TryGetValue( action, out var entry ) )
                {
                    return GetActionConstraintsFromEntry( entry, httpContext, action );
                }

                if ( action.ActionConstraints == null || action.ActionConstraints.Count == 0 )
                {
                    return null;
                }

                var items = new List<ActionConstraintItem>( action.ActionConstraints.Count );

                for ( var i = 0; i < action.ActionConstraints.Count; i++ )
                {
                    items.Add( new ActionConstraintItem( action.ActionConstraints[i] ) );
                }

                ExecuteProviders( httpContext, action, items );

                var actionConstraints = ExtractActionConstraints( items );
                var allActionConstraintsCached = true;

                for ( var i = 0; i < items.Count; i++ )
                {
                    var item = items[i];
                    if ( !item.IsReusable )
                    {
                        item.Constraint = null;
                        allActionConstraintsCached = false;
                    }
                }

                if ( allActionConstraintsCached )
                {
                    entry = new CacheEntry( actionConstraints );
                }
                else
                {
                    entry = new CacheEntry( items );
                }

                cache.Entries.TryAdd( action, entry );
                return actionConstraints;
            }

            IReadOnlyList<IActionConstraint> GetActionConstraintsFromEntry( CacheEntry entry, HttpContext httpContext, ActionDescriptor action )
            {
                if ( entry.ActionConstraints != null )
                {
                    return entry.ActionConstraints;
                }

                var items = new List<ActionConstraintItem>( entry.Items.Count );

                for ( var i = 0; i < entry.Items.Count; i++ )
                {
                    var item = entry.Items[i];

                    if ( item.IsReusable )
                    {
                        items.Add( item );
                    }
                    else
                    {
                        items.Add( new ActionConstraintItem( item.Metadata ) );
                    }
                }

                ExecuteProviders( httpContext, action, items );

                return ExtractActionConstraints( items );
            }

            void ExecuteProviders( HttpContext httpContext, ActionDescriptor action, List<ActionConstraintItem> items )
            {
                var context = new ActionConstraintProviderContext( httpContext, action, items );

                for ( var i = 0; i < actionConstraintProviders.Length; i++ )
                {
                    actionConstraintProviders[i].OnProvidersExecuting( context );
                }

                for ( var i = actionConstraintProviders.Length - 1; i >= 0; i-- )
                {
                    actionConstraintProviders[i].OnProvidersExecuted( context );
                }
            }

            static IReadOnlyList<IActionConstraint> ExtractActionConstraints( List<ActionConstraintItem> items )
            {
                var count = 0;

                for ( var i = 0; i < items.Count; i++ )
                {
                    if ( items[i].Constraint != null )
                    {
                        count++;
                    }
                }

                if ( count == 0 )
                {
                    return null;
                }

                var actionConstraints = new IActionConstraint[count];
                var actionConstraintIndex = 0;

                for ( var i = 0; i < items.Count; i++ )
                {
                    var actionConstraint = items[i].Constraint;

                    if ( actionConstraint != null )
                    {
                        actionConstraints[actionConstraintIndex++] = actionConstraint;
                    }
                }

                return actionConstraints;
            }

            internal class InnerCache
            {
                private readonly ActionDescriptorCollection actions;

                public InnerCache( ActionDescriptorCollection actions )
                {
                    this.actions = actions;
                }

                public ConcurrentDictionary<ActionDescriptor, CacheEntry> Entries { get; } =
                    new ConcurrentDictionary<ActionDescriptor, CacheEntry>();

                public int Version => actions.Version;
            }

            internal readonly struct CacheEntry
            {
                public CacheEntry( IReadOnlyList<IActionConstraint> actionConstraints )
                {
                    ActionConstraints = actionConstraints;
                    Items = null;
                }

                public CacheEntry( List<ActionConstraintItem> items )
                {
                    Items = items;
                    ActionConstraints = null;
                }

                public IReadOnlyList<IActionConstraint> ActionConstraints { get; }

                public List<ActionConstraintItem> Items { get; }
            }
        }

        sealed partial class StringArrayComparer
        {
            public int GetHashCode( string[] obj )
            {
                if ( obj == null )
                {
                    return 0;
                }

                var hash = default( HashCode );

                for ( var i = 0; i < obj.Length; i++ )
                {
                    hash.Add( obj[i] ?? string.Empty, valueComparer );
                }

                return hash.ToHashCode();
            }
        }
    }
}
#endif