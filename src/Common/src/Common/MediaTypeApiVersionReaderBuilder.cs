// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0079
#pragma warning disable SA1121
#pragma warning disable SA1135
#pragma warning disable SA1200

#if NETFRAMEWORK
using System.Net.Http.Headers;
using HttpRequest = System.Net.Http.HttpRequestMessage;
using MediaTypeHeaderValue = System.Net.Http.Headers.MediaTypeWithQualityHeaderValue;
#else
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using System.Collections.Frozen;
#endif

namespace Asp.Versioning;

#if !NETFRAMEWORK
using System.Buffers;
#endif
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
#if NETFRAMEWORK
using FrozenSet = HashSet<string>;
#else
using FrozenSet = FrozenSet<StringSegment>;
#endif
using ReaderCallback = Func<IReadOnlyList<MediaTypeHeaderValue>, IReadOnlyList<string>>;
using SelectorCallback = Func<HttpRequest, IReadOnlyList<string>, IReadOnlyList<string>>;
#if NETFRAMEWORK
using Str = String;
using StrComparer = StringComparer;
#else
using Str = StringSegment;
using StrComparer = StringSegmentComparer;
#endif
using static Asp.Versioning.ApiVersionParameterLocation;
using static System.StringComparison;

/// <summary>
/// Represents a builder for an API version reader that reads the value from a media type HTTP header in the request.
/// </summary>
public partial class MediaTypeApiVersionReaderBuilder
{
    private HashSet<string>? parameters;
    private HashSet<Str>? included;
    private HashSet<Str>? excluded;
    private SelectorCallback? select;
    private List<ReaderCallback>? readers;

    /// <summary>
    /// Adds the name of a media type parameter to be read.
    /// </summary>
    /// <param name="name">The name of the media type parameter.</param>
    /// <returns>The current <see cref="MediaTypeApiVersionReaderBuilder"/>.</returns>
    public virtual MediaTypeApiVersionReaderBuilder Parameter( string name )
    {
        if ( !string.IsNullOrEmpty( name ) )
        {
            parameters ??= new( StringComparer.OrdinalIgnoreCase );
            parameters.Add( name );
            AddReader( mediaTypes => ReadMediaTypeParameter( mediaTypes, name ) );
        }

        return this;
    }

    /// <summary>
    /// Excludes the specified media type from being read.
    /// </summary>
    /// <param name="name">The name of the media type to exclude.</param>
    /// <returns>The current <see cref="MediaTypeApiVersionReaderBuilder"/>.</returns>
    public virtual MediaTypeApiVersionReaderBuilder Exclude( string name )
    {
        if ( !string.IsNullOrEmpty( name ) )
        {
            excluded ??= new( StrComparer.OrdinalIgnoreCase );
            excluded.Add( name );
        }

        return this;
    }

    /// <summary>
    /// Includes the specified media type to be read.
    /// </summary>
    /// <param name="name">The name of the media type to include.</param>
    /// <returns>The current <see cref="MediaTypeApiVersionReaderBuilder"/>.</returns>
    public virtual MediaTypeApiVersionReaderBuilder Include( string name )
    {
        if ( !string.IsNullOrEmpty( name ) )
        {
            included ??= new( StrComparer.OrdinalIgnoreCase );
            included.Add( name );
        }

        return this;
    }

    /// <summary>
    /// Adds a pattern used to read an API version from a media type.
    /// </summary>
    /// <param name="pattern">The regular expression used to match the API version in the media type.</param>
    /// <returns>The current <see cref="MediaTypeApiVersionReaderBuilder"/>.</returns>
#if NETFRAMEWORK
    public virtual MediaTypeApiVersionReaderBuilder Match( string pattern )
#else
    public virtual MediaTypeApiVersionReaderBuilder Match( [StringSyntax( StringSyntaxAttribute.Regex )] string pattern )
#endif
    {
        if ( !string.IsNullOrEmpty( pattern ) )
        {
            AddReader( mediaTypes => ReadMediaType( mediaTypes, pattern ) );
        }

        return this;
    }

    /// <summary>
    /// Selects one or more raw API versions read from media types.
    /// </summary>
    /// <param name="selector">The <see cref="Func{T, TResult}">function</see> used to select results.</param>
    /// <returns>The current <see cref="MediaTypeApiVersionReaderBuilder"/>.</returns>
    /// <remarks>The selector will only be invoked if there is more than one value.</remarks>
#if !NETFRAMEWORK
    [CLSCompliant( false )]
#endif
#pragma warning disable CA1716 // Identifiers should not match keywords
    public virtual MediaTypeApiVersionReaderBuilder Select( SelectorCallback selector )
#pragma warning restore CA1716 // Identifiers should not match keywords
    {
        select = selector;
        return this;
    }

    /// <summary>
    /// Creates and returns a new API version reader.
    /// </summary>
    /// <returns>A new <see cref="IApiVersionReader">API version reader</see>.</returns>
#if !NETFRAMEWORK
    [CLSCompliant( false )]
#endif
    public virtual IApiVersionReader Build() =>
        new BuiltMediaTypeApiVersionReader(
            parameters?.ToArray() ?? [],
#if NET45
            included ?? [],
            excluded ?? [],
#elif NETFRAMEWORK
            included ?? new( capacity: 0 ),
            excluded ?? new( capacity: 0 ),
#else
            included?.ToFrozenSet( included.Comparer ) ?? FrozenSet<Str>.Empty,
            excluded?.ToFrozenSet( excluded.Comparer ) ?? FrozenSet<Str>.Empty,
#endif
            select ?? DefaultSelector,
            readers?.ToArray() ?? [] );

    /// <summary>
    /// Adds a function used to read the an API version from one or more media types.
    /// </summary>
    /// <param name="reader">The <see cref="Func{T, TResult}">function</see> used to read the API version.</param>
    /// <exception cref="ArgumentNullException"><paramref name="reader"/> is <c>null</c>.</exception>
#if !NETFRAMEWORK
    [CLSCompliant( false )]
#endif
    protected void AddReader( ReaderCallback reader )
    {
        ArgumentNullException.ThrowIfNull( reader );

        readers ??= [];
        readers.Add( reader );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static IReadOnlyList<string> DefaultSelector( HttpRequest request, IReadOnlyList<string> versions ) => versions;

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static string[] ToArray( ref string? version, List<string>? versions )
    {
        if ( version is null )
        {
            return [];
        }

        return versions is null ? [version] : [.. versions];
    }

    private static string[] ReadMediaType(
        IReadOnlyList<MediaTypeHeaderValue> mediaTypes,
        string pattern )
    {
        var version = default( string );
        var versions = default( List<string> );
        var regex = default( Regex );
        var count = mediaTypes.Count;

        for ( var i = 0; i < count; i++ )
        {
            var mediaType = mediaTypes[i].MediaType;

            if ( Str.IsNullOrEmpty( mediaType ) )
            {
                continue;
            }

            regex ??= new( pattern, RegexOptions.Singleline );

#if NETFRAMEWORK
            var input = mediaType;
#else
            var input = mediaType.Value!;
#endif
            var match = regex.Match( input );

            while ( match.Success )
            {
                var groups = match.Groups;
                var value = groups.Count > 1 ? groups[1].Value : match.Value;

                if ( version == null )
                {
                    version = value;
                }
                else if ( versions == null )
                {
                    versions = new( capacity: count - i + 1 )
                    {
                        version,
                        value,
                    };
                }
                else
                {
                    versions.Add( value );
                }

                match = match.NextMatch();
            }
        }

        return ToArray( ref version, versions );
    }

    private static string[] ReadMediaTypeParameter(
        IReadOnlyList<MediaTypeHeaderValue> mediaTypes,
        string parameterName )
    {
        var version = default( string );
        var versions = default( List<string> );
        var count = mediaTypes.Count;

        for ( var i = 0; i < count; i++ )
        {
            var mediaType = mediaTypes[i];

            foreach ( var parameter in mediaType.Parameters )
            {
                if ( !Str.Equals( parameterName, parameter.Name, OrdinalIgnoreCase ) ||
                      Str.IsNullOrEmpty( parameter.Value ) )
                {
                    continue;
                }

#if NETFRAMEWORK
                var value = parameter.Value;
#else
                var value = parameter.Value.Value!;
#endif
                if ( version == null )
                {
                    version = value;
                }
                else if ( versions == null )
                {
                    versions = new( capacity: count - i + 1 )
                    {
                        version,
                        value,
                    };
                }
                else
                {
                    versions.Add( value );
                }
            }
        }

        return ToArray( ref version, versions );
    }

    private sealed class BuiltMediaTypeApiVersionReader : IApiVersionReader
    {
        private readonly string[] parameters;
        private readonly FrozenSet included;
        private readonly FrozenSet excluded;
        private readonly SelectorCallback selector;
        private readonly ReaderCallback[] readers;

        internal BuiltMediaTypeApiVersionReader(
            string[] parameters,
            FrozenSet included,
            FrozenSet excluded,
            SelectorCallback selector,
            ReaderCallback[] readers )
        {
            this.parameters = parameters;
            this.included = included;
            this.excluded = excluded;
            this.selector = selector;
            this.readers = readers;
        }

        public void AddParameters( IApiVersionParameterDescriptionContext context )
        {
            ArgumentNullException.ThrowIfNull( context );

            if ( parameters.Length == 0 )
            {
                context.AddParameter( name: string.Empty, MediaTypeParameter );
            }
            else
            {
                for ( var i = 0; i < parameters.Length; i++ )
                {
                    context.AddParameter( parameters[i], MediaTypeParameter );
                }
            }
        }

        public IReadOnlyList<string> Read( HttpRequest request )
        {
            if ( readers.Length == 0 )
            {
                return Array.Empty<string>();
            }

#if NETFRAMEWORK
            var headers = request.Headers;
            var contentType = request.Content?.Headers.ContentType;
#else
            var headers = request.GetTypedHeaders();
            var contentType = headers.ContentType;
#endif
            var accept = headers.Accept;
            var version = default( string );
            var versions = default( SortedSet<string> );
            var mediaTypes = default( List<MediaTypeHeaderValue> );

            if ( contentType != null )
            {
#if NETFRAMEWORK
                mediaTypes = [MediaTypeHeaderValue.Parse( contentType.ToString() )];
#else
                mediaTypes = [contentType];
#endif
            }

            if ( accept != null && accept.Count > 0 )
            {
                mediaTypes ??= new( capacity: accept.Count );
                mediaTypes.AddRange( accept );
            }

            if ( mediaTypes == null )
            {
                return Array.Empty<string>();
            }

            Filter( mediaTypes );

            switch ( mediaTypes.Count )
            {
                case 0:
                    return Array.Empty<string>();
                case 1:
                    break;
                default:
                    mediaTypes.Sort( static ( l, r ) => -Nullable.Compare( l.Quality, r.Quality ) );
                    break;
            }

            Read( mediaTypes, ref version, ref versions );

            if ( versions == null )
            {
                return version == null ? Array.Empty<string>() : [version];
            }

            return selector( request, versions.ToArray() );
        }

        private void Filter( List<MediaTypeHeaderValue> mediaTypes )
        {
            if ( excluded.Count > 0 )
            {
                for ( var i = mediaTypes.Count - 1; i >= 0; i-- )
                {
                    var mediaType = mediaTypes[i].MediaType;

                    if ( Str.IsNullOrEmpty( mediaType ) || excluded.Contains( mediaType ) )
                    {
                        mediaTypes.RemoveAt( i );
                    }
                }
            }

            if ( included.Count == 0 )
            {
                return;
            }

            for ( var i = mediaTypes.Count - 1; i >= 0; i-- )
            {
                if ( !included.Contains( mediaTypes[i].MediaType! ) )
                {
                    mediaTypes.RemoveAt( i );
                }
            }
        }

        private void Read(
            IReadOnlyList<MediaTypeHeaderValue> mediaTypes,
            ref string? version,
            ref SortedSet<string>? versions )
        {
            for ( var i = 0; i < readers.Length; i++ )
            {
                var result = readers[i]( mediaTypes );

                for ( var j = 0; j < result.Count; j++ )
                {
                    if ( version == null )
                    {
                        version = result[j];
                    }
                    else if ( versions == null )
                    {
                        versions = new( StringComparer.OrdinalIgnoreCase )
                        {
                            version,
                            result[j],
                        };
                    }
                    else
                    {
                        versions.Add( result[j] );
                    }
                }
            }
        }
    }
}