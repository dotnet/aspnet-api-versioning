// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using System.Globalization;
#if NETSTANDARD
using DateOnly = System.DateTime;
#endif

/// <summary>
/// Represents an application programming interface (API) version.
/// </summary>
public partial class ApiVersion : IEquatable<ApiVersion>, IComparable<ApiVersion>, IFormattable
{
    private static ApiVersion? @default;
    private static ApiVersion? neutral;
    private int hashCode;

    private ApiVersion()
    {
        const int Major = int.MaxValue;
        const int Minor = int.MaxValue;
        var group = DateOnly.MaxValue;
        hashCode = HashCode.Combine( group, Major, Minor );
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiVersion"/> class.
    /// </summary>
    /// <param name="groupVersion">The group version.</param>
    /// <param name="status">The optional version status.</param>
    public ApiVersion( DateOnly groupVersion, string? status = default )
        : this( new DateOnly?( groupVersion ), null, null, status ) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiVersion"/> class.
    /// </summary>
    /// <param name="majorVersion">The major version.</param>
    /// <param name="minorVersion">The optional minor version.</param>
    /// <param name="status">The optional version status.</param>
    public ApiVersion( int majorVersion, int? minorVersion = default, string? status = default )
        : this( null, new int?( majorVersion ), minorVersion, status ) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiVersion"/> class.
    /// </summary>
    /// <param name="groupVersion">The group version.</param>
    /// <param name="majorVersion">The major version.</param>
    /// <param name="minorVersion">The minor version.</param>
    /// <param name="status">The optional version status.</param>
    public ApiVersion( DateOnly groupVersion, int majorVersion, int minorVersion, string? status = default )
        : this( new DateOnly?( groupVersion ), new int?( majorVersion ), new int?( minorVersion ), status ) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiVersion"/> class.
    /// </summary>
    /// <param name="version">The version number.</param>
    /// <param name="status">The optional version status.</param>
    public ApiVersion( double version, string? status = default )
        : this( version, status, IsValidStatus ) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiVersion"/> class.
    /// </summary>
    /// <param name="version">The version number.</param>
    /// <param name="status">The optional version status.</param>
    /// <param name="isValidStatus">The function used to valid status.</param>
    protected ApiVersion( double version, string? status, Func<string?, bool> isValidStatus )
    {
        if ( version < 0d || double.IsNaN( version ) || double.IsInfinity( version ) )
        {
            throw new ArgumentOutOfRangeException( nameof( version ) );
        }

        Status = ValidateStatus(
            status,
            isValidStatus ?? throw new System.ArgumentNullException( nameof( isValidStatus ) ) );

        var number = new decimal( version );
        var bits = decimal.GetBits( number );
        var scale = ( bits[3] >> 16 ) & 31;
        var major = decimal.Truncate( number );
        var minor = (int) ( ( number - major ) * new decimal( Math.Pow( 10, scale ) ) );

        MajorVersion = (int) major;
        MinorVersion = minor;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiVersion"/> class.
    /// </summary>
    /// <param name="groupVersion">The optional group version.</param>
    /// <param name="majorVersion">The optional major version.</param>
    /// <param name="minorVersion">The optional minor version.</param>
    /// <param name="status">The optional version status.</param>
    /// <param name="isValidStatus">The optional function used to valid status.
    /// The default value is <see cref="IsValidStatus(string?)"/>.</param>
    protected internal ApiVersion(
        DateOnly? groupVersion,
        int? majorVersion,
        int? minorVersion,
        string? status,
        Func<string?, bool>? isValidStatus = default )
    {
        if ( majorVersion.HasValue && majorVersion.Value < 0 )
        {
            throw new ArgumentOutOfRangeException( nameof( majorVersion ) );
        }

        if ( minorVersion.HasValue && minorVersion.Value < 0 )
        {
            throw new ArgumentOutOfRangeException( nameof( minorVersion ) );
        }

        Status = ValidateStatus( status, isValidStatus ?? IsValidStatus );
        GroupVersion = groupVersion;
        MajorVersion = majorVersion;
        MinorVersion = minorVersion;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiVersion"/> class.
    /// </summary>
    /// <param name="other">The instance to derive from.</param>
    protected ApiVersion( ApiVersion other )
    {
        ArgumentNullException.ThrowIfNull( other );

        hashCode = other.hashCode;
        GroupVersion = other.GroupVersion;
        MajorVersion = other.MajorVersion;
        MinorVersion = other.MinorVersion;
        Status = other.Status;
    }

    /// <summary>
    /// Gets the default API version.
    /// </summary>
    /// <value>The default <see cref="ApiVersion">API version</see>, which is always "1.0".</value>
    public static ApiVersion Default => @default ??= new( 1, 0 );

    /// <summary>
    /// Gets the neutral API version.
    /// </summary>
    /// <value>The neutral <see cref="ApiVersion">API version</see>.</value>
    public static ApiVersion Neutral => neutral ??= new();

    /// <summary>
    /// Gets the group version.
    /// </summary>
    /// <value>The group version or null.</value>
    /// <remarks>If the group version is specified, only the date component is considered.</remarks>
    public DateOnly? GroupVersion { get; }

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

    private int ImpliedMinorVersion => MinorVersion ?? 0;

    /// <summary>
    /// Gets the optional version status.
    /// </summary>
    /// <value>The version status.</value>
    /// <remarks>The version status typically allows services to indicate pre-release or test
    /// versions that are not release quality or guaranteed to be supported. Example values
    /// might include "Alpha", "Beta", "RC", etc.</remarks>
    public string? Status { get; }

    /// <summary>
    /// Returns the text representation of the version using the specified format and format provider.
    /// <seealso cref="ApiVersionFormatProvider"/></summary>
    /// <param name="format">The format to return the text representation in. The value can be <c>null</c> or empty.</param>
    /// <returns>The <see cref="string">string</see> representation of the version.</returns>
    /// <exception cref="FormatException">The specified <paramref name="format"/> is not one of the supported format values.</exception>
    public virtual string ToString( string format ) => ToString( format, CultureInfo.InvariantCulture );

    /// <inheritdoc />
    public override string ToString() => ToString( null, CultureInfo.InvariantCulture );

    /// <inheritdoc />
    public override bool Equals( object? obj ) => Equals( obj as ApiVersion );

    /// <inheritdoc />
    public override int GetHashCode()
    {
        // perf: api version is used in a lot sets and as a dictionary keys
        // since it's immutable, calculate the hash code once and reuse it
        if ( hashCode != default )
        {
            return hashCode;
        }

        var hash = default( HashCode );

        if ( GroupVersion.HasValue )
        {
            hash.Add( GroupVersion.Value );
        }

        if ( MajorVersion.HasValue )
        {
            hash.Add( MajorVersion.Value );
            hash.Add( ImpliedMinorVersion );
        }

        if ( !string.IsNullOrEmpty( Status ) )
        {
            hash.Add( Status, StringComparer.OrdinalIgnoreCase );
        }

        return hashCode = hash.ToHashCode();
    }

    /// <summary>
    /// Overloads the equality operator.
    /// </summary>
    /// <param name="version1">The <see cref="ApiVersion"/> to compare.</param>
    /// <param name="version2">The <see cref="ApiVersion"/> to compare against.</param>
    /// <returns>True if the objects are equal; otherwise, false.</returns>
    public static bool operator ==( ApiVersion? version1, ApiVersion? version2 ) =>
        version1 is null ? version2 is null : version1.Equals( version2 );

    /// <summary>
    /// Overloads the inequality operator.
    /// </summary>
    /// <param name="version1">The <see cref="ApiVersion"/> to compare.</param>
    /// <param name="version2">The <see cref="ApiVersion"/> to compare against.</param>
    /// <returns>True if the objects are not equal; otherwise, false.</returns>
    public static bool operator !=( ApiVersion? version1, ApiVersion? version2 ) =>
            version1 is null ? version2 is not null : !version1.Equals( version2 );

    /// <summary>
    /// Overloads the less than operator.
    /// </summary>
    /// <param name="version1">The <see cref="ApiVersion"/> to compare.</param>
    /// <param name="version2">The <see cref="ApiVersion"/> to compare against.</param>
    /// <returns>True the first object is less than the second object; otherwise, false.</returns>
    public static bool operator <( ApiVersion? version1, ApiVersion? version2 ) =>
        version1 is null ? version2 is not null : version1.CompareTo( version2 ) < 0;

    /// <summary>
    /// Overloads the less than or equal to operator.
    /// </summary>
    /// <param name="version1">The <see cref="ApiVersion"/> to compare.</param>
    /// <param name="version2">The <see cref="ApiVersion"/> to compare against.</param>
    /// <returns>True the first object is less than or equal to the second object; otherwise, false.</returns>
    public static bool operator <=( ApiVersion? version1, ApiVersion? version2 ) =>
        version1 is null || version1.CompareTo( version2 ) <= 0;

    /// <summary>
    /// Overloads the greater than operator.
    /// </summary>
    /// <param name="version1">The <see cref="ApiVersion"/> to compare.</param>
    /// <param name="version2">The <see cref="ApiVersion"/> to compare against.</param>
    /// <returns>True the first object is greater than the second object; otherwise, false.</returns>
    public static bool operator >( ApiVersion? version1, ApiVersion? version2 ) =>
        version1 is not null && version1.CompareTo( version2 ) > 0;

    /// <summary>
    /// Overloads the greater than or equal to operator.
    /// </summary>
    /// <param name="version1">The <see cref="ApiVersion"/> to compare.</param>
    /// <param name="version2">The <see cref="ApiVersion"/> to compare against.</param>
    /// <returns>True the first object is greater than or equal to the second object; otherwise, false.</returns>
    public static bool operator >=( ApiVersion? version1, ApiVersion? version2 ) =>
        version1 is null ? version2 is null : version1.CompareTo( version2 ) >= 0;

    /// <inheritdoc />
    public virtual bool Equals( ApiVersion? other ) => other is not null && GetHashCode() == other.GetHashCode();

    /// <inheritdoc />
    public virtual int CompareTo( ApiVersion? other )
    {
        if ( other == null )
        {
            return 1;
        }

        var result = Nullable.Compare( GroupVersion, other.GroupVersion );

        if ( result != 0 )
        {
            return result;
        }

        result = Nullable.Compare( MajorVersion, other.MajorVersion );

        if ( result != 0 )
        {
            return result;
        }

        result = ImpliedMinorVersion.CompareTo( other.ImpliedMinorVersion );

        if ( result != 0 )
        {
            return result;
        }

        if ( string.IsNullOrEmpty( Status ) )
        {
            if ( !string.IsNullOrEmpty( other.Status ) )
            {
                result = 1;
            }
        }
        else if ( string.IsNullOrEmpty( other.Status ) )
        {
            result = -1;
        }
        else
        {
            result = StringComparer.OrdinalIgnoreCase.Compare( Status, other.Status );

            if ( result < 0 )
            {
                result = -1;
            }
            else if ( result > 0 )
            {
                result = 1;
            }
        }

        return result;
    }

    /// <inheritdoc />
    public virtual string ToString( string? format, IFormatProvider? formatProvider )
    {
        var provider = ApiVersionFormatProvider.GetInstance( formatProvider );
#pragma warning disable IDE0079
#pragma warning disable CA1062 // Validate arguments of public methods
        return provider.Format( format, this, formatProvider );
#pragma warning restore CA1062 // Validate arguments of public methods
#pragma warning restore IDE0079
    }

    private static string? ValidateStatus( string? status, Func<string?, bool> isValid )
    {
        if ( isValid( status ) )
        {
            return status;
        }

        var message = string.Format( CultureInfo.CurrentCulture, Format.ApiVersionBadStatus, status );
        throw new ArgumentException( message, nameof( status ) );
    }
}