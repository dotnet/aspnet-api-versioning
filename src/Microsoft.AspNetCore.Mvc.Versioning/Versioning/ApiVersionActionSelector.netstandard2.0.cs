#if NETSTANDARD2_0
namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.ActionConstraints;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.AspNetCore.Mvc.Internal;
    using Microsoft.AspNetCore.Mvc.Routing;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Threading;
    using static ApiVersionMapping;
    using static ErrorCodes;
    using static System.Globalization.CultureInfo;

    /// <content>
    /// Provides additional implementation specific to ASP.NET Core 2.2.
    /// </content>
    public partial class ApiVersionActionSelector
    {
        Cache cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiVersionActionSelector"/> class.
        /// </summary>
        /// <param name="actionDescriptorCollectionProvider">The <see cref="IActionDescriptorCollectionProvider "/> used to select candidate routes.</param>
        /// <param name="actionConstraintCache">The <see cref="ActionConstraintCache"/> that providers a set of <see cref="IActionConstraint"/> instances.</param>
        /// <param name="options">The <see cref="ApiVersioningOptions">options</see> associated with the action selector.</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        /// <param name="routePolicy">The <see cref="IApiVersionRoutePolicy">route policy</see> applied to candidate matches.</param>
        public ApiVersionActionSelector(
            IActionDescriptorCollectionProvider actionDescriptorCollectionProvider,
            ActionConstraintCache actionConstraintCache,
            IOptions<ApiVersioningOptions> options,
            ILoggerFactory loggerFactory,
            IApiVersionRoutePolicy routePolicy )
        {
            Arg.NotNull( actionDescriptorCollectionProvider, nameof( actionDescriptorCollectionProvider ) );
            Arg.NotNull( actionConstraintCache, nameof( actionConstraintCache ) );
            Arg.NotNull( options, nameof( options ) );
            Arg.NotNull( loggerFactory, nameof( loggerFactory ) );
            Arg.NotNull( routePolicy, nameof( routePolicy ) );

            this.actionDescriptorCollectionProvider = actionDescriptorCollectionProvider;
            this.actionConstraintCache = actionConstraintCache;
            this.options = options;
            Logger = loggerFactory.CreateLogger( GetType() );
            RoutePolicy = routePolicy;
        }

        Cache Current
        {
            get
            {
                var actions = actionDescriptorCollectionProvider.ActionDescriptors;
                var value = Volatile.Read( ref cache );

                if ( value != null && value.Version == actions.Version )
                {
                    return value;
                }

                value = new Cache( actions );
                Volatile.Write( ref cache, value );

                return value;
            }
        }

#pragma warning disable CA1724 // private type and will not cause a conflict
        sealed class Cache
#pragma warning restore CA1724
        {
            public Cache( ActionDescriptorCollection actions )
            {
                Contract.Requires( actions != null );

                Version = actions.Version;
                RouteKeys = IdentifyRouteKeysForActionSelection( actions );
                BuildOrderedSetOfKeysForRouteValues( actions );
            }

            public int Version { get; }

            public string[] RouteKeys { get; }

            public Dictionary<string[], List<ActionDescriptor>> OrdinalEntries { get; } = new Dictionary<string[], List<ActionDescriptor>>( StringArrayComparer.Ordinal );

            public Dictionary<string[], List<ActionDescriptor>> OrdinalIgnoreCaseEntries { get; } = new Dictionary<string[], List<ActionDescriptor>>( StringArrayComparer.OrdinalIgnoreCase );

            static string[] IdentifyRouteKeysForActionSelection( ActionDescriptorCollection actions )
            {
                Contract.Requires( actions != null );
                Contract.Ensures( Contract.Result<string[]>() != null );

                var routeKeys = new HashSet<string>( StringComparer.OrdinalIgnoreCase );

                for ( var i = 0; i < actions.Items.Count; i++ )
                {
                    var action = actions.Items[i];

                    if ( action.AttributeRouteInfo == null )
                    {
                        foreach ( var kvp in action.RouteValues )
                        {
                            routeKeys.Add( kvp.Key );
                        }
                    }
                }

                return routeKeys.ToArray();
            }

            void BuildOrderedSetOfKeysForRouteValues( ActionDescriptorCollection actions )
            {
                Contract.Requires( actions != null );

                for ( var i = 0; i < actions.Items.Count; i++ )
                {
                    var action = actions.Items[i];

                    if ( action.AttributeRouteInfo != null )
                    {
                        continue;
                    }

                    var routeValues = new string[RouteKeys.Length];

                    for ( var j = 0; j < RouteKeys.Length; j++ )
                    {
                        action.RouteValues.TryGetValue( RouteKeys[j], out routeValues[j] );
                    }

                    if ( !OrdinalIgnoreCaseEntries.TryGetValue( routeValues, out var entries ) )
                    {
                        entries = new List<ActionDescriptor>();
                        OrdinalIgnoreCaseEntries.Add( routeValues, entries );
                    }

                    entries.Add( action );

                    if ( !OrdinalEntries.ContainsKey( routeValues ) )
                    {
                        OrdinalEntries.Add( routeValues, entries );
                    }
                }
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

                var hash = 0;
                var i = 0;

                for ( ; i < obj.Length; i++ )
                {
                    if ( obj[i] != null )
                    {
                        hash = valueComparer.GetHashCode( obj[i] );
                        break;
                    }
                }

                for ( ; i < obj.Length; i++ )
                {
                    if ( obj[i] != null )
                    {
                        hash = ( hash * 397 ) ^ valueComparer.GetHashCode( obj[i] );
                    }
                }

                return hash;
            }
        }
    }
}
#endif