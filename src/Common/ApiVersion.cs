#if WEBAPI
namespace Microsoft.Web.Http
#else
namespace Microsoft.AspNetCore.Mvc
#endif
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using System.Globalization;
    using System.Text;
    using static System.DateTime;
    using static System.Globalization.CultureInfo;
    using static System.String;
    using static System.Text.RegularExpressions.Regex;
    using static System.Text.RegularExpressions.RegexOptions;

    /// <summary>
    /// Represents the application programming interface (API) version of a service.
    /// </summary>
    public class ApiVersion : IEquatable<ApiVersion>, IComparable<ApiVersion>, IFormattable
    {
        const string ParsePattern = @"^(\d{4}-\d{2}-\d{2})?\.?(\d{0,9})\.?(\d{0,9})\.?-?(.*)$";
        const string GroupVersionFormat = "yyyy-MM-dd";
        static Lazy<ApiVersion> defaultVersion = new Lazy<ApiVersion>( () => new ApiVersion( 1, 0 ) );

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiVersion"/> class.
        /// </summary>
        /// <param name="groupVersion">The group version.</param>
        public ApiVersion( DateTime groupVersion )
            : this( new DateTime?( groupVersion ), null, null, null ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiVersion"/> class.
        /// </summary>
        /// <param name="groupVersion">The group version.</param>
        /// <param name="status">The version status.</param>
        public ApiVersion( DateTime groupVersion, string status )
            : this( new DateTime?( groupVersion ), null, null, status )
        {
            Arg.NotNullOrEmpty( status, nameof( status ) );
            RequireValidStatus( status );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiVersion"/> class.
        /// </summary>
        /// <param name="majorVersion">The major version.</param>
        /// <param name="minorVersion">The minor version.</param>
        public ApiVersion( int majorVersion, int minorVersion )
            : this( null, new int?( majorVersion ), new int?( minorVersion ), null )
        {
            Arg.InRange( majorVersion, 0, int.MaxValue, nameof( majorVersion ) );
            Arg.InRange( minorVersion, 0, int.MaxValue, nameof( minorVersion ) );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiVersion"/> class.
        /// </summary>
        /// <param name="majorVersion">The major version.</param>
        /// <param name="minorVersion">The minor version.</param>
        /// <param name="status">The version status.</param>
        public ApiVersion( int majorVersion, int minorVersion, string status )
            : this( null, new int?( majorVersion ), new int?( minorVersion ), status )
        {
            Arg.InRange( majorVersion, 0, int.MaxValue, nameof( majorVersion ) );
            Arg.InRange( minorVersion, 0, int.MaxValue, nameof( minorVersion ) );
            Arg.NotNullOrEmpty( status, nameof( status ) );
            RequireValidStatus( status );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiVersion"/> class.
        /// </summary>
        /// <param name="groupVersion">The group version.</param>
        /// <param name="majorVersion">The major version.</param>
        /// <param name="minorVersion">The minor version.</param>
        public ApiVersion( DateTime groupVersion, int majorVersion, int minorVersion )
            : this( new DateTime?( groupVersion ), new int?( majorVersion ), new int?( minorVersion ), null )
        {
            Arg.InRange( majorVersion, 0, int.MaxValue, nameof( majorVersion ) );
            Arg.InRange( minorVersion, 0, int.MaxValue, nameof( minorVersion ) );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiVersion"/> class.
        /// </summary>
        /// <param name="groupVersion">The group version.</param>
        /// <param name="majorVersion">The major version.</param>
        /// <param name="minorVersion">The minor version.</param>
        /// <param name="status">The version status.</param>
        public ApiVersion( DateTime groupVersion, int majorVersion, int minorVersion, string status )
            : this( new DateTime?( groupVersion ), new int?( majorVersion ), new int?( minorVersion ), status )
        {
            Arg.InRange( majorVersion, 0, int.MaxValue, nameof( majorVersion ) );
            Arg.InRange( minorVersion, 0, int.MaxValue, nameof( minorVersion ) );
            Arg.NotNullOrEmpty( status, nameof( status ) );
            RequireValidStatus( status );
        }

        internal ApiVersion( DateTime? groupVersion, int? majorVersion, int? minorVersion, string status )
        {
            GroupVersion = groupVersion;
            MajorVersion = majorVersion;
            MinorVersion = minorVersion;
            Status = IsNullOrEmpty( status ) ? null : status;
        }

        [DebuggerStepThrough]
        [ContractArgumentValidator]
        static void RequireValidStatus( string status )
        {
            if ( !IsValidStatus( status ) )
            {
                throw new ArgumentException( SR.ApiVersionBadStatus.FormatDefault( status ), nameof( status ) );
            }
            Contract.EndContractBlock();
        }

        /// <summary>
        /// Gets the default API version.
        /// </summary>
        /// <value>The default <see cref="ApiVersion">API version</see>, which is always "1.0".</value>
        public static ApiVersion Default => defaultVersion.Value;

        /// <summary>
        /// Gets the group version.
        /// </summary>
        /// <value>The group version or null.</value>
        /// <remarks>If the group version is specified, only the date component is considered.</remarks>
        public DateTime? GroupVersion { get; }

        /// <summary>
        /// Gets the major version number.
        /// </summary>
        /// <value>The major version number or <c>null</c>.</value>
        public int? MajorVersion { get; }

        /// <summary>
        /// Gets the minor version number.
        /// </summary>
        /// <value>The minor version number or <c>null</c>.</value>
        public int? MinorVersion { get; }

        int ImpliedMinorVersion => MinorVersion ?? 0;

        /// <summary>
        /// Gets the optional version status.
        /// </summary>
        /// <value>The version status.</value>
        /// <remarks>The version status typically allows services to indicate pre-release or test
        /// versions that are not release quality or guaranteed to be supported. Example values
        /// might include "Alpha", "Beta", "RC", etc.</remarks>
        public string Status { get; }

        /// <summary>
        /// Gets a value indicating whether the specified status is valid.
        /// </summary>
        /// <param name="status">The status to evaluate.</param>
        /// <returns>True if the status is valid; otherwise, false.</returns>
        /// <remarks>The status must be alphabetic or alpanumeric, start with a letter, and contain no spaces.</remarks>
        [Pure]
        public static bool IsValidStatus( string status ) => IsNullOrEmpty( status ) ? false : IsMatch( status, @"^[a-zA-Z][a-zA-Z0-9]*$", Singleline );

        /// <summary>
        /// Parses the specified text into an API version.
        /// </summary>
        /// <param name="text">The text to parse.</param>
        /// <returns>The parsed <see cref="ApiVersion">API version</see>.</returns>
        /// <exception cref="FormatException">The specified group version or version status is invalid.</exception>
        public static ApiVersion Parse( string text )
        {
            Arg.NotNullOrEmpty( text, nameof( text ) );
            Contract.Ensures( Contract.Result<ApiVersion>() != null );

            var match = Match( text, ParsePattern, Singleline );

            if ( !match.Success )
            {
                throw new FormatException( SR.ApiVersionInvalidFormat );
            }

            var status = default( string );

            if ( match.Groups[4].Success )
            {
                status = match.Groups[4].Value;

                if ( !IsNullOrEmpty( status ) && !IsValidStatus( status ) )
                {
                    throw new FormatException( SR.ApiVersionBadStatus.FormatDefault( status ) );
                }
            }

            var culture = InvariantCulture;
            var group = default( DateTime? );
            var major = default( int? );
            var minor = default( int? );

            if ( match.Groups[1].Success )
            {
                if ( !TryParseExact( match.Groups[1].Value, GroupVersionFormat, culture, DateTimeStyles.None, out var temp ) )
                {
                    throw new FormatException( SR.ApiVersionBadGroupVersion.FormatDefault( match.Groups[1].Value ) );
                }

                group = temp;
            }

            var matchGroup = match.Groups[2];

            if ( matchGroup.Success && matchGroup.Length > 0 )
            {
                major = int.Parse( matchGroup.Value, culture );
            }

            matchGroup = match.Groups[3];

            if ( matchGroup.Success && matchGroup.Length > 0 )
            {
                minor = int.Parse( matchGroup.Value, culture );
            }

            if ( group == null && major == null && minor == null )
            {
                throw new FormatException( SR.ApiVersionInvalidFormat );
            }

            return new ApiVersion( group, major, minor, status );
        }

        /// <summary>
        /// Attempts to parse the specified text into an API version.
        /// </summary>
        /// <param name="text">The text to parse.</param>
        /// <param name="version">The parsed <see cref="ApiVersion">API version</see>, if the operation is successful.</param>
        /// <returns>True if the operation succeeded; otherwise false.</returns>
        public static bool TryParse( string text, out ApiVersion version )
        {
            Contract.Ensures( ( Contract.Result<bool>() && Contract.ValueAtReturn( out version ) != null ) || ( !Contract.Result<bool>() && Contract.ValueAtReturn( out version ) == null ) );

            version = null;

            if ( IsNullOrEmpty( text ) )
            {
                return false;
            }

            var match = Match( text, ParsePattern, Singleline );

            if ( !match.Success )
            {
                return false;
            }

            var status = default( string );

            if ( match.Groups[4].Success )
            {
                status = match.Groups[4].Value;

                if ( !IsNullOrEmpty( status ) && !IsValidStatus( status ) )
                {
                    return false;
                }
            }

            var culture = InvariantCulture;
            var group = default( DateTime? );
            var major = default( int? );
            var minor = default( int? );

            if ( match.Groups[1].Success )
            {
                if ( !TryParseExact( match.Groups[1].Value, GroupVersionFormat, culture, DateTimeStyles.None, out var temp ) )
                {
                    return false;
                }

                group = temp;
            }

            var matchGroup = match.Groups[2];

            if ( matchGroup.Success && matchGroup.Length > 0 )
            {
                major = int.Parse( matchGroup.Value, culture );
            }

            matchGroup = match.Groups[3];

            if ( matchGroup.Success && matchGroup.Length > 0 )
            {
                minor = int.Parse( matchGroup.Value, culture );
            }

            if ( group == null && major == null && minor == null )
            {
                return false;
            }

            version = new ApiVersion( group, major, minor, status );
            return true;
        }

        void AppendGroupVersion( StringBuilder text, IFormatProvider formatProvider )
        {
            Contract.Requires( text != null );

            if ( GroupVersion != null )
            {
                text.Append( GroupVersion.Value.ToString( GroupVersionFormat, formatProvider ) );
            }
        }

        void AppendMajorAndMinorVersion( StringBuilder text, IFormatProvider formatProvider )
        {
            Contract.Requires( text != null );

            if ( MajorVersion != null )
            {
                if ( text.Length > 0 )
                {
                    text.Append( '.' );
                }

                text.Append( MajorVersion.Value.ToString( formatProvider ) );

                if ( MinorVersion == null )
                {
                    return;
                }

                text.Append( '.' );
                text.Append( MinorVersion.Value.ToString( formatProvider ) );
            }
            else if ( MinorVersion != null )
            {
                text.Append( "0." );
                text.Append( MinorVersion.Value.ToString( formatProvider ) );
            }
        }

        void AppendStatus( StringBuilder text )
        {
            Contract.Requires( text != null );

            if ( text.Length > 0 && !IsNullOrEmpty( Status ) )
            {
                text.Append( '-' );
                text.Append( Status );
            }
        }

        /// <summary>
        /// Returns the text representation of the version using the specified format and format provider.
        /// </summary>
        /// <param name="format">The format to return the text representation in.</param>
        /// <returns>The <see cref="String">string</see> representation of the version.</returns>
        /// <remarks>The supported format codes are:
        /// <para>
        /// <list type="table">
        ///     <listheader>
        ///         <term>Format</term>
        ///         <description>Description</description>
        ///     </listheader>
        ///     <item>
        ///         <term>G, g</term>
        ///         <description>Returns only the <see cref="P:GroupVersion">group version</see>, if present.</description>
        ///     </item>
        ///     <item>
        ///         <term>V, v</term>
        ///         <description>Returns only the <see cref="P:MajorVersion">major</see> and <see cref="P:MinorVersion">minor</see> versions, if present.</description>
        ///     </item>
        ///     <item>
        ///         <term>S, s</term>
        ///         <description>Returns full API version with the <see cref="P:Status">status</see>, if <see cref="P:Status">status</see> is present.</description>
        ///     </item>
        ///     <item>
        ///         <term>F, f</term>
        ///         <description>Returns the full API version.</description>
        ///     </item>
        /// </list>
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">The specified <paramref name="format"/> is <c>null</c> or any empty string.</exception>
        /// <exception cref="FormatException">The specified <paramref name="format"/> is not one of the supported format values.</exception>
        public virtual string ToString( string format ) => ToString( format, InvariantCulture );

        /// <summary>
        /// Returns the text representation of the version.
        /// </summary>
        /// <returns>The <see cref="String">string</see> representation of the version.</returns>
        public override string ToString() => ToString( null, InvariantCulture );

        /// <summary>
        /// Determines whether the current object equals another object.
        /// </summary>
        /// <param name="obj">The <see cref="Object">object</see> to evaluate.</param>
        /// <returns>True if the specified objet is equal to the current instance; otherwise, false.</returns>
        public override bool Equals( object obj ) => Equals( obj as ApiVersion );

        /// <summary>
        /// Gets a hash code for the current instance.
        /// </summary>
        /// <returns>A hash code.</returns>
        /// <remarks>The hash code is based on the uppercase, invariant hash of the
        /// <see cref="M:ToString">text representation</see> of the object.</remarks>
        public override int GetHashCode() => ToString().ToUpperInvariant().GetHashCode();

        /// <summary>
        /// Overloads the equality operator.
        /// </summary>
        /// <param name="version1">The <see cref="ApiVersion"/> to compare.</param>
        /// <param name="version2">The <see cref="ApiVersion"/> to compare against.</param>
        /// <returns>True if the objects are equal; otherwise, false.</returns>
        public static bool operator ==( ApiVersion version1, ApiVersion version2 ) =>
            ReferenceEquals( version1, null ) ? ReferenceEquals( version2, null ) : version1.Equals( version2 );

        /// <summary>
        /// Overloads the inequality operator.
        /// </summary>
        /// <param name="version1">The <see cref="ApiVersion"/> to compare.</param>
        /// <param name="version2">The <see cref="ApiVersion"/> to compare against.</param>
        /// <returns>True if the objects are not equal; otherwise, false.</returns>
        public static bool operator !=( ApiVersion version1, ApiVersion version2 ) =>
             ReferenceEquals( version1, null ) ? !ReferenceEquals( version2, null ) : !version1.Equals( version2 );

        /// <summary>
        /// Overloads the less than operator.
        /// </summary>
        /// <param name="version1">The <see cref="ApiVersion"/> to compare.</param>
        /// <param name="version2">The <see cref="ApiVersion"/> to compare against.</param>
        /// <returns>True the first object is less than the second object; otherwise, false.</returns>
        public static bool operator <( ApiVersion version1, ApiVersion version2 ) =>
            ReferenceEquals( version1, null ) ? !ReferenceEquals( version2, null ) : version1.CompareTo( version2 ) < 0;

        /// <summary>
        /// Overloads the less than or equal to operator.
        /// </summary>
        /// <param name="version1">The <see cref="ApiVersion"/> to compare.</param>
        /// <param name="version2">The <see cref="ApiVersion"/> to compare against.</param>
        /// <returns>True the first object is less than or equal to the second object; otherwise, false.</returns>
        public static bool operator <=( ApiVersion version1, ApiVersion version2 ) =>
            ReferenceEquals( version1, null ) ? true : version1.CompareTo( version2 ) <= 0;

        /// <summary>
        /// Overloads the greater than operator.
        /// </summary>
        /// <param name="version1">The <see cref="ApiVersion"/> to compare.</param>
        /// <param name="version2">The <see cref="ApiVersion"/> to compare against.</param>
        /// <returns>True the first object is greater than the second object; otherwise, false.</returns>
        public static bool operator >( ApiVersion version1, ApiVersion version2 ) =>
            ReferenceEquals( version1, null ) ? false : version1.CompareTo( version2 ) > 0;

        /// <summary>
        /// Overloads the greater than or equal to operator.
        /// </summary>
        /// <param name="version1">The <see cref="ApiVersion"/> to compare.</param>
        /// <param name="version2">The <see cref="ApiVersion"/> to compare against.</param>
        /// <returns>True the first object is greater than or equal to the second object; otherwise, false.</returns>
        public static bool operator >=( ApiVersion version1, ApiVersion version2 ) =>
            ReferenceEquals( version1, null ) ? ReferenceEquals( version2, null ) : version1.CompareTo( version2 ) >= 0;

        /// <summary>
        /// Determines whether the current object equals another object.
        /// </summary>
        /// <param name="other">The <see cref="ApiVersion">other</see> to evaluate.</param>
        /// <returns>True if the specified objet is equal to the current instance; otherwise, false.</returns>
        public virtual bool Equals( ApiVersion other )
        {
            if ( other == null )
            {
                return false;
            }

            return Nullable.Equals( GroupVersion, other.GroupVersion ) &&
                   Nullable.Equals( MajorVersion, other.MajorVersion ) &&
                   ImpliedMinorVersion.Equals( other.ImpliedMinorVersion ) &&
                   string.Equals( Status, other.Status, StringComparison.OrdinalIgnoreCase );
        }

        /// <summary>
        /// Performs a comparison of the current object to another object and returns a value
        /// indicating whether the object is less than, greater than, or equal to the other.
        /// </summary>
        /// <param name="other">The <see cref="ApiVersion">other</see> object to compare to.</param>
        /// <returns>Zero if the objects are equal, one if the current object is greater than the
        /// <paramref name="other"/> object, or negative one if the current object is less than the
        /// <paramref name="other"/> object.</returns>
        /// <remarks>The version <see cref="P:Status">status</see> is not included in comparisons.</remarks>
        public virtual int CompareTo( ApiVersion other )
        {
            if ( other == null )
            {
                return 1;
            }

            var result = Nullable.Compare( GroupVersion, other.GroupVersion );

            if ( result == 0 )
            {
                result = Nullable.Compare( MajorVersion, other.MajorVersion );

                if ( result == 0 )
                {
                    result = ImpliedMinorVersion.CompareTo( other.ImpliedMinorVersion );

                    if ( result == 0 )
                    {
                        result = StringComparer.OrdinalIgnoreCase.Compare( Status, other.Status );

                        if ( result != 0 )
                        {
                            result = result < 0 ? -1 : 1;
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Returns the text representation of the version using the specified format and format provider.
        /// </summary>
        /// <param name="format">The format to return the text representation in.</param>
        /// <param name="formatProvider">The <see cref="IFormatProvider">format provider</see> used to generate text.
        /// This implementation should typically use an <see cref="P:CultureInfo.InvariantCulture">invariant culture</see>.</param>
        /// <returns>The <see cref="String">string</see> representation of the version.</returns>
        /// <remarks>The supported format codes are:
        /// <para>
        /// <list type="table">
        ///     <listheader>
        ///         <term>Format</term>
        ///         <description>Description</description>
        ///     </listheader>
        ///     <item>
        ///         <term>G, g</term>
        ///         <description>Returns only the <see cref="P:GroupVersion">group version</see>, if present.</description>
        ///     </item>
        ///     <item>
        ///         <term>V, v</term>
        ///         <description>Returns only the <see cref="P:MajorVersion">major</see> and <see cref="P:MinorVersion">minor</see> versions, if present.</description>
        ///     </item>
        ///     <item>
        ///         <term>S, s</term>
        ///         <description>Returns full API version with the <see cref="P:Status">status</see>, if <see cref="P:Status">status</see> is present.</description>
        ///     </item>
        ///     <item>
        ///         <term>F, f</term>
        ///         <description>Returns the full API version.</description>
        ///     </item>
        /// </list>
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">The specified <paramref name="format"/> is <c>null</c> or any empty string.</exception>
        /// <exception cref="FormatException">The specified <paramref name="format"/> is not one of the supported format values.</exception>
        public virtual string ToString( string format, IFormatProvider formatProvider )
        {
            // syntax := <group version>[.<major>.<minor>][-status] | [<group version>.]<major>.<minor>[-status]
            var text = new StringBuilder();

            switch ( format )
            {
                case "G":
                case "g":
                    AppendGroupVersion( text, formatProvider );
                    break;
                case "V":
                case "v":
                    AppendMajorAndMinorVersion( text, formatProvider );
                    break;
                case "S":
                case "s":
                    AppendGroupVersion( text, formatProvider );
                    AppendMajorAndMinorVersion( text, formatProvider );
                    break;
                case null:
                case "F":
                case "f":
                    AppendGroupVersion( text, formatProvider );
                    AppendMajorAndMinorVersion( text, formatProvider );
                    AppendStatus( text );
                    break;
                default:
                    throw new FormatException( SR.ApiVersionInvalidFormatCode.FormatDefault( format ) );
            }

            return text.ToString();
        }
    }
}