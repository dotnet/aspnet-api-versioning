namespace Microsoft.AspNetCore.Mvc
{
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
        readonly List<ChangeToken> changeTokens = new List<ChangeToken>();
        readonly object syncRoot = new object();

        ODataActionDescriptorChangeProvider() { }

        /// <summary>
        /// Gets the change provider instance.
        /// </summary>
        /// <value>The singleton <see cref="ODataActionDescriptorChangeProvider"/> instance.</value>
        public static ODataActionDescriptorChangeProvider Instance { get; } = new ODataActionDescriptorChangeProvider();

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
                HasChanged = true;

                var tokens = changeTokens.ToArray();

                changeTokens.Clear();

                try
                {
                    for ( var i = 0; i < tokens.Length; i++ )
                    {
                        var token = tokens[i];
                        token.Callback();
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
            readonly HashSet<(Action<object> Callback, object State)> callbacks = new HashSet<(Action<object> Callback, object State)>();
            readonly object syncRoot = new object();

            internal ChangeToken( ODataActionDescriptorChangeProvider provider ) => this.provider = provider;

            public bool HasChanged => provider.HasChanged;

            public bool ActiveChangeCallbacks => true;

            public IDisposable RegisterChangeCallback( Action<object> callback, object state )
            {
                var item = (callback, state);

                lock ( syncRoot )
                {
                    callbacks.Add( item );
                }

                return new ChangeSubscription( this, item );
            }

            internal void Remove( (Action<object> Callback, object State) item )
            {
                lock ( syncRoot )
                {
                    callbacks.Remove( item );
                }
            }

            internal void Callback()
            {
                var items = default( (Action<object> Callback, object State)[] );

                lock ( syncRoot )
                {
                    items = callbacks.ToArray();
                    callbacks.Clear();
                }

                for ( var i = 0; i < items.Length; i++ )
                {
                    var item = items[i];
                    item.Callback( item.State );
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