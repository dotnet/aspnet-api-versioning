namespace Microsoft.AspNetCore.Mvc
{
    using Microsoft.AspNet.OData.Routing;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.Extensions.Primitives;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using static System.ComponentModel.EditorBrowsableState;

    /// <summary>
    /// Represents a <see cref="IActionDescriptorChangeProvider">change provider</see> that notifies consumers when
    /// OData action descriptors have changed.
    /// </summary>
    /// <remarks>This class is used to notify consumers when OData route information is available after one or more
    /// MapVersionedODataRoute or MapVersionedODataRoutes calls have been made.</remarks>
    public sealed class ODataActionDescriptorChangeProvider : IActionDescriptorChangeProvider
    {
        readonly object syncRoot = new();
        readonly IODataRouteCollectionProvider routeCollectionProvider;
        int version;
        List<ChangeToken>? changeTokens;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataActionDescriptorChangeProvider"/> class.
        /// </summary>
        /// <param name="routeCollectionProvider">The corresponding <see cref="IODataRouteCollectionProvider">
        /// OData route collection provider</see>.</param>
        [CLSCompliant( false )]
        public ODataActionDescriptorChangeProvider( IODataRouteCollectionProvider routeCollectionProvider ) =>
            this.routeCollectionProvider = routeCollectionProvider;

        int CurrentVersion => routeCollectionProvider.Items.Count;

        internal bool HasChanged { get; private set; }

        /// <summary>
        /// Gets a change token from the provider.
        /// </summary>
        /// <returns>A <see cref="IChangeToken">change token</see> that can be used to subscribe to changes.</returns>
        [CLSCompliant( false )]
        public IChangeToken GetChangeToken()
        {
            var changeToken = new ChangeToken( this );

            lock ( syncRoot )
            {
                changeTokens ??= new();
                changeTokens.Add( changeToken );
            }

            return changeToken;
        }

        /// <summary>
        /// Notifies any consumers that the OData action descriptors have changed.
        /// </summary>
        /// <remarks>This method supports API versioning for OData and is not meant to be called directly by your code.</remarks>
        [EditorBrowsable( Never )]
        public void NotifyChanged()
        {
            lock ( syncRoot )
            {
                if ( changeTokens == null )
                {
                    return;
                }

                var currentVersion = CurrentVersion;

                if ( version == currentVersion )
                {
                    return;
                }

                version = currentVersion;
                HasChanged = true;

                var tokens = changeTokens.ToArray();

                changeTokens.Clear();

                try
                {
                    for ( var i = 0; i < tokens.Length; i++ )
                    {
                        tokens[i].Callback();
                    }
                }
                finally
                {
                    HasChanged = false;
                }
            }
        }

        sealed class ChangeToken : IChangeToken
        {
            readonly ODataActionDescriptorChangeProvider provider;
            readonly object syncRoot = new();
            HashSet<(Action<object> Callback, object State)>? callbacks;

            internal ChangeToken( ODataActionDescriptorChangeProvider provider ) => this.provider = provider;

            public bool HasChanged => provider.HasChanged;

            public bool ActiveChangeCallbacks => true;

            public IDisposable RegisterChangeCallback( Action<object> callback, object state )
            {
                var item = (callback, state);

                lock ( syncRoot )
                {
                    callbacks ??= new();
                    callbacks.Add( item );
                }

                return new ChangeSubscription( this, item );
            }

            internal void Remove( (Action<object> Callback, object State) item )
            {
                lock ( syncRoot )
                {
                    if ( callbacks != null )
                    {
                        callbacks.Remove( item );
                    }
                }
            }

            internal void Callback()
            {
                var items = default( (Action<object> Callback, object State)[] );

                lock ( syncRoot )
                {
                    if ( callbacks == null )
                    {
                        items = Array.Empty<(Action<object> Callback, object State)>();
                    }
                    else
                    {
                        items = callbacks.ToArray();
                        callbacks.Clear();
                    }
                }

                for ( var i = 0; i < items.Length; i++ )
                {
                    var (callback, state) = items[i];
                    callback( state );
                }
            }
        }

        sealed class ChangeSubscription : IDisposable
        {
            readonly ChangeToken changeToken;
            readonly (Action<object>, object) callback;
            bool disposed;

            internal ChangeSubscription( ChangeToken changeToken, (Action<object> Func, object State) callback )
            {
                this.changeToken = changeToken;
                this.callback = callback;
            }

            public void Dispose()
            {
                if ( disposed )
                {
                    return;
                }

                disposed = true;
                changeToken.Remove( callback );
            }
        }
    }
}