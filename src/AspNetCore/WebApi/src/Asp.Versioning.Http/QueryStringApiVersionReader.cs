// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using Microsoft.AspNetCore.Http;
using System.Buffers;

/// <content>
/// Provides the implementation for ASP.NET Core.
/// </content>
[CLSCompliant( false )]
public partial class QueryStringApiVersionReader
{
    /// <inheritdoc />
    public virtual IReadOnlyList<string> Read( HttpRequest request )
    {
        ArgumentNullException.ThrowIfNull( request );

        var count = ParameterNames.Count;

        if ( count == 0 )
        {
            return Array.Empty<string>();
        }

        var version = default( string );
        var versions = default( SortedSet<string> );
        var pool = ArrayPool<string>.Shared;
        var names = pool.Rent( count );

        ParameterNames.CopyTo( names, 0 );

        for ( var i = 0; i < count; i++ )
        {
            var values = request.Query[names[i]];

            for ( var j = 0; j < values.Count; j++ )
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

        pool.Return( names );

        if ( versions == null )
        {
            return version == null ? [] : [version];
        }

        return versions.ToArray();
    }
}