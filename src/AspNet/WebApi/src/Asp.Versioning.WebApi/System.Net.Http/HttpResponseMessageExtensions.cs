// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace System.Net.Http;

using Asp.Versioning;
using Backport;
using System.Collections.Generic;
using System.Net.Http.Headers;

/// <summary>
/// Provides extension methods for <see cref="HttpResponseMessage"/>.
/// </summary>
public static class HttpResponseMessageExtensions
{
    private const string Sunset = nameof( Sunset );
    private const string Link = nameof( Link );

    /// <summary>
    /// Writes the sunset policy to the specified HTTP response.
    /// </summary>
    /// <param name="response">The <see cref="HttpResponseMessage">HTTP response</see> to write to.</param>
    /// <param name="sunsetPolicy">The <see cref="SunsetPolicy">sunset policy</see> to write.</param>
    public static void WriteSunsetPolicy( this HttpResponseMessage response, SunsetPolicy sunsetPolicy )
    {
        ArgumentNullException.ThrowIfNull( response );
        ArgumentNullException.ThrowIfNull( sunsetPolicy );

        var headers = response.Headers;

        if ( sunsetPolicy.Date.HasValue )
        {
            headers.Add( Sunset, sunsetPolicy.Date.Value.ToString( "r" ) );
        }

        AddLinkHeaders( headers, sunsetPolicy.Links );
    }

    private static void AddLinkHeaders( HttpResponseHeaders headers, IList<LinkHeaderValue> links )
    {
        var values = headers.TryGetValues( Link, out var existing )
            ? existing is ICollection<string> collection && !collection.IsReadOnly ? collection : new List<string>( existing )
            : new List<string>( capacity: links.Count );

        for ( var i = 0; i < links.Count; i++ )
        {
            values.Add( links[i].ToString() );
        }

        headers.Remove( Link );
        headers.Add( Link, values );
    }
}