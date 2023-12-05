// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

/// <content>
/// Provides the implementation for ASP.NET Web API.
/// </content>
public partial class HeaderApiVersionReader
{
    /// <inheritdoc />
    public virtual IReadOnlyList<string> Read( HttpRequestMessage request )
    {
        ArgumentNullException.ThrowIfNull( request );

        var count = HeaderNames.Count;

        if ( count == 0 )
        {
            return Array.Empty<string>();
        }

        var version = default( string );
        var versions = default( SortedSet<string> );
        var names = new string[count];
        var headers = request.Headers;

        HeaderNames.CopyTo( names, 0 );

        for ( var i = 0; i < count; i++ )
        {
            if ( !headers.TryGetValues( names[i], out var headerValues ) )
            {
                continue;
            }

            var values = headerValues.ToArray();

            for ( var j = 0; j < values.Length; j++ )
            {
                var value = values[j];

                if ( string.IsNullOrEmpty( value ) )
                {
                    continue;
                }

                if ( version == null )
                {
                    version = value;
                }
                else if ( versions == null )
                {
                    versions = new( StringComparer.OrdinalIgnoreCase )
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
        }

        if ( versions == null )
        {
            return version == null ? [] : [version];
        }

        return versions.ToArray();
    }
}