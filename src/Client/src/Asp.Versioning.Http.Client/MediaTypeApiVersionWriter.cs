// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Http;

using System.Net.Http.Headers;
using static System.StringComparison;

/// <summary>
/// Represents an API version writer that writes the value from a media type HTTP header in the request.
/// </summary>
public sealed class MediaTypeApiVersionWriter : IApiVersionWriter
{
    private readonly string parameterName;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaTypeApiVersionWriter"/> class.
    /// </summary>
    /// <remarks>This constructor always uses the "v" media type parameter.</remarks>
    public MediaTypeApiVersionWriter() => parameterName = "v";

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaTypeApiVersionWriter"/> class.
    /// </summary>
    /// <param name="parameterName">The name of the media type parameter to write the API version to.</param>
    public MediaTypeApiVersionWriter( string parameterName )
    {
        ArgumentException.ThrowIfNullOrEmpty( parameterName );
        this.parameterName = parameterName;
    }

    /// <inheritdoc />
    public void Write( HttpRequestMessage request, ApiVersion apiVersion )
    {
        ArgumentNullException.ThrowIfNull( request );
        ArgumentNullException.ThrowIfNull( apiVersion );

        UpdateAccept( request, apiVersion );

        if ( request.Method == HttpMethod.Get )
        {
            return;
        }

        if ( request.Content is HttpContent content &&
             content.Headers.ContentType is MediaTypeHeaderValue contentType )
        {
            UpdateMediaType( contentType, apiVersion );
        }
    }

    private void UpdateAccept( HttpRequestMessage request, ApiVersion apiVersion )
    {
        var accept = request.Headers.Accept;

        if ( accept.Count == 0 )
        {
            return;
        }

        foreach ( var mediaType in accept )
        {
            UpdateMediaType( mediaType, apiVersion );
        }
    }

    private void UpdateMediaType( MediaTypeHeaderValue mediaType, ApiVersion apiVersion )
    {
        var parameters = mediaType.Parameters;

        if ( parameters.Count > 0 )
        {
            foreach ( var parameter in parameters )
            {
                if ( string.Equals( parameter.Name, parameterName, OrdinalIgnoreCase ) )
                {
                    return;
                }
            }
        }

        parameters.Add( new( parameterName, apiVersion.ToString() ) );
    }
}