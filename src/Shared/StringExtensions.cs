namespace Microsoft
{
    using static System.Globalization.CultureInfo;

    static class StringExtensions
    {
        internal static string FormatInvariant( this string format, params object?[] args ) => string.Format( InvariantCulture, format, args );

        internal static string FormatDefault( this string format, params object?[] args ) => string.Format( CurrentCulture, format, args );
    }
}