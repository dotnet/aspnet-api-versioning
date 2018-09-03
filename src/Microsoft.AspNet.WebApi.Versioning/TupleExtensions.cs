namespace Microsoft.Web.Http
{
    using System;
    using System.Diagnostics.Contracts;

    internal static class TupleExtensions
    {
        public static void Deconstruct<T1, T2, T3, T4>( this Tuple<T1, T2, T3, T4> tuple, out T1 item1, out T2 item2, out T3 item3, out T4 item4 )
        {
            Contract.Requires( tuple != null );

            item1 = tuple.Item1;
            item2 = tuple.Item2;
            item3 = tuple.Item3;
            item4 = tuple.Item4;
        }
    }
}