// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Http;

using System.Collections;

internal readonly struct ApiVersionEnumerator : IEnumerable<ApiVersion>
{
    private const string ApiSupportedVersions = "api-supported-versions";
    private const string ApiDeprecatedVersions = "api-deprecated-versions";
    private readonly IEnumerable<string> values;
    private readonly IApiVersionParser? parser;

    private ApiVersionEnumerator( IEnumerable<string> values, IApiVersionParser? parser = default )
    {
        this.values = values;
        this.parser = parser;
    }

    public IEnumerator<ApiVersion> GetEnumerator()
    {
        using var iterator = values.GetEnumerator();

        if ( !iterator.MoveNext() )
        {
            yield break;
        }

        var parser = this.parser ?? ApiVersionParser.Default;

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

    internal static ApiVersionEnumerator Supported(
        HttpResponseMessage response,
        IApiVersionParser? parser = default )
    {
        if ( response.Headers.TryGetValues( ApiSupportedVersions, out var values ) )
        {
            return new( values, parser );
        }

        return new( Enumerable.Empty<string>() );
    }

    internal static ApiVersionEnumerator Deprecated(
        HttpResponseMessage response,
        IApiVersionParser? parser = default )
    {
        if ( response.Headers.TryGetValues( ApiDeprecatedVersions, out var values ) )
        {
            return new( values, parser );
        }

        return new( Enumerable.Empty<string>() );
    }
}