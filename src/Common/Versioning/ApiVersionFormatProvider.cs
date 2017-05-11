#if WEBAPI
namespace Microsoft.Web.Http.Versioning
#else
namespace Microsoft.AspNetCore.Mvc.Versioning
#endif
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Globalization;
    using System.Reflection;
    using System.Text;
    using static System.Char;
    using static System.Globalization.DateTimeFormatInfo;
    using static System.String;

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
    ///         <description>The full, formatted API version where optional, absent components are ommitted.</description>
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
    ///         <description>The API version version status.</description>
    ///         <description>
    ///             <para>1.0-Beta -> Beta</para>
    ///         </description>
    ///     </item>
    /// </list>
    /// </remarks>
    public class ApiVersionFormatProvider : IFormatProvider, ICustomFormatter
    {
        const string GroupVersionFormat = "yyyy-MM-dd";

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiVersionFormatProvider"/> class.
        /// </summary>
        public ApiVersionFormatProvider()
        {
            DateTimeFormat = CurrentInfo;
            Calendar = CurrentInfo.Calendar;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiVersionFormatProvider"/> class.
        /// </summary>
        /// <param name="dateTimeFormat">The <see cref="DateTimeFormatInfo"/> used by the format provider.</param>
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Validated by a code contract" )]
        public ApiVersionFormatProvider( DateTimeFormatInfo dateTimeFormat ) : this( dateTimeFormat, dateTimeFormat?.Calendar ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiVersionFormatProvider"/> class.
        /// </summary>
        /// <param name="calendar">The <see cref="Calendar"/> used by the format provider.</param>
        public ApiVersionFormatProvider( Calendar calendar ) : this( CurrentInfo, calendar ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiVersionFormatProvider"/> class.
        /// </summary>
        /// <param name="dateTimeFormat">The <see cref="DateTimeFormatInfo"/> used by the format provider.</param>
        /// <param name="calendar">The <see cref="Calendar"/> used by the format provider.</param>
        public ApiVersionFormatProvider( DateTimeFormatInfo dateTimeFormat, Calendar calendar )
        {
            Arg.NotNull( dateTimeFormat, nameof( dateTimeFormat ) );
            Arg.NotNull( calendar, nameof( calendar ) );

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
        public static ApiVersionFormatProvider CurrentCulture { get; } = new ApiVersionFormatProvider( CurrentInfo, CurrentInfo.Calendar );

        /// <summary>
        /// Gets the API version format provider for the invariant culture.
        /// </summary>
        /// <value>The <see cref="ApiVersionFormatProvider"/> for the invariant culture.</value>
        public static ApiVersionFormatProvider InvariantCulture { get; } = new ApiVersionFormatProvider( InvariantInfo, InvariantInfo.Calendar );

        /// <summary>
        /// Gets an instance of an API version format provider from the given format provider.
        /// </summary>
        /// <param name="formatProvider">The <see cref="IFormatProvider">format provider</see> used to retrieve the instance.</param>
        /// <returns>An <see cref="ApiVersionFormatProvider"/> object.</returns>
        public static ApiVersionFormatProvider GetInstance( IFormatProvider formatProvider )
        {
            Contract.Ensures( Contract.Result<ApiVersionFormatProvider>() != null );

            if ( formatProvider is ApiVersionFormatProvider provider )
            {
                return provider;
            }

            if ( formatProvider == null )
            {
                return CurrentCulture;
            }

            if ( ( provider = formatProvider.GetFormat( typeof( ApiVersionFormatProvider ) ) as ApiVersionFormatProvider ) == null )
            {
                if ( formatProvider is CultureInfo culture )
                {
                    return new ApiVersionFormatProvider( culture.DateTimeFormat, culture.Calendar );
                }

                return CurrentCulture;
            }

            return provider;
        }

        /// <summary>
        /// Formats all parts using the default format.
        /// </summary>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> to format.</param>
        /// <param name="format">The format string for the API version. This parameter can be <c>null</c> or empty.</param>
        /// <param name="formatProvider">The <see cref="IFormatProvider"/> used to apply the format.</param>
        /// <returns>A formatted <see cref="string">string</see> representing the API version.</returns>
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1", Justification = "Validated by a code contract" )]
        protected virtual string FormatAllParts( ApiVersion apiVersion, string format, IFormatProvider formatProvider )
        {
            Arg.NotNull( apiVersion, nameof( apiVersion ) );
            Arg.NotNull( formatProvider, nameof( formatProvider ) );
            Contract.Ensures( !IsNullOrEmpty( Contract.Result<string>() ) );

            var text = new StringBuilder();

            if ( apiVersion.GroupVersion != null )
            {
                text.Append( apiVersion.GroupVersion.Value.ToString( GroupVersionFormat, formatProvider ) );
            }

            if ( apiVersion.MajorVersion != null )
            {
                if ( text.Length > 0 )
                {
                    text.Append( '.' );
                }

                text.Append( apiVersion.MajorVersion.Value.ToString( formatProvider ) );

                if ( apiVersion.MinorVersion == null )
                {
                    if ( format == "FF" )
                    {
                        text.Append( ".0" );
                    }
                }
                else
                {
                    text.Append( '.' );
                    text.Append( apiVersion.MinorVersion.Value.ToString( formatProvider ) );
                }
            }
            else if ( apiVersion.MinorVersion != null )
            {
                text.Append( "0." );
                text.Append( apiVersion.MinorVersion.Value.ToString( formatProvider ) );
            }

            if ( text.Length > 0 && !IsNullOrEmpty( apiVersion.Status ) )
            {
                text.Append( '-' );
                text.Append( apiVersion.Status );
            }

            return text.ToString();
        }

        /// <summary>
        /// Formats the specified group version using the provided format.
        /// </summary>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> to format.</param>
        /// <param name="format">The format string for the group version.</param>
        /// <param name="formatProvider">The <see cref="IFormatProvider"/> used to apply the format.</param>
        /// <returns>A formatted <see cref="string">string</see> representing the group version.</returns>
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1", Justification = "Validated by a code contract" )]
        protected virtual string FormatGroupVersionPart( ApiVersion apiVersion, string format, IFormatProvider formatProvider )
        {
            Arg.NotNull( apiVersion, nameof( apiVersion ) );
            Arg.NotNullOrEmpty( format, nameof( format ) );
            Arg.NotNull( formatProvider, nameof( formatProvider ) );
            Contract.Ensures( !IsNullOrEmpty( Contract.Result<string>() ) );

            if ( apiVersion.GroupVersion == null )
            {
                return Empty;
            }

            var groupVersion = apiVersion.GroupVersion.Value;

            switch ( format[0] )
            {
                case 'G':
                    // G, GG
                    var text = new StringBuilder( groupVersion.ToString( GroupVersionFormat, formatProvider ) );

                    // GG
                    if ( format.Length == 2 )
                    {
                        AppendStatus( text, apiVersion.Status );
                    }

                    return text.ToString();
                case 'M':
                    var month = Calendar.GetMonth( groupVersion );

                    switch ( format.Length )
                    {
                        case 1: // M
                            return month.ToString( formatProvider );
                        case 2: // MM
                            return month.ToString( "00", formatProvider );
                        case 3: // MMM
                            return DateTimeFormat.GetAbbreviatedMonthName( month );
                    }

                    // MMMM*
                    return DateTimeFormat.GetMonthName( month );
                case 'd':
                    switch ( format.Length )
                    {
                        case 1: // d
                            return Calendar.GetDayOfMonth( groupVersion ).ToString( formatProvider );
                        case 2: // dd
                            return Calendar.GetDayOfMonth( groupVersion ).ToString( "00", formatProvider );
                        case 3: // ddd
                            return DateTimeFormat.GetAbbreviatedDayName( Calendar.GetDayOfWeek( groupVersion ) );
                    }

                    // dddd*
                    return DateTimeFormat.GetDayName( Calendar.GetDayOfWeek( groupVersion ) );
                case 'y':
                    var year = Calendar.GetYear( groupVersion );

                    switch ( format.Length )
                    {
                        case 1: // y
                            return ( year % 100 ).ToString( formatProvider );
                        case 2: // yy
                            return ( year % 100 ).ToString( "00", formatProvider );
                        case 3: // yyy
                            return year.ToString( "000", formatProvider );
                    }

                    // yyyy*
                    return year.ToString( formatProvider );
            }

            return groupVersion.ToString( format, formatProvider );
        }

        /// <summary>
        /// Formats the specified version using the provided format.
        /// </summary>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> to format.</param>
        /// <param name="format">The format string for the version.</param>
        /// <param name="formatProvider">The <see cref="IFormatProvider"/> used to apply the format.</param>
        /// <returns>A formatted <see cref="string">string</see> representing the version.</returns>
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1", Justification = "Validated by a code contract" )]
        protected virtual string FormatVersionPart( ApiVersion apiVersion, string format, IFormatProvider formatProvider )
        {
            Arg.NotNull( apiVersion, nameof( apiVersion ) );
            Arg.NotNullOrEmpty( format, nameof( format ) );
            Arg.NotNull( formatProvider, nameof( formatProvider ) );
            Contract.Ensures( !IsNullOrEmpty( Contract.Result<string>() ) );

            switch ( format[0] )
            {
                case 'V':
                case 'v':
                    return FormatVersionWithoutPadding( apiVersion, format, formatProvider );
                case 'P':
                case 'p':
                    return FormatVersionWithPadding( apiVersion, format, formatProvider );
            }

            return Empty;
        }

        /// <summary>
        /// Formats the specified status part using the provided format.
        /// </summary>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> to format.</param>
        /// <param name="format">The format string for the status.</param>
        /// <param name="formatProvider">The <see cref="IFormatProvider"/> used to apply the format.</param>
        /// <returns>A formatted <see cref="string">string</see> representing the status.</returns>
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1", Justification = "Validated by a code contract" )]
        protected virtual string FormatStatusPart( ApiVersion apiVersion, string format, IFormatProvider formatProvider )
        {
            Arg.NotNull( apiVersion, nameof( apiVersion ) );
            Arg.NotNullOrEmpty( format, nameof( format ) );
            Arg.NotNull( formatProvider, nameof( formatProvider ) );
            Contract.Ensures( !IsNullOrEmpty( Contract.Result<string>() ) );

            return apiVersion.Status ?? Empty;
        }

        /// <summary>
        /// Returns the formatter for the requested type.
        /// </summary>
        /// <param name="formatType">The <see cref="Type">type</see> of requested formatter.</param>
        /// <returns>A <see cref="DateTimeFormatInfo"/>, <see cref="ICustomFormatter"/>, or <c>null</c> depending on the requested <paramref name="formatType">format type</paramref>.</returns>
        public virtual object GetFormat( Type formatType )
        {
            if ( typeof( ICustomFormatter ).Equals( formatType ) )
            {
                return this;
            }

            if ( GetType().GetTypeInfo().IsAssignableFrom( formatType.GetTypeInfo() ) )
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
        /// <returns>A <see cref="string">string</see> represeting the formatted argument.</returns>
        public virtual string Format( string format, object arg, IFormatProvider formatProvider )
        {
            if ( !( arg is ApiVersion value ) )
            {
                return GetDefaultFormat( format, arg, formatProvider );
            }

            formatProvider = formatProvider == null || ReferenceEquals( this, formatProvider ) ? CultureInfo.CurrentCulture : formatProvider;

            if ( IsNullOrEmpty( format ) )
            {
                return FormatAllParts( value, null, formatProvider );
            }

            var tokens = FormatTokenizer.Tokenize( format );
            var text = new StringBuilder();

            foreach ( var token in tokens )
            {
                if ( token.IsInvalid )
                {
                    throw new FormatException( SR.InvalidFormatString );
                }

                text.Append( token.IsLiteral ? token.Format : GetCustomFormat( value, token.Format, formatProvider ) );
            }

            return text.ToString();
        }

        static string GetDefaultFormat( string format, object arg, IFormatProvider formatProvider )
        {
            if ( arg == null )
            {
                return format ?? Empty;
            }

            if ( !IsNullOrEmpty( format ) )
            {
                if ( arg is IFormattable formattable )
                {
                    return formattable.ToString( format, formatProvider );
                }
            }

            return arg.ToString();
        }

        string GetCustomFormat( ApiVersion value, string format, IFormatProvider formatProvider )
        {
            Contract.Requires( !IsNullOrEmpty( format ) );
            Contract.Requires( formatProvider != null );
            Contract.Ensures( Contract.Result<string>() != null );

            switch ( format[0] )
            {
                case 'F':
                    return FormatAllParts( value, format, formatProvider );
                case 'G':
                case 'M':
                case 'd':
                case 'y':
                    return FormatGroupVersionPart( value, format, formatProvider );
                case 'P':
                case 'V':
                case 'p':
                case 'v':
                    return FormatVersionPart( value, format, formatProvider );
                case 'S':
                    return FormatStatusPart( value, format, formatProvider );
            }

            return Empty;
        }

        static string FormatVersionWithoutPadding( ApiVersion apiVersion, string format, IFormatProvider formatProvider )
        {
            Contract.Requires( apiVersion != null );
            Contract.Requires( !IsNullOrEmpty( format ) );
            Contract.Requires( formatProvider != null );
            Contract.Ensures( !IsNullOrEmpty( Contract.Result<string>() ) );

            if ( format.Length == 1 && format[0] == 'v' )
            {
                return apiVersion.MinorVersion == null ? Empty : apiVersion.MinorVersion.Value.ToString( formatProvider );
            }

            if ( apiVersion.MajorVersion == null || format[0] != 'V' )
            {
                return Empty;
            }

            // V*
            var text = new StringBuilder( apiVersion.MajorVersion.Value.ToString( formatProvider ) );

            if ( format.Length == 1 )
            {
                return text.ToString();
            }

            var minor = apiVersion.MinorVersion ?? 0;

            switch ( format.Length )
            {
                case 2: // VV
                    text.Append( '.' );
                    text.Append( minor.ToString( formatProvider ) );
                    break;
                case 3: // VVV
                    if ( minor > 0 )
                    {
                        text.Append( '.' );
                        text.Append( minor.ToString( formatProvider ) );
                    }
                    AppendStatus( text, apiVersion.Status );
                    break;
                case 4: // VVVV
                    text.Append( '.' );
                    text.Append( minor.ToString( formatProvider ) );
                    AppendStatus( text, apiVersion.Status );
                    break;
            }

            return text.ToString();
        }

        static string FormatVersionWithPadding( ApiVersion apiVersion, string format, IFormatProvider formatProvider )
        {
            Contract.Requires( apiVersion != null );
            Contract.Requires( !IsNullOrEmpty( format ) );
            Contract.Requires( formatProvider != null );
            Contract.Ensures( !IsNullOrEmpty( Contract.Result<string>() ) );

            SplitFormatSpecifierWithNumber( format, formatProvider, out var specifier, out var count );

            const string TwoDigits = "D2";
            const string LeadingZeros = "'D'0";

            // p, p(n)
            if ( specifier == "p" )
            {
                format = count.ToString( LeadingZeros, InvariantCulture );
                return apiVersion.MinorVersion == null ? Empty : apiVersion.MinorVersion.Value.ToString( format, formatProvider );
            }

            if ( apiVersion.MajorVersion == null || format[0] != 'P' )
            {
                return Empty;
            }

            // P, P(n)
            if ( specifier == "P" )
            {
                format = count.ToString( LeadingZeros, InvariantCulture );
                return apiVersion.MajorVersion.Value.ToString( format, formatProvider );
            }

            var text = new StringBuilder( apiVersion.MajorVersion.Value.ToString( TwoDigits, formatProvider ) );
            var minor = apiVersion.MinorVersion ?? 0;

            switch ( format.Length )
            {
                case 2: // PP
                    text.Append( '.' );
                    text.Append( minor.ToString( TwoDigits, formatProvider ) );
                    break;
                case 3: // PPP
                    if ( minor > 0 )
                    {
                        text.Append( '.' );
                        text.Append( minor.ToString( TwoDigits, formatProvider ) );
                    }
                    AppendStatus( text, apiVersion.Status );
                    break;
                case 4: // PPPP
                    text.Append( '.' );
                    text.Append( minor.ToString( TwoDigits, formatProvider ) );
                    AppendStatus( text, apiVersion.Status );
                    break;
            }

            return text.ToString();
        }

        static void SplitFormatSpecifierWithNumber( string format, IFormatProvider formatProvider, out string specifier, out int count )
        {
            Contract.Requires( !IsNullOrEmpty( format ) );
            Contract.Requires( formatProvider != null );
            Contract.Ensures( !IsNullOrEmpty( Contract.ValueAtReturn( out specifier ) ) );
            Contract.Ensures( Contract.ValueAtReturn( out count ) >= 0 );

            count = 2;

            if ( format.Length == 1 )
            {
                specifier = format;
                return;
            }

            var start = 0;
            var end = 0;

            for ( ; end < format.Length; end++ )
            {
                var ch = format[end];

                if ( ch != 'P' && ch != 'p' )
                {
                    break;
                }
            }

            specifier = format.Substring( start, end );
            start = end;

            for ( ; end < format.Length; end++ )
            {
                if ( !char.IsNumber( format[end] ) )
                {
                    break;
                }
            }

            if ( end > start )
            {
                count = int.Parse( format.Substring( start, end - start ), formatProvider );
            }
        }

        static void AppendStatus( StringBuilder text, string status )
        {
            Contract.Requires( text != null );

            if ( !IsNullOrEmpty( status ) )
            {
                text.Append( '-' );
                text.Append( status );
            }
        }

        [DebuggerDisplay( "Token = {Token,nq}, Invalid = {IsInvalid,nq}, Literal = {IsLiteral,nq}" )]
        sealed class FormatToken
        {
            internal readonly string Format;
            internal readonly bool IsLiteral;
            internal readonly bool IsInvalid;

            internal FormatToken( string format ) : this( format, false, false ) { }

            internal FormatToken( string format, bool literal ) : this( format, literal, false ) { }

            internal FormatToken( string format, bool literal, bool invalid )
            {
                Contract.Requires( format != null );
                Format = format;
                IsLiteral = literal;
                IsInvalid = invalid;
            }
        }

        static class FormatTokenizer
        {
            static bool IsLiteralDelimiter( char ch ) => ch == '\'' || ch == '\"';

            static bool IsFormatSpecifier( char ch )
            {
                switch ( ch )
                {
                    case 'F':
                    case 'G':
                    case 'M':
                    case 'P':
                    case 'S':
                    case 'V':
                    case 'd':
                    case 'p':
                    case 'v':
                    case 'y':
                        return true;
                }

                return false;
            }

            static bool IsEscapeSequence( string sequence )
            {
                Contract.Requires( sequence != null );
                Contract.Requires( sequence.Length == 2 );

                switch ( sequence )
                {
                    case @"\'":
                    case @"\\":
                    case @"\F":
                    case @"\G":
                    case @"\M":
                    case @"\P":
                    case @"\S":
                    case @"\V":
                    case @"\d":
                    case @"\p":
                    case @"\v":
                    case @"\y":
                        return true;
                }

                return false;
            }

            static bool IsSingleCustomFormatSpecifier( string sequence )
            {
                Contract.Requires( sequence != null );
                Contract.Requires( sequence.Length == 2 );

                switch ( sequence )
                {
                    case "%F":
                    case "%G":
                    case "%M":
                    case "%P":
                    case "%S":
                    case "%V":
                    case "%d":
                    case "%v":
                    case "%p":
                    case "%y":
                        return true;
                }

                return false;
            }

            static void EnsureCurrentLiteralSequenceTerminated( ICollection<FormatToken> tokens, StringBuilder token )
            {
                Contract.Requires( tokens != null );
                Contract.Requires( token != null );

                if ( token.Length > 0 )
                {
                    tokens.Add( new FormatToken( token.ToString(), true ) );
                    token.Length = 0;
                }
            }

            static void ConsumeLiteral( ICollection<FormatToken> tokens, StringBuilder token, string format, char ch, ref int i )
            {
                Contract.Requires( tokens != null );
                Contract.Requires( token != null );
                Contract.Requires( !IsNullOrEmpty( format ) );
                Contract.Requires( i >= 0 );

                EnsureCurrentLiteralSequenceTerminated( tokens, token );

                var delimiter = ch;
                var current = '\0';

                while ( ( ++i < format.Length ) && ( ( current = format[i] ) != delimiter ) )
                {
                    token.Append( current );
                }

                tokens.Add( new FormatToken( token.ToString(), literal: true, invalid: current != delimiter ) );
                token.Length = 0;
            }

            static void ConsumeEscapeSequence( ICollection<FormatToken> tokens, StringBuilder token, string format, ref int i )
            {
                Contract.Requires( tokens != null );
                Contract.Requires( token != null );
                Contract.Requires( !IsNullOrEmpty( format ) );
                Contract.Requires( i >= 0 );

                EnsureCurrentLiteralSequenceTerminated( tokens, token );
                tokens.Add( new FormatToken( format.Substring( ++i, 1 ), literal: true ) );
                token.Length = 0;
            }

            static void ConsumeSingleCustomFormat( ICollection<FormatToken> tokens, StringBuilder token, string format, ref int i )
            {
                Contract.Requires( tokens != null );
                Contract.Requires( token != null );
                Contract.Requires( !IsNullOrEmpty( format ) );
                Contract.Requires( i >= 0 );

                EnsureCurrentLiteralSequenceTerminated( tokens, token );

                var start = ++i;
                var end = start + 1;

                for ( ; end < format.Length; end++ )
                {
                    if ( !char.IsNumber( format[end] ) )
                    {
                        break;
                    }
                }

                tokens.Add( new FormatToken( format.Substring( start, end - start ) ) );
                token.Length = 0;
            }

            static void ConsumeCustomFormat( ICollection<FormatToken> tokens, StringBuilder token, string format, char ch, ref int i )
            {
                Contract.Requires( tokens != null );
                Contract.Requires( token != null );
                Contract.Requires( !IsNullOrEmpty( format ) );
                Contract.Requires( i >= 0 );

                EnsureCurrentLiteralSequenceTerminated( tokens, token );
                token.Append( ch );

                var last = ch;

                while ( ( ++i < format.Length ) && ( ( ch = format[i] ) == last ) )
                {
                    token.Append( ch );
                }

                for ( ; i < format.Length; i++ )
                {
                    if ( IsNumber( ch = format[i] ) )
                    {
                        token.Append( ch );
                    }
                    else
                    {
                        break;
                    }
                }

                tokens.Add( new FormatToken( token.ToString() ) );
                token.Length = 0;

                if ( i != format.Length )
                {
                    --i;
                }
            }

            internal static IEnumerable<FormatToken> Tokenize( string format )
            {
                Contract.Requires( !IsNullOrEmpty( format ) );
                Contract.Ensures( Contract.Result<IEnumerable<FormatToken>>() != null );

                var tokens = new List<FormatToken>();
                var token = new StringBuilder();

                for ( var i = 0; i < format.Length; i++ )
                {
                    var ch = format[i];

                    if ( IsLiteralDelimiter( ch ) )
                    {
                        ConsumeLiteral( tokens, token, format, ch, ref i );
                    }
                    else if ( ( ch == '\\' ) && ( i < format.Length - 1 ) && IsEscapeSequence( format.Substring( i, 2 ) ) )
                    {
                        ConsumeEscapeSequence( tokens, token, format, ref i );
                    }
                    else if ( ( ch == '%' ) && ( i < format.Length - 1 ) && IsSingleCustomFormatSpecifier( format.Substring( i, 2 ) ) )
                    {
                        ConsumeSingleCustomFormat( tokens, token, format, ref i );
                    }
                    else if ( IsFormatSpecifier( ch ) )
                    {
                        ConsumeCustomFormat( tokens, token, format, ch, ref i );
                    }
                    else
                    {
                        token.Append( ch );
                    }
                }

                return tokens;
            }
        }
    }
}