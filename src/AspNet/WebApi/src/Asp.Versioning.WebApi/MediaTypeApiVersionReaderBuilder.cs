// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using Asp.Versioning.Routing;
using System.Globalization;
using System.Net.Http.Headers;
using System.Web.Http.Routing;

/// <content>
/// Provides additional implementation specific to ASP.NET Web API.
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
    public virtual MediaTypeApiVersionReaderBuilder Template( string template, string? parameterName = default )
    {
        ArgumentException.ThrowIfNullOrEmpty( template );

        if ( string.IsNullOrEmpty( parameterName ) )
        {
            var parser = new RouteParser();
            var parsedRoute = parser.Parse( template );
            var segments = from content in parsedRoute.PathSegments.OfType<IPathContentSegment>()
                           from segment in content.Subsegments.OfType<IPathParameterSubsegment>()
                           select segment;

            if ( segments.Count() > 1 )
            {
                var message = string.Format( CultureInfo.CurrentCulture, CommonSR.InvalidMediaTypeTemplate, template );
                throw new System.ArgumentException( message, nameof( template ) );
            }
        }

        var route = new HttpRoute( template );

        AddReader( mediaTypes => ReadMediaTypePattern( mediaTypes, route, parameterName ) );

        return this;
    }

    private static IReadOnlyList<string> ReadMediaTypePattern(
        IReadOnlyList<MediaTypeHeaderValue> mediaTypes,
        HttpRoute route,
        string? parameterName )
    {
        var assumeOneParameter = string.IsNullOrEmpty( parameterName );
        var version = default( string );
        var versions = default( List<string> );
        using var request = new HttpRequestMessage();

        for ( var i = 0; i < mediaTypes.Count; i++ )
        {
            var mediaType = mediaTypes[i].MediaType;
            request.RequestUri = new Uri( "http://localhost/" + mediaType );
            var data = route.GetRouteData( string.Empty, request );

            if ( data == null )
            {
                continue;
            }

            var values = data.Values;

            if ( values.Count == 0 )
            {
                continue;
            }

            object datum;

            if ( assumeOneParameter )
            {
                datum = values.Values.First();
            }
            else if ( !values.TryGetValue( parameterName, out datum ) )
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