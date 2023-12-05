// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0079
#pragma warning disable SA1121

namespace Asp.Versioning;

using System.Globalization;
using System.Reflection;
using System.Text;
#if NETSTANDARD1_0
using Text = System.String;
#else
using Text = System.ReadOnlySpan<char>;
#endif

/// <summary>
/// Represents a format provider for <see cref="ApiVersion">API versions</see>.
/// </summary>
/// <remarks>
/// <para>This format provider supports the following custom format strings:</para>
/// <list type="table">
///     <listheader>
///         <term>Format specifier</term>
///         <description>Description</description>
///         <description>Examples</description>
///     </listheader>
///     <item>
///         <term>"F"</term>
///         <description>The full, formatted API version where optional, absent components are omitted.</description>
///         <description>
///             <para>2017-01-01 -> 2017-01-01</para>
///             <para>2017-01-01.1 -> 2017-01-01.1</para>
///             <para>2017-01-01.1.5-RC -> 2017-01-01.1.5-RC</para>
///             <para>2017-01-01-Beta -> 2017-01-01-Beta</para>
///             <para>1 -> 1</para>
///             <para>1.5 -> 1.5</para>
///             <para>1-Beta -> 1-Beta</para>
///             <para>0.9-Alpha -> 0.9-Alpha</para>
///         </description>
///     </item>
///     <item>
///         <term>"FF"</term>
///         <description>The full, formatted API version where optional components have default values.</description>
///         <description>
///             <para>2017-01-01 -> 2017-01-01</para>
///             <para>2017-01-01.1 -> 2017-01-01.1.0</para>
///             <para>2017-01-01.1.5-RC -> 2017-01-01.1.5-RC</para>
///             <para>2017-01-01-Beta -> 2017-01-01-Beta</para>
///             <para>1 -> 1.0</para>
///             <para>1.5 -> 1.5</para>
///             <para>1-Beta -> 1.0-Beta</para>
///             <para>0.9-Alpha -> 0.9-Alpha</para>
///         </description>
///     </item>
///     <item>
///         <term>"G"</term>
///         <description>The group version of the API group version.</description>
///         <description>
///             <para>2017-01-01 -> 2017-01-01</para>
///             <para>2017-01-01-RC -> 2017-01-01</para>
///             <para>2017-01-01.1.0 -> 2017-01-01</para>
///         </description>
///     </item>
///     <item>
///         <term>"GG"</term>
///         <description>The group version and status of the API group version.</description>
///         <description>
///             <para>2017-01-01-RC -> 2017-01-01-RC</para>
///             <para>2017-01-01.1.0-RC -> 2017-01-01-RC</para>
///         </description>
///     </item>
///     <item>
///         <term>"yyyy"</term>
///         <description>The year of the API group version.</description>
///         <description>
///             <para>2017-01-01 -> 2017</para>
///             <para>2017-01-01-RC -> 2017</para>
///         </description>
///     </item>
///     <item>
///         <term>"MM"</term>
///         <description>The month of the API group version.</description>
///         <description>
///             <para>2017-01-01 -> 01</para>
///             <para>2017-01-01-RC -> 01</para>
///         </description>
///     </item>
///     <item>
///         <term>"dd"</term>
///         <description>The day of the API group version.</description>
///         <description>
///             <para>2017-01-01 -> 01</para>
///             <para>2017-01-01-RC -> 01</para>
///         </description>
///     </item>
///     <item>
///         <term>"v"</term>
///         <description>The minor version of the API version.</description>
///         <description>
///             <para>1.5 -> 5</para>
///             <para>1.5-Alpha -> 5</para>
///         </description>
///     </item>
///     <item>
///         <term>"V"</term>
///         <description>The major version of the API version.</description>
///         <description>
///             <para>1.5 -> 1</para>
///             <para>1.5-Alpha -> 1</para>
///         </description>
///     </item>
///     <item>
///         <term>"VV"</term>
///         <description>The major and minor version of the API version.</description>
///         <description>
///             <para>1.5 -> 1.5</para>
///             <para>1 -> 1.0</para>
///             <para>1.5-Alpha -> 1.5</para>
///             <para>1-Alpha -> 1.0</para>
///         </description>
///     </item>
///     <item>
///         <term>"VVV"</term>
///         <description>The major version, optional minor version, and status of the API version.</description>
///         <description>
///             <para>1 -> 1</para>
///             <para>1.5 -> 1.5</para>
///             <para>1-Alpha -> 1-Alpha</para>
///             <para>1.5-Alpha -> 1.5-Alpha</para>
///         </description>
///     </item>
///     <item>
///         <term>"VVVV"</term>
///         <description>The major version, minor version, and status of the API version.</description>
///         <description>
///             <para>1 -> 1.0</para>
///             <para>1.5 -> 1.5</para>
///             <para>1-Alpha -> 1.0-Alpha</para>
///             <para>1.5-Alpha -> 1.5-Alpha</para>
///         </description>
///     </item>
///     <item>
///         <term>"p"</term>
///         <description>The minor version of the API version with padded zeros. The default padding is for two digits.</description>
///         <description>
///             <para>1.5 -> 05</para>
///             <para>1.5-Alpha -> 05</para>
///         </description>
///     </item>
///     <item>
///         <term>"p(n)"</term>
///         <description>The minor version of the API version with padded zeros where "n" is the total number of digits.</description>
///         <description>
///             <para>p3 -> 1.5 -> 005</para>
///             <para>p3 -> 1.5-Alpha -> 005</para>
///         </description>
///     </item>
///     <item>
///         <term>"P"</term>
///         <description>The major version of the API version with padded zeros. The default padding is for two digits.</description>
///         <description>
///             <para>1.5 -> 01</para>
///             <para>1.5-Alpha -> 01</para>
///         </description>
///     </item>
///     <item>
///         <term>"P(n)"</term>
///         <description>The major version of the API version with padded zeros where "n" is the total number of digits.</description>
///         <description>
///             <para>P3 -> 1.5 -> 001</para>
///             <para>P3 -> 1.5-Alpha -> 001</para>
///         </description>
///     </item>
///     <item>
///         <term>"S"</term>
///         <description>The API version status.</description>
///         <description>
///             <para>1.0-Beta -> Beta</para>
///         </description>
///     </item>
/// </list>
/// </remarks>
public partial class ApiVersionFormatProvider : IFormatProvider, ICustomFormatter
{
    private const int FormatCapacity = 32;
    internal const string GroupVersionFormat = "yyyy-MM-dd";

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiVersionFormatProvider"/> class.
    /// </summary>
    public ApiVersionFormatProvider()
    {
        DateTimeFormat = DateTimeFormatInfo.CurrentInfo;
        Calendar = DateTimeFormatInfo.CurrentInfo.Calendar;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiVersionFormatProvider"/> class.
    /// </summary>
    /// <param name="dateTimeFormat">The <see cref="DateTimeFormatInfo"/> used by the format provider.</param>
    public ApiVersionFormatProvider( DateTimeFormatInfo dateTimeFormat )
        : this( dateTimeFormat ?? throw new System.ArgumentNullException( nameof( dateTimeFormat ) ), dateTimeFormat.Calendar ) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiVersionFormatProvider"/> class.
    /// </summary>
    /// <param name="calendar">The <see cref="Calendar"/> used by the format provider.</param>
    public ApiVersionFormatProvider( Calendar calendar ) : this( DateTimeFormatInfo.CurrentInfo, calendar ) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiVersionFormatProvider"/> class.
    /// </summary>
    /// <param name="dateTimeFormat">The <see cref="DateTimeFormatInfo"/> used by the format provider.</param>
    /// <param name="calendar">The <see cref="Calendar"/> used by the format provider.</param>
    public ApiVersionFormatProvider( DateTimeFormatInfo dateTimeFormat, Calendar calendar )
    {
        DateTimeFormat = dateTimeFormat;
        Calendar = calendar;
    }

    /// <summary>
    /// Gets the underlying date and time format information.
    /// </summary>
    /// <value>A <see cref="DateTimeFormatInfo"/> object.</value>
    protected DateTimeFormatInfo DateTimeFormat { get; }

    /// <summary>
    /// Gets the calendar associated with the format provider.
    /// </summary>
    /// <value>A <see cref="Calendar"/> object.</value>
    /// <remarks>The <see cref="DateTimeFormatInfo.Calendar"/> cannot be assigned to a custom calendar.</remarks>
    protected Calendar Calendar { get; }

    /// <summary>
    /// Gets the API version format provider for the current culture.
    /// </summary>
    /// <value>The <see cref="ApiVersionFormatProvider"/> for the current culture.</value>
    public static ApiVersionFormatProvider CurrentCulture { get; } =
        new ApiVersionFormatProvider(
            DateTimeFormatInfo.CurrentInfo,
            DateTimeFormatInfo.CurrentInfo.Calendar );

    /// <summary>
    /// Gets the API version format provider for the invariant culture.
    /// </summary>
    /// <value>The <see cref="ApiVersionFormatProvider"/> for the invariant culture.</value>
    public static ApiVersionFormatProvider InvariantCulture { get; } =
        new ApiVersionFormatProvider(
            DateTimeFormatInfo.InvariantInfo,
            DateTimeFormatInfo.InvariantInfo.Calendar );

    /// <summary>
    /// Gets an instance of an API version format provider from the given format provider.
    /// </summary>
    /// <param name="formatProvider">The <see cref="IFormatProvider">format provider</see> used to retrieve the instance.</param>
    /// <returns>An <see cref="ApiVersionFormatProvider"/> object.</returns>
    public static ApiVersionFormatProvider GetInstance( IFormatProvider? formatProvider )
    {
        if ( formatProvider is ApiVersionFormatProvider provider )
        {
            return provider;
        }

        if ( formatProvider == null )
        {
            return CurrentCulture;
        }

        if ( formatProvider.GetFormat( typeof( ApiVersionFormatProvider ) ) is ApiVersionFormatProvider customProvider )
        {
            return customProvider;
        }

        if ( formatProvider is CultureInfo culture )
        {
            return new ApiVersionFormatProvider( culture.DateTimeFormat, culture.Calendar );
        }

        return CurrentCulture;
    }

    /// <summary>
    /// Formats the specified version using the provided format.
    /// </summary>
    /// <param name="text">The formatted text.</param>
    /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> to format.</param>
    /// <param name="format">The format string for the version.</param>
    /// <param name="formatProvider">The <see cref="IFormatProvider"/> used to apply the format.</param>
    protected virtual void FormatVersionPart(
        StringBuilder text,
        ApiVersion apiVersion,
        Text format,
        IFormatProvider formatProvider )
    {
        ArgumentNullException.ThrowIfNull( text );
        ArgumentNullException.ThrowIfNull( apiVersion );
#if NETSTANDARD1_0
        ArgumentNullException.ThrowIfNull( format );
#endif

        switch ( format[0] )
        {
            case 'V':
            case 'v':
                FormatVersionWithoutPadding( text, apiVersion, format, formatProvider );
                break;
            case 'P':
            case 'p':
                FormatVersionWithPadding( text, apiVersion, format, formatProvider );
                break;
        }
    }

    /// <summary>
    /// Formats the specified status part using the provided format.
    /// </summary>
    /// <param name="text">The formatted text.</param>
    /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> to format.</param>
    /// <param name="format">The format string for the status.</param>
    /// <param name="formatProvider">The <see cref="IFormatProvider"/> used to apply the format.</param>
    protected virtual void FormatStatusPart(
        StringBuilder text,
        ApiVersion apiVersion,
        Text format,
        IFormatProvider formatProvider )
    {
        ArgumentNullException.ThrowIfNull( text );
        ArgumentNullException.ThrowIfNull( apiVersion );
        text.Append( apiVersion.Status );
    }

    /// <summary>
    /// Returns the formatter for the requested type.
    /// </summary>
    /// <param name="formatType">The <see cref="Type">type</see> of requested formatter.</param>
    /// <returns>A <see cref="DateTimeFormatInfo"/>, <see cref="ICustomFormatter"/>, or <c>null</c> depending on the requested <paramref name="formatType">format type</paramref>.</returns>
    public virtual object? GetFormat( Type? formatType )
    {
        if ( typeof( ICustomFormatter ).Equals( formatType ) )
        {
            return this;
        }

        if ( formatType != null &&
             GetType().GetTypeInfo().IsAssignableFrom( formatType.GetTypeInfo() ) )
        {
            return this;
        }

        return null;
    }

    /// <summary>
    /// Formats the provided argument with the specified format and provider.
    /// </summary>
    /// <param name="format">The format string to apply to the argument.</param>
    /// <param name="arg">The argument to format.</param>
    /// <param name="formatProvider">The <see cref="IFormatProvider"/> used to format the argument.</param>
    /// <returns>A <see cref="string">string</see> representing the formatted argument.</returns>
    public virtual string Format( string? format, object? arg, IFormatProvider? formatProvider )
    {
        if ( arg is not ApiVersion value )
        {
            return GetDefaultFormat( format, arg, formatProvider );
        }

        formatProvider = formatProvider is null || ReferenceEquals( this, formatProvider ) ?
                         CultureInfo.CurrentCulture :
                         formatProvider;

        var text = new StringBuilder( FormatCapacity );

        if ( string.IsNullOrEmpty( format ) )
        {
            FormatAllParts( text, value, default, formatProvider );
            return text.ToString();
        }

        var writer = new FormatWriter( this, text, value, formatProvider );

#if NETSTANDARD1_0
        FormatTokenizer.Tokenize( format!, ref writer );
#else
        FormatTokenizer.Tokenize( format.AsSpan(), ref writer );
#endif

        return text.ToString();
    }

    private static string GetDefaultFormat( string? format, object? arg, IFormatProvider? formatProvider )
    {
        if ( arg == null )
        {
            return format ?? string.Empty;
        }

        if ( !string.IsNullOrEmpty( format ) && arg is IFormattable formattable )
        {
            return formattable.ToString( format, formatProvider );
        }

        return arg.ToString() ?? string.Empty;
    }

    internal void AppendCustomFormat( StringBuilder text, ApiVersion value, Text format, IFormatProvider formatProvider )
    {
        switch ( format[0] )
        {
            case 'F':
                FormatAllParts( text, value, format, formatProvider );
                break;
            case 'G':
            case 'M':
            case 'd':
            case 'y':
                FormatGroupVersionPart( text, value, format, formatProvider );
                break;
            case 'P':
            case 'V':
            case 'p':
            case 'v':
                FormatVersionPart( text, value, format, formatProvider );
                break;
            case 'S':
                FormatStatusPart( text, value, format, formatProvider );
                break;
        }
    }

    private static void SplitFormatSpecifierWithNumber(
        Text format,
        IFormatProvider? formatProvider,
        out Text specifier,
        out int count )
    {
        if ( format.Length == 1 )
        {
            specifier = format;
            count = 2;
            return;
        }

        var start = 0;
        var end = 0;

        for ( ; end < format.Length; end++ )
        {
#if NETSTANDARD
            var ch = format[end];
#else
            ref readonly var ch = ref format[end];
#endif
            if ( ch != 'P' && ch != 'p' )
            {
                break;
            }
        }

        specifier = Str.Slice( format, start, end );
        start = end;

        for ( ; end < format.Length; end++ )
        {
            if ( !char.IsDigit( format[end] ) )
            {
                break;
            }
        }

        count = end > start
            ? int.Parse(
                Str.StringOrSpan( Str.Slice( format, start, end ) ),
                default,
                formatProvider )
            : 2;
    }

    private static void AppendStatus( StringBuilder text, string? status )
    {
        if ( !string.IsNullOrEmpty( status ) )
        {
            text.Append( '-' ).Append( status );
        }
    }
}