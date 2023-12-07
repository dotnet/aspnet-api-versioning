// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0079
#pragma warning disable SA1121

namespace Asp.Versioning;

#if !NETSTANDARD1_0
using Microsoft.Extensions.Primitives;
#endif
using System.Collections;
using System.Text;
#if NETSTANDARD1_0
using StringSegment = System.String;
using StringSegmentComparer = System.StringComparer;
#endif

#pragma warning disable IDE0079

/// <summary>
/// Represents a HTTP Link header value.
/// </summary>
/// <remarks>For more information see <a href="https://datatracker.ietf.org/doc/html/rfc8288">RFC 8288</a>.</remarks>
public partial class LinkHeaderValue
{
    private List<StringSegment>? languages;
    private ExtensionDictionary? extensions;

    /// <summary>
    /// Initializes a new instance of the <see cref="LinkHeaderValue"/> class.
    /// </summary>
    /// <param name="linkTarget">The link target URL.</param>
    /// <param name="relationType">The link relation type that identifies the semantics of the link.</param>
    public LinkHeaderValue( Uri linkTarget, StringSegment relationType )
    {
        LinkTarget = linkTarget;
        RelationType = relationType;
#if NETSTANDARD1_0
        Media = string.Empty;
        Title = string.Empty;
        Type = string.Empty;
#endif
    }

    /// <summary>
    /// Gets the link target Uniform Resource Locator (URL).
    /// </summary>
    /// <value>The link target URL.</value>
    public Uri LinkTarget { get; }

    /// <summary>
    /// Gets the link relation type.
    /// </summary>
    /// <value>The link relation type that identifies the semantics of the link.</value>
    public StringSegment RelationType { get; }

    /// <summary>
    /// Gets or sets the language associated with the link.
    /// </summary>
    /// <value>A hint indicating what the language of the result of dereferencing the link should be.</value>
    /// <remarks>
    /// <para>
    /// This is only a hint; for example, it does not override the Content-Language header field of
    /// a HTTP response obtained by actually following the link. A single link may indicate that multiple
    /// languages are available from the indicated resource.
    /// </para>
    /// <para>
    /// If the link has more than one associated language, this property always gets or sets the first
    /// language defined.
    /// </para>
    /// </remarks>
    public StringSegment Language
    {
        get => languages is null || languages.Count == 0 ?
#if NETSTANDARD1_0
               string.Empty :
#else
               default :
#endif
               languages[0];
        set
        {
#if NETFRAMEWORK1_0
            value ??= string.Empty;
#endif
            if ( languages is null )
            {
                languages = [value];
            }
            else if ( languages.Count == 0 )
            {
                languages.Add( value );
            }
            else
            {
                languages[0] = value;
            }
        }
    }

    /// <summary>
    /// Gets languages associated with the link.
    /// </summary>
    /// <value>A hint indicating what the language of the result of dereferencing the link should be.</value>
    /// <remarks>This is only a hint; for example, it does not override the Content-Language header field of
    /// a HTTP response obtained by actually following the link. A single link may indicate that multiple
    /// languages are available from the indicated resource.</remarks>
    public IList<StringSegment> Languages => languages ??= [];

    /// <summary>
    /// Gets or sets the link media.
    /// </summary>
    /// <value>Used to indicate intended destination medium or media for style information.</value>
    /// <remarks>For more information see, <a href="https://html.spec.whatwg.org/multipage/semantics.html#attr-link-media">The link element</a>.</remarks>
    public StringSegment Media { get; set; }

    /// <summary>
    /// Gets or sets the link title.
    /// </summary>
    /// <value>Used to label the destination of the link such that it can be used as a human-readable identifier
    /// (e.g. "menu entry") in the language indicated by the Content-Language header field, if present.</value>
    public StringSegment Title { get; set; }

    /// <summary>
    /// Gets or sets the link type.
    /// </summary>
    /// <value>A hint indicating what the media type of the result of dereferencing the link should be.</value>
    public StringSegment Type { get; set; }

    /// <summary>
    /// Gets the target attribute extensions, if any.
    /// </summary>
    /// <value>A collection of key/value pairs representing the target attribute extensions.</value>
    public IDictionary<StringSegment, StringSegment> Extensions => extensions ??= new();

    /// <summary>
    /// Parses the specified header value.
    /// </summary>
    /// <param name="input">The input value to parse.</param>
    /// <param name="resolveRelativeUrl">The optional function used to resolve relative URLs.</param>
    /// <returns>A new <see cref="LinkHeaderValue">LINK header value</see>.</returns>
    /// <remarks>For more information see <a href="https://datatracker.ietf.org/doc/html/rfc8288#appendix-B">RFC 8288 - Appendix B</a>.</remarks>
    public static LinkHeaderValue Parse( StringSegment input, Func<Uri, Uri>? resolveRelativeUrl = default )
    {
        if ( TryParse( input, resolveRelativeUrl, out var parsedValue ) )
        {
            return parsedValue;
        }

        throw new FormatException( SR.InvalidOrMalformedHeader );
    }

    /// <summary>
    /// Attempts to parse the specified header value.
    /// </summary>
    /// <param name="input">The input value to parse.</param>
    /// <param name="resolveRelativeUrl">The optional function used to resolve relative URLs.</param>
    /// <param name="parsedValue">A new <see cref="LinkHeaderValue">LINK header value</see> or <c>null</c>.</param>
    /// <returns>True if the header is successfully parsed; otherwise, false.</returns>
    /// <remarks>For more information see <a href="https://datatracker.ietf.org/doc/html/rfc8288#appendix-B">RFC 8288 - Appendix B</a>.</remarks>
    public static bool TryParse(
        StringSegment input,
        Func<Uri, Uri>? resolveRelativeUrl,
        [MaybeNullWhen( false )] out LinkHeaderValue parsedValue )
    {
#if NETSTANDARD1_0
        if ( string.IsNullOrEmpty( input ) )
        {
            parsedValue = default!;
            return false;
        }
#endif

        if ( !TryParseTargetLink( ref input, resolveRelativeUrl, out var targetlink ) )
        {
            parsedValue = default!;
            return false;
        }

        var rel = default( StringSegment );
        var title = default( StringSegment );
        var media = default( StringSegment );
        var type = default( StringSegment );
        var languages = default( List<StringSegment> );
        var extensions = default( List<KeyValuePair<StringSegment, StringSegment>> );

        foreach ( var attribute in new TargetAttributesEnumerator( input ) )
        {
#if NETSTANDARD1_0
            var key = attribute.Key;
#else
            var key = attribute.Key.Value;
#endif
            switch ( key )
            {
                case "rel":
                    rel = attribute.Value;
                    break;
                case "title":
                    title = attribute.Value;
                    break;
                case "media":
                    media = attribute.Value;
                    break;
                case "type":
                    type = attribute.Value;
                    break;
                case "hreflang":
                    languages ??= [];
                    languages.Add( attribute.Value );
                    break;
                default:
                    extensions ??= [];
                    extensions.Add( attribute );
                    break;
            }
        }

#if NETSTANDARD1_0
        if ( string.IsNullOrEmpty( rel ) )
#else
        if ( !rel.HasValue )
#endif
        {
            parsedValue = default!;
            return false;
        }

        parsedValue = new( targetlink, rel! )
        {
#if NETSTANDARD1_0
            Media = media ?? string.Empty,
            Title = title ?? string.Empty,
            Type = type ?? string.Empty,
#else
            Media = media,
            Title = title,
            Type = type,
#endif
        };

        if ( languages != null )
        {
            if ( languages.Count == 1 )
            {
                parsedValue.Language = languages[0];
            }
            else
            {
                for ( var i = 0; i < languages.Count; i++ )
                {
                    parsedValue.Languages.Add( languages[i] );
                }
            }
        }

        if ( extensions != null )
        {
            for ( var i = 0; i < extensions.Count; i++ )
            {
                var ext = extensions[i];
                parsedValue.Extensions[ext.Key] = ext.Value;
            }
        }

        return true;
    }

    /// <summary>
    /// Parses a sequence of inputs as a sequence of <see cref="LinkHeaderValue"/> values.
    /// </summary>
    /// <param name="input">The values to parse.</param>
    /// <param name="resolveRelativeUrl">The optional function used to resolve relative URLs.</param>
    /// <returns>The parsed values.</returns>
    public static IList<LinkHeaderValue> ParseList( IList<string>? input, Func<Uri, Uri>? resolveRelativeUrl = default )
    {
        if ( input == null )
        {
            return new List<LinkHeaderValue>();
        }

        var list = new List<LinkHeaderValue>( capacity: input.Count );

        for ( var i = 0; i < input.Count; i++ )
        {
            list.Add( Parse( input[i], resolveRelativeUrl ) );
        }

        return list;
    }

    /// <summary>
    /// Attempts to parse the sequence of values as a sequence of <see cref="LinkHeaderValue"/>.
    /// </summary>
    /// <param name="input">The values to parse.</param>
    /// <param name="resolveRelativeUrl">The optional function used to resolve relative URLs.</param>
    /// <param name="parsedValues">The parsed values.</param>
    /// <returns><see langword="true"/> if all inputs are valid <see cref="LinkHeaderValue"/>, otherwise <see langword="false"/>.</returns>
    public static bool TryParseList(
        IList<string>? input,
        Func<Uri, Uri>? resolveRelativeUrl,
        [MaybeNullWhen( false )]
        out IList<LinkHeaderValue> parsedValues )
    {
        if ( input == null )
        {
            parsedValues = default!;
            return false;
        }

        parsedValues = new List<LinkHeaderValue>( capacity: input.Count );

        for ( var i = 0; i < input.Count; i++ )
        {
            if ( TryParse( input[i], resolveRelativeUrl, out var value ) )
            {
                parsedValues.Add( value );
            }
        }

        return true;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        var builder = new StringBuilder();

        builder.Append( '<' );

        if ( !LinkTarget.IsAbsoluteUri )
        {
            builder.Append( '/' );
        }

        builder.Append( LinkTarget.OriginalString ).Append( '>' );
        AppendTargetAttribute( builder, "rel", RelationType );

        if ( languages != null )
        {
            for ( var i = 0; i < languages.Count; i++ )
            {
                AppendTargetAttribute( builder, "hreflang", languages[i] );
            }
        }

#if NETSTANDARD1_0
        if ( !string.IsNullOrEmpty( Media ) )
#else
        if ( Media.HasValue )
#endif
        {
            AppendTargetAttribute( builder, "media", Media );
        }

#if NETSTANDARD1_0
        if ( !string.IsNullOrEmpty( Title ) )
#else
        if ( Title.HasValue )
#endif
        {
            AppendTargetAttribute( builder, "title", Title );
        }

#if NETSTANDARD1_0
        if ( !string.IsNullOrEmpty( Type ) )
#else
        if ( Type.HasValue )
#endif
        {
            AppendTargetAttribute( builder, "type", Type );
        }

        if ( extensions != null )
        {
            foreach ( var extension in extensions )
            {
                AppendTargetAttribute( builder, extension.Key, extension.Value );
            }
        }

        return builder.ToString();
    }

#if NETSTANDARD1_0
    private static void AppendTargetAttribute( StringBuilder builder, string name, string value ) =>
        builder.Append( "; " ).Append( name ).Append( "=\"" ).Append( value ).Append( '"' );
#elif NETSTANDARD2_0
    private static void AppendTargetAttribute( StringBuilder builder, string name, ReadOnlySpan<char> value ) =>
        builder.Append( "; " ).Append( name ).Append( "=\"" ).Append( value.ToString() ).Append( '"' );

    private static void AppendTargetAttribute( StringBuilder builder, StringSegment name, ReadOnlySpan<char> value ) =>
        builder.Append( "; " ).Append( name.ToString() ).Append( "=\"" ).Append( value.ToString() ).Append( '"' );
#else
    private static void AppendTargetAttribute( StringBuilder builder, ReadOnlySpan<char> name, ReadOnlySpan<char> value ) =>
        builder.Append( "; " ).Append( name ).Append( "=\"" ).Append( value ).Append( '"' );
#endif

    private static bool TryParseTargetLink(
        ref StringSegment segment,
        Func<Uri, Uri>? resolveRelativeUrl,
        [MaybeNullWhen( false )] out Uri targetLink )
    {
        var start = segment.IndexOf( '<' );

        if ( start < 0 )
        {
            targetLink = default!;
            return false;
        }

        var end = segment.IndexOf( '>', ++start );

        if ( end < 0 )
        {
            targetLink = default!;
            return false;
        }

        var url = segment.Substring( start, end - start );

        if ( !Uri.TryCreate( url, UriKind.RelativeOrAbsolute, out targetLink ) )
        {
            return false;
        }

        if ( !targetLink.IsAbsoluteUri && resolveRelativeUrl != null )
        {
            targetLink = resolveRelativeUrl( targetLink );
        }

        start = segment.IndexOf( ';', end + 1 ) + 1;

        // 'rel' is required
#if NETSTANDARD1_0
        segment = segment.Substring( start );
        return !string.IsNullOrEmpty( segment );
#else
        segment = segment.Subsegment( start );
        return segment.HasValue;
#endif
    }

    private sealed class ExtensionDictionary : IDictionary<StringSegment, StringSegment>
    {
        private readonly Dictionary<StringSegment, StringSegment> items = new( StringSegmentComparer.OrdinalIgnoreCase );

        public ICollection<StringSegment> Keys => items.Keys;

        public ICollection<StringSegment> Values => items.Values;

        public int Count => items.Count;

        public bool IsReadOnly => ( (ICollection<KeyValuePair<StringSegment, StringSegment>>) items ).IsReadOnly;

        public StringSegment this[StringSegment key]
        {
            get => items[key];
            set => items[ValidateKey( ref key )] = value;
        }

        public void Add( StringSegment key, StringSegment value ) => items.Add( ValidateKey( ref key ), value );

        public void Add( KeyValuePair<StringSegment, StringSegment> item )
        {
            var key = item.Key;
            ValidateKey( ref key );
            ( (ICollection<KeyValuePair<StringSegment, StringSegment>>) items ).Add( item );
        }

        public void Clear() => items.Clear();

        public bool Contains( KeyValuePair<StringSegment, StringSegment> item ) =>
            ( (ICollection<KeyValuePair<StringSegment, StringSegment>>) items ).Contains( item );

        public bool ContainsKey( StringSegment key ) => items.ContainsKey( key );

        public void CopyTo( KeyValuePair<StringSegment, StringSegment>[] array, int arrayIndex ) =>
            ( (ICollection<KeyValuePair<StringSegment, StringSegment>>) items ).CopyTo( array, arrayIndex );

        public IEnumerator<KeyValuePair<StringSegment, StringSegment>> GetEnumerator() => items.GetEnumerator();

        public bool Remove( StringSegment key ) => items.Remove( key );

        public bool Remove( KeyValuePair<StringSegment, StringSegment> item ) =>
            ( (ICollection<KeyValuePair<StringSegment, StringSegment>>) items ).Remove( item );

        public bool TryGetValue(
            StringSegment key,
#if !NETSTANDARD1_0
            [MaybeNullWhen( false )]
#endif
            out StringSegment value ) => items.TryGetValue( key, out value );

        IEnumerator IEnumerable.GetEnumerator() => items.GetEnumerator();

        private static ref StringSegment ValidateKey( ref StringSegment key )
        {
            if ( key.Length == 0 )
            {
                throw new ArgumentException( SR.EmptyKey, nameof( key ) );
            }

#if NETSTANDARD1_0
            var text = key;
            var ch = text[0];
#else
            var text = key.AsSpan();
            ref readonly var ch = ref text[0];
#endif
            if ( !char.IsLetter( ch ) )
            {
                throw new ArgumentException( SR.FirstCharMustBeLetter, nameof( key ) );
            }

            for ( var i = 1; i < text.Length; i++ )
            {
#if NETSTANDARD1_0
                ch = text[i];
#else
                ch = ref text[i];
#endif
                var valid = char.IsLetterOrDigit( ch ) || ch == '-' || ch == '_';

                if ( !valid )
                {
                    throw new ArgumentException( SR.InvalidLinkKey, nameof( key ) );
                }
            }

            return ref key;
        }
    }

    private struct TargetAttributesEnumerator( StringSegment remaining )
        : IEnumerable<KeyValuePair<StringSegment, StringSegment>>
    {
        private int start = 0;

        public IEnumerator<KeyValuePair<StringSegment, StringSegment>> GetEnumerator()
        {
            ConsumeWhitespace();

            while ( start < remaining.Length )
            {
                var end = start;
                var valid = true;

                while ( end < remaining.Length && valid )
                {
                    var ch = remaining[end];
                    valid = char.IsLetterOrDigit( ch ) || ch == '-' || ch == '_';

                    if ( valid )
                    {
                        end++;
                    }
                }

                // REF: https://datatracker.ietf.org/doc/html/rfc8288#appendix-B.3 #9
#pragma warning disable CA1308 // Normalize strings to uppercase (all ascii and should normalize to lowercase)
#if NETSTANDARD1_0
                var key = remaining.Substring( start, end - start ).ToLowerInvariant();
#else
                var key = new StringSegment( remaining.Substring( start, end - start ).ToLowerInvariant() );
#endif
#pragma warning restore CA1308 // Normalize strings to uppercase

                start = end;
                ConsumeWhitespace();

                if ( start > remaining.Length || remaining[start] != '=' )
                {
                    yield break;
                }
                else
                {
                    start++;
                }

                ConsumeWhitespace();

                if ( start > remaining.Length || remaining[start] != '"' )
                {
                    yield break;
                }

                end = remaining.IndexOf( '"', start + 1 );

                var value =
                    UnescapeAsQuotedString(
                        RemoveQuotes(
                            remaining
#if NETSTANDARD1_0
                                .Substring
#else
                                .Subsegment
#endif
#pragma warning disable SA1110 // Opening parenthesis or bracket should be on declaration line
                                ( start, end - start + 1 ) ) );
#pragma warning restore SA1110 // Opening parenthesis or bracket should be on declaration line

                yield return new( key, value );

                end = remaining.IndexOf( ';', end + 1 ) + 1;

                if ( end > 0 && end < remaining.Length )
                {
                    start = end;
                }

                ConsumeWhitespace();
            }
        }

        private void ConsumeWhitespace()
        {
            while ( start < remaining.Length && char.IsWhiteSpace( remaining[start] ) )
            {
                start++;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private static StringSegment RemoveQuotes( StringSegment input )
        {
            if ( IsQuoted( input ) )
            {
#if NETSTANDARD1_0
                return input.Substring( 1, input.Length - 2 );
#else
                return input.Subsegment( 1, input.Length - 2 );
#endif
            }

            return input;
        }

#pragma warning disable IDE0056 // Use index operator
        private static bool IsQuoted( StringSegment input ) =>
            !StringSegment.IsNullOrEmpty( input ) &&
            input.Length >= 2 &&
            input[0] == '"' &&
            input[input.Length - 1] == '"';
#pragma warning restore IDE0056 // Use index operator

        private static StringSegment UnescapeAsQuotedString( StringSegment input )
        {
            input = RemoveQuotes( input );

            var backSlashCount = CountBackslashesForDecodingQuotedString( input );

            if ( backSlashCount == 0 )
            {
                return input;
            }

#if NETSTANDARD1_0
            var buffer = new char[input.Length - backSlashCount];
            OnCreateString( buffer, input );
            return new( buffer );
#elif NETSTANDARD2_0
            Span<char> buffer = stackalloc char[input.Length - backSlashCount];
            OnCreateString( buffer, input );
            return buffer.ToString();
#else
            return string.Create( input.Length - backSlashCount, input, OnCreateString );
#endif
        }

        private static void OnCreateString(
#if NETSTANDARD1_0
            char[] span,
#else
            Span<char> span,
#endif
            StringSegment segment )
        {
            var spanIndex = 0;
            var spanLength = span.Length;

            for ( var i = 0; i < segment.Length && (uint) spanIndex < (uint) spanLength; i++ )
            {
                var nextIndex = i + 1;
                if ( (uint) nextIndex < (uint) segment.Length && segment[i] == '\\' )
                {
                    span[spanIndex] = segment[nextIndex];
                    i++;
                }
                else
                {
                    span[spanIndex] = segment[i];
                }

                spanIndex++;
            }
        }

        private static int CountBackslashesForDecodingQuotedString( StringSegment input )
        {
            var numberBackSlashes = 0;

            for ( var i = 0; i < input.Length; i++ )
            {
                if ( i < input.Length - 1 && input[i] == '\\' )
                {
                    if ( input[i + 1] == '\\' )
                    {
                        i++;
                    }

                    numberBackSlashes++;
                }
            }

            return numberBackSlashes;
        }
    }
}