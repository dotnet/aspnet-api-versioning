// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Microsoft.AspNetCore.Http;

using Asp.Versioning;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

/// <summary>
/// Provides extension methods for <see cref="HttpResponse"/>.
/// </summary>
[CLSCompliant( false )]
public static class HttpResponseExtensions
{
    private const string Sunset = nameof( Sunset );
    private const string Link = nameof( Link );

    /// <summary>
    /// Writes the sunset policy to the specified HTTP response.
    /// </summary>
    /// <param name="response">The <see cref="HttpResponseMessage">HTTP response</see> to write to.</param>
    /// <param name="sunsetPolicy">The <see cref="SunsetPolicy">sunset policy</see> to write.</param>
    [CLSCompliant( false )]
    public static void WriteSunsetPolicy( this HttpResponse response, SunsetPolicy sunsetPolicy )
    {
        ArgumentNullException.ThrowIfNull( response );
        ArgumentNullException.ThrowIfNull( sunsetPolicy );

        var headers = response.Headers;

        if ( headers.ContainsKey( Sunset ) )
        {
            // the 'Sunset' header is present, assume the headers have been written.
            // this can happen when ApiVersioningOptions.ReportApiVersions = true
            // and [ReportApiVersions] are both applied
            return;
        }

        if ( sunsetPolicy.Date.HasValue )
        {
            headers[Sunset] = sunsetPolicy.Date.Value.ToString( "r" );
        }

        AddLinkHeaders( headers, sunsetPolicy.Links );
    }

    private static void AddLinkHeaders( IHeaderDictionary headers, IList<LinkHeaderValue> links )
    {
        var values = new string[links.Count];

        for ( var i = 0; i < links.Count; i++ )
        {
            values[i] = links[i].ToString();
        }

        headers.Append( Link, values );
    }

    /// <summary>
    /// Attempts to add the requested API version to the response content type.
    /// </summary>
    /// <param name="response">The extended <see cref="HttpResponse">HTTP response</see>.</param>
    /// <param name="name">The name of the API version parameter.</param>
    /// <remarks>This method performs no action if the requested API version is unavailable,
    /// the parameter is already set, or the response does not indicate success.</remarks>
    public static void AddApiVersionToContentType( this HttpResponse response, string name )
    {
        ArgumentNullException.ThrowIfNull( response );

        if ( response.StatusCode < 200 && response.StatusCode > 299 )
        {
            return;
        }

        var headers = response.GetTypedHeaders();
        var contentType = headers.ContentType;

        if ( contentType == null )
        {
            return;
        }

        var feature = response.HttpContext.ApiVersioningFeature();

        if ( feature.RawRequestedApiVersion is not string apiVersion )
        {
            return;
        }

        var parameters = contentType.Parameters;
        var parameter = default( NameValueHeaderValue );

        for ( var i = 0; i < parameters.Count; i++ )
        {
            if ( parameters[i].Name.Equals( name, StringComparison.OrdinalIgnoreCase ) )
            {
                parameter = parameters[i];
                break;
            }
        }

        if ( parameter == null )
        {
            parameter = new( name );
            parameters.Add( parameter );
        }

        if ( !parameter.Value.HasValue )
        {
            parameter.Value = new( apiVersion );
            headers.ContentType = contentType;
        }
    }
}