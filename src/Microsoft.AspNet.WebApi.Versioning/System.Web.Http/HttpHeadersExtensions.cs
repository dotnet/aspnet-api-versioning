namespace System.Web.Http
{
    using Collections.Generic;
    using Diagnostics.Contracts;
    using Linq;
    using Net.Http.Headers;
    using System;

    internal static partial class HttpHeadersExtensions
    {
        internal static string FirstOrDefault( this HttpHeaders headers, string name ) => headers.FirstOrDefault( name, () => default( string ) );

        internal static string FirstOrDefault( this HttpHeaders headers, string name, Func<string> valueFactory )
        {
            Contract.Requires( headers != null );
            Contract.Requires( !string.IsNullOrEmpty( name ) );
            Contract.Requires( valueFactory != null );

            IEnumerable<string> values;
            return headers.TryGetValues( name, out values ) ? ( values.FirstOrDefault() ?? valueFactory() ) : valueFactory();
        }
    }
}
