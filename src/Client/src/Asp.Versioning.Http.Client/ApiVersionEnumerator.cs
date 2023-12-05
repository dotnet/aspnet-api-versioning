// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0079
#pragma warning disable CA1815 // Override equals and operator equals on value types

namespace Asp.Versioning.Http;

using System.Collections;

/// <summary>
/// Represents an enumerator of API versions from a HTTP header.
/// </summary>
public readonly struct ApiVersionEnumerator : IEnumerable<ApiVersion>
{
    private readonly IEnumerable<string> values;
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

        this.values =
            response.Headers.TryGetValues( headerName, out var values )
            ? values
            : Enumerable.Empty<string>();

        this.parser = parser ?? ApiVersionParser.Default;
    }

    /// <inheritdoc />
    public IEnumerator<ApiVersion> GetEnumerator()
    {
        using var iterator = values.GetEnumerator();

        if ( !iterator.MoveNext() )
        {
            yield break;
        }

        if ( parser.TryParse( iterator.Current, out var value ) )
        {
            yield return value!;
        }

        while ( iterator.MoveNext() )
        {
            if ( parser.TryParse( iterator.Current, out value ) )
            {
                yield return value!;
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}