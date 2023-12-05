// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

#if NETFRAMEWORK
using System.Net.Http.Headers;
#else
using Microsoft.Net.Http.Headers;
using MediaTypeWithQualityHeaderValue = Microsoft.Net.Http.Headers.MediaTypeHeaderValue;
#endif
using static Asp.Versioning.ApiVersionParameterLocation;
using static System.StringComparison;

/// <summary>
/// Represents an API version reader that reads the value from a media type HTTP header in the request.
/// </summary>
public partial class MediaTypeApiVersionReader : IApiVersionReader
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MediaTypeApiVersionReader"/> class.
    /// </summary>
    /// <remarks>This constructor always uses the "v" media type parameter.</remarks>
    public MediaTypeApiVersionReader() => ParameterName = "v";

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaTypeApiVersionReader"/> class.
    /// </summary>
    /// <param name="parameterName">The name of the media type parameter to read the API version from.</param>
    public MediaTypeApiVersionReader( string parameterName )
    {
        ArgumentException.ThrowIfNullOrEmpty( parameterName );
        ParameterName = parameterName;
    }

    /// <summary>
    /// Gets or sets the name of the media type parameter to read the API version from.
    /// </summary>
    /// <value>The name of the media type parameter to read the API version from.
    /// The default value is "v".</value>
    public string ParameterName { get; set; }

    /// <summary>
    /// Reads the requested API version from the HTTP Accept header.
    /// </summary>
    /// <param name="accept">The <see cref="ICollection{T}">collection</see> of Accept
    /// <see cref="MediaTypeWithQualityHeaderValue">headers</see> to read from.</param>
    /// <returns>The API version read or <c>null</c>.</returns>
    /// <remarks>The default implementation will return the first defined API version ranked by the media type
    /// quality parameter.</remarks>
    protected virtual string? ReadAcceptHeader( ICollection<MediaTypeWithQualityHeaderValue> accept )
    {
        ArgumentNullException.ThrowIfNull( accept );

        var count = accept.Count;

        if ( count == 0 )
        {
            return default;
        }

        var mediaTypes = accept.ToArray();

        System.Array.Sort( mediaTypes, ByQualityDescending );

        for ( var i = 0; i < count; i++ )
        {
#if NETFRAMEWORK
            var parameters = mediaTypes[i].Parameters.ToArray();
            var paramCount = parameters.Length;
#else
            var parameters = mediaTypes[i].Parameters;
            var paramCount = parameters.Count;
#endif
            for ( var j = 0; j < paramCount; j++ )
            {
                var parameter = parameters[j];

                if ( parameter.Name.Equals( ParameterName, OrdinalIgnoreCase ) )
                {
#if NETFRAMEWORK
                    return parameter.Value;
#else
                    return parameter.Value.Value;
#endif
                }
            }
        }

        return default;
    }

    /// <summary>
    /// Reads the requested API version from the HTTP Content-Type header.
    /// </summary>
    /// <param name="contentType">The Content-Type <see cref="MediaTypeHeaderValue">header</see> to read from.</param>
    /// <returns>The API version read or <c>null</c>.</returns>
    protected virtual string? ReadContentTypeHeader( MediaTypeHeaderValue contentType )
    {
        ArgumentNullException.ThrowIfNull( contentType );
#if NETFRAMEWORK
        var parameters = contentType.Parameters.ToArray();
        var count = parameters.Length;
#else
        var parameters = contentType.Parameters;
        var count = parameters.Count;
#endif
        for ( var i = 0; i < count; i++ )
        {
            var parameter = parameters[i];

            if ( parameter.Name.Equals( ParameterName, OrdinalIgnoreCase ) )
            {
#if NETFRAMEWORK
                return parameter.Value;
#else
                return parameter.Value.Value;
#endif
            }
        }

        return default;
    }

    /// <summary>
    /// Provides API version parameter descriptions supported by the current reader using the supplied provider.
    /// </summary>
    /// <param name="context">The <see cref="IApiVersionParameterDescriptionContext">context</see> used to add API version parameter descriptions.</param>
    public virtual void AddParameters( IApiVersionParameterDescriptionContext context )
    {
        ArgumentNullException.ThrowIfNull( context );
        context.AddParameter( ParameterName, MediaTypeParameter );
    }

    private static int ByQualityDescending( MediaTypeWithQualityHeaderValue? left, MediaTypeWithQualityHeaderValue? right ) =>
        -Nullable.Compare( left?.Quality, right?.Quality );
}