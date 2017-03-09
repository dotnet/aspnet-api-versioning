namespace Microsoft.AspNetCore.Mvc
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;

    static partial class CollectionExtensions
    {
        internal static TValue GetOrDefault<TKey, TValue>( this IDictionary<TKey, object> dictionary, TKey key, TValue defaultValue ) =>
            dictionary.TryGetValue( key, out TValue value ) ? value : defaultValue;

        internal static TValue GetOrDefault<TKey, TValue>( this IDictionary<TKey, object> dictionary, TKey key, Func<TValue> defaultValue )
        {
            Contract.Requires( defaultValue != null );
            return dictionary.TryGetValue( key, out TValue value ) ? value : defaultValue();
        }

        internal static void SetOrRemove<TKey, TValue>( this IDictionary<TKey, object> dictionary, TKey key, TValue value ) where TValue : class
        {
            Contract.Requires( dictionary != null );

            if ( value == null )
            {
                dictionary.Remove( key );
            }
            else
            {
                dictionary[key] = value;
            }
        }

        internal static T AddAndReturn<T>( this ICollection<T> collection, T item )
        {
            Contract.Requires( collection != null );
            Contract.Ensures( Contract.Result<T>() != null );
            collection.Add( item );
            return item;
        }
    }
}