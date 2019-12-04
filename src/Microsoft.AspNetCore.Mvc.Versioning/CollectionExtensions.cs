namespace Microsoft.AspNetCore.Mvc
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    static partial class CollectionExtensions
    {
#if NETCOREAPP3_1
        [return: MaybeNull]
        internal static TValue GetOrDefault<TKey, TValue>( this IDictionary<TKey, object?> dictionary, TKey key, [AllowNull] TValue defaultValue ) where TKey : notnull =>
#else
        internal static TValue GetOrDefault<TKey, TValue>( this IDictionary<TKey, object?> dictionary, TKey key, TValue defaultValue ) where TKey : notnull =>
#endif
            dictionary.TryGetValue( key, out TValue value ) ? value : defaultValue;

#if NETCOREAPP3_1
        [return: MaybeNull]
#endif
        internal static TValue GetOrDefault<TKey, TValue>( this IDictionary<TKey, object?> dictionary, TKey key, Func<TValue> defaultValue ) where TKey : notnull =>
            dictionary.TryGetValue( key, out TValue value ) ? value : defaultValue();

#if NETCOREAPP3_1
        internal static void SetOrRemove<TKey, TValue>( this IDictionary<TKey, object?> dictionary, TKey key, [AllowNull] TValue value ) where TKey : notnull
#else
        internal static void SetOrRemove<TKey, TValue>( this IDictionary<TKey, object?> dictionary, TKey key, TValue value ) where TKey : notnull
#endif
        {
            if ( !typeof( TValue ).IsValueType && value is null )
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
            collection.Add( item );
            return item;
        }
    }
}