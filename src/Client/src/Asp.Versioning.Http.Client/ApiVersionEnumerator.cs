// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0079
#pragma warning disable CA1815 // Override equals and operator equals on value types

namespace Asp.Versioning.Http;

#if NET
using System.Buffers;
#endif
using System.Collections;
#if NET
using static System.StringSplitOptions;
#endif

/// <summary>
/// Represents an enumerator of API versions from a HTTP header.
/// </summary>
public readonly struct ApiVersionEnumerator : IEnumerable<ApiVersion>
{
    private readonly string[] values;
    private readonly IApiVersionParser parser;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiVersionEnumerator"/> struct.
    /// </summary>
    /// <param name="response">The HTTP response to create the enumerator from.</param>
    /// <param name="headerName">The HTTP header name to enumerate.</param>
    /// <param name="parser">The optional <see cref="IApiVersionParser">API version parser</see>.</param>
    public ApiVersionEnumerator(
        HttpResponseMessage response,
        string headerName,
        IApiVersionParser? parser = default )
    {
        ArgumentNullException.ThrowIfNull( response );
        ArgumentException.ThrowIfNullOrEmpty( headerName );

        this.values = response.Headers.TryGetValues( headerName, out var values ) ? [.. values] : [];
        this.parser = parser ?? ApiVersionParser.Default;
    }

    /// <inheritdoc />
    public IEnumerator<ApiVersion> GetEnumerator()
    {
#if NETSTANDARD
        for ( var i = 0; i < values.Length; i++ )
        {
            var items = values[i].Split( ',' );

            for ( var j = 0; j < items.Length; j++ )
            {
                var item = items[j].Trim();

                if ( item.Length > 0 && parser.TryParse( item, out var result ) )
                {
                    yield return result!;
                }
            }
        }
#else
        for ( var i = 0; i < values.Length; i++ )
        {
            var (count, versions) = ParseVersions( values[i] );

            for ( var j = 0; j < count; j++ )
            {
                yield return versions[j];
            }
        }
#endif
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
#if NET
    private (int Count, ApiVersion[] Results) ParseVersions( ReadOnlySpan<char> value )
    {
        var pool = ArrayPool<Range>.Shared;
        var ranges = pool.Rent( 5 );
        var length = value.Split( ranges, ',', RemoveEmptyEntries | TrimEntries );

        while ( length >= ranges.Length )
        {
            pool.Return( ranges );
            length <<= 1;
            ranges = pool.Rent( length );
            length = value.Split( ranges, ',', RemoveEmptyEntries | TrimEntries );
        }

        var results = new ApiVersion[length];
        var count = 0;

        for ( var i = 0; i < length; i++ )
        {
            var text = value[ranges[i]];

            if ( text.Length > 0 && parser.TryParse( text, out var result ) )
            {
                results[count++] = result;
            }
        }

        pool.Return( ranges );
        return (count, results);
    }
#endif
}