// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.Net.Http.Headers;
using System.Globalization;

/// <content>
/// Provides additional implementation specific to ASP.NET Core.
/// </content>
public partial class MediaTypeApiVersionReaderBuilder
{
    /// <summary>
    /// Adds a template used to read an API version from a media type.
    /// </summary>
    /// <param name="template">The template used to match the media type.</param>
    /// <param name="parameterName">The optional name of the API version parameter in the template.
    /// If a value is not specified, there is expected to be a single template parameter.</param>
    /// <returns>The current <see cref="MediaTypeApiVersionReaderBuilder"/>.</returns>
    /// <remarks>The template syntax is the same used by route templates; however, constraints are not supported.</remarks>
#pragma warning disable IDE0079
#pragma warning disable CA1716 // Identifiers should not match keywords
    public virtual MediaTypeApiVersionReaderBuilder Template( string template, string? parameterName = default )
#pragma warning restore CA1716 // Identifiers should not match keywords
#pragma warning restore IDE0079
    {
        ArgumentException.ThrowIfNullOrEmpty( template );

        var routePattern = RoutePatternFactory.Parse( template );

        if ( string.IsNullOrEmpty( parameterName ) && routePattern.Parameters.Count > 1 )
        {
            var message = string.Format( CultureInfo.CurrentCulture, Format.InvalidMediaTypeTemplate, template );
            throw new ArgumentException( message, nameof( template ) );
        }

        var defaults = new RouteValueDictionary( routePattern.RequiredValues );
        var matcher = new TemplateMatcher( new( routePattern ), defaults );

        AddReader( mediaTypes => ReadMediaTypePattern( mediaTypes, matcher, parameterName ) );

        return this;
    }

    private static string[] ReadMediaTypePattern(
        IReadOnlyList<MediaTypeHeaderValue> mediaTypes,
        TemplateMatcher matcher,
        string? parameterName )
    {
        const char RequiredPrefix = '/';
        var assumeOneParameter = string.IsNullOrEmpty( parameterName );
        var version = default( string );
        var versions = default( List<string> );
        var values = new RouteValueDictionary();

        for ( var i = 0; i < mediaTypes.Count; i++ )
        {
            var mediaType = mediaTypes[i].MediaType.Value;
            var path = new PathString( RequiredPrefix + mediaType );

            values.Clear();

            if ( !matcher.TryMatch( path, values ) || values.Count == 0 )
            {
                continue;
            }

            object? datum;

            if ( assumeOneParameter )
            {
                datum = values.Values.First();
            }
            else if ( !values.TryGetValue( parameterName!, out datum ) )
            {
                continue;
            }

            if ( datum is not string value || string.IsNullOrEmpty( value ) )
            {
                continue;
            }

            if ( version == null )
            {
                version = value;
            }
            else if ( versions == null )
            {
                versions = new( capacity: mediaTypes.Count - i + 1 )
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

        return ToArray( ref version, versions );
    }
}