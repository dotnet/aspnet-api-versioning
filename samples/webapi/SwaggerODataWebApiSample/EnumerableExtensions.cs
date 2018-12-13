namespace Microsoft.Examples
{
    using System;
    using System.Collections;

    /// <summary>
    /// Provides extension methods for the <see cref="IEnumerable"/> interface.
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Returns the first element from the specified sequence.
        /// </summary>
        /// <param name="enumerable">The <see cref="IEnumerable">sequence</see> to take an element from.</param>
        /// <returns>The first element in the sequence or <c>null</c>.</returns>
        public static object FirstOrDefault( this IEnumerable enumerable )
        {
            var iterator = enumerable.GetEnumerator();

            try
            {
                if ( iterator.MoveNext() )
                {
                    return iterator.Current;
                }
            }
            finally
            {
                ( iterator as IDisposable )?.Dispose();
            }

            return default( object );
        }

        /// <summary>
        /// Returns a single element from the specified sequence.
        /// </summary>
        /// <param name="enumerable">The <see cref="IEnumerable">sequence</see> to take an element from.</param>
        /// <returns>The single element in the sequence or <c>null</c>.</returns>
        public static object SingleOrDefault( this IEnumerable enumerable )
        {
            var iterator = enumerable.GetEnumerator();
            var result = default( object );

            try
            {
                if ( iterator.MoveNext() )
                {
                    result = iterator.Current;

                    if ( iterator.MoveNext() )
                    {
                        throw new InvalidOperationException( "The sequence contains more than one element." );
                    }
                }
            }
            finally
            {
                ( iterator as IDisposable )?.Dispose();
            }

            return result;
        }
    }
}