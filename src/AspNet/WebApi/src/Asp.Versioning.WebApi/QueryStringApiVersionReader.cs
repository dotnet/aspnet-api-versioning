// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

/// <content>
/// Provides the implementation for ASP.NET Web API.
/// </content>
public partial class QueryStringApiVersionReader
{
    /// <inheritdoc />
    public virtual IReadOnlyList<string> Read( HttpRequestMessage request )
    {
        ArgumentNullException.ThrowIfNull(request);

        var count = ParameterNames.Count;

        if ( count == 0 )
        {
            return Array.Empty<string>();
        }

        var version = default( string );
        var versions = default( SortedSet<string> );
        var names = new string[count];
        var comparer = StringComparer.OrdinalIgnoreCase;

        ParameterNames.CopyTo( names, 0 );

        foreach ( var pair in request.GetQueryNameValuePairs() )
        {
            for ( var i = 0; i < count; i++ )
            {
                var parameterName = names[i];
                var value = pair.Value;

                if ( value.Length == 0 || !comparer.Equals( parameterName, pair.Key ) )
                {
                    continue;
                }

                if ( version == null )
                {
                    version = value;
                }
                else if ( versions == null )
                {
                    versions = new( comparer )
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