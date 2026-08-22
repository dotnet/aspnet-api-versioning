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
    private readonly bool acceptHeaderOverridden;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaTypeApiVersionReader"/> class.
    /// </summary>
    /// <remarks>This constructor always uses the "v" media type parameter.</remarks>
    public MediaTypeApiVersionReader()
    {
        ParameterName = "v";
        acceptHeaderOverridden = IsAcceptHeaderOverridden();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaTypeApiVersionReader"/> class.
    /// </summary>
    /// <param name="parameterName">The name of the media type parameter to read the API version from.</param>
    public MediaTypeApiVersionReader( string parameterName )
    {
        ArgumentException.ThrowIfNullOrEmpty( parameterName );
        ParameterName = parameterName;
        acceptHeaderOverridden = IsAcceptHeaderOverridden();
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
    /// <remarks>
    /// <para>This method returns the first defined API version ranked by the media type quality parameter. A media
    /// type without a quality parameter has the highest weight of 1.0, and a media type with a quality of zero is
    /// never considered because it means 'not acceptable'.</para>
    /// <para>An API version reader discovers and collates API versions rather than selecting one, and equally
    /// ranked media types can yield more than one. This method cannot express that, so it is only used when a
    /// derived class overrides it; an overriding implementation is responsible for ranking and resolving the
    /// Accept header itself. When it is not overridden, an internal implementation that collates equally ranked
    /// media types is used instead.</para>
    /// <para>This method will be refactored in a future major version.</para>
    /// </remarks>
    protected virtual string? ReadAcceptHeader( ICollection<MediaTypeWithQualityHeaderValue> accept )
    {
        // TODO: refactor breaking change at next major version
        ArgumentNullException.ThrowIfNull( accept );
        return ReadRankedAcceptHeader( accept ) is { } versions ? versions[0] : default;
    }

    // this is the correct, expected behavior because equally ranked media types can collate more than one API version.
    // it cannot replace ReadAcceptHeader before the next major version because widening the return type is a breaking
    // change for any derived class that overrides it. this becomes the permanent implementation when a breaking change
    // is allowed, at which point ReadAcceptHeader and the override detection it requires can both be removed
    private List<string>? ReadRankedAcceptHeader( ICollection<MediaTypeWithQualityHeaderValue> accept )
    {
        var count = accept.Count;

        if ( count == 0 )
        {
            return default;
        }

        var mediaTypes = accept.ToArray();

        MediaTypeQuality.SortDescending( mediaTypes );

        var versions = default( List<string> );
        var start = 0;

        while ( start < count )
        {
            // neither this media type nor any that follow can be considered
            if ( !MediaTypeQuality.IsAcceptable( mediaTypes[start] ) )
            {
                break;
            }

            var end = start + 1;

            while ( end < count && MediaTypeQuality.SameRank( mediaTypes[end], mediaTypes[start] ) )
            {
                end++;
            }

            // media types collated at the same rank are equally preferred
            for ( var i = start; i < end; i++ )
            {
                if ( ReadParameter( mediaTypes[i] ) is not string value )
                {
                    continue;
                }

                versions ??= new( capacity: end - start );

                if ( !versions.Contains( value, StringComparer.OrdinalIgnoreCase ) )
                {
                    versions.Add( value );
                }
            }

            // a higher rank breaks precedence over every media type ranked below it
            if ( versions is not null )
            {
                break;
            }

            start = end;
        }

        return versions;
    }

    private bool IsAcceptHeaderOverridden()
    {
        if ( GetType() == typeof( MediaTypeApiVersionReader ) )
        {
            return false;
        }

        var readAcceptHeader = ReadAcceptHeader;

        return readAcceptHeader.Method.DeclaringType != typeof( MediaTypeApiVersionReader );
    }

    private static IReadOnlyList<string> Collate( string? version, string? otherVersion )
    {
        if ( otherVersion is null )
        {
            return version is null ? [] : [version];
        }

        return version is null || StringComparer.OrdinalIgnoreCase.Equals( version, otherVersion )
               ? [otherVersion]
               : [version, otherVersion];
    }

    private static List<string> Collate( string? version, List<string>? versions )
    {
        if ( versions is null || versions.Count == 0 )
        {
            return version is null ? [] : [version];
        }

        if ( version is null )
        {
            return versions;
        }

        // the content-type version is ranked first among the equally ranked versions it is collated with
        var collated = new List<string>( capacity: versions.Count + 1 ) { version };

        for ( var i = 0; i < versions.Count; i++ )
        {
            if ( !StringComparer.OrdinalIgnoreCase.Equals( version, versions[i] ) )
            {
                collated.Add( versions[i] );
            }
        }

        return collated;
    }

    private string? ReadParameter( MediaTypeHeaderValue mediaType )
    {
#if NETFRAMEWORK
        var parameters = mediaType.Parameters.ToArray();
        var count = parameters.Length;
#else
        var parameters = mediaType.Parameters;
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
    /// Reads the requested API version from the HTTP Content-Type header.
    /// </summary>
    /// <param name="contentType">The Content-Type <see cref="MediaTypeHeaderValue">header</see> to read from.</param>
    /// <returns>The API version read or <c>null</c>.</returns>
    protected virtual string? ReadContentTypeHeader( MediaTypeHeaderValue contentType )
    {
        ArgumentNullException.ThrowIfNull( contentType );
        return ReadParameter( contentType );
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
}