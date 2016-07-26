namespace Microsoft
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using static System.Globalization.CultureInfo;

    internal static class StringExtensions
    {
        [Pure]
        [SuppressMessage( "Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Shared source, but not libraries. May not be used in call links." )]
        internal static string FormatInvariant( this string format, params object[] args ) => string.Format( InvariantCulture, format, args );

        [Pure]
        [SuppressMessage( "Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Shared source, but not libraries. May not be used in call links." )]
        public static string FormatDefault( this string format, params object[] args ) => string.Format( CurrentCulture, format, args );
    }
}
