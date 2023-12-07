// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

#if NETFRAMEWORK
using HttpRequest = System.Net.Http.HttpRequestMessage;
#else
using Microsoft.AspNetCore.Http;
#endif

/// <summary>
/// Provides utility functions for API version readers.
/// </summary>
#if !NETFRAMEWORK
[CLSCompliant( false )]
#endif
public static class ApiVersionReader
{
    private static IApiVersionReader? @default;

    /// <summary>
    /// Gets the default API version reader.
    /// </summary>
    /// <value>The default <see cref="IApiVersionReader"/>.</value>
    public static IApiVersionReader Default => @default ??= Combine( new QueryStringApiVersionReader(), new UrlSegmentApiVersionReader() );

    /// <summary>
    /// Returns a new API version reader that is a combination of the specified set.
    /// </summary>
    /// <param name="apiVersionReader">The primary <see cref="IApiVersionReader">API version reader</see>.</param>
    /// <param name="otherApiVersionReaders">An array of the other
    /// <see cref="IApiVersionReader">API version readers</see> to combine.</param>
    /// <returns>A new, combined <see cref="IApiVersionReader">API version reader</see>.</returns>
    public static IApiVersionReader Combine(
        IApiVersionReader apiVersionReader,
        params IApiVersionReader[] otherApiVersionReaders )
    {
        ArgumentNullException.ThrowIfNull( apiVersionReader );

        int count;
        IApiVersionReader[] apiVersionReaders;

        if ( otherApiVersionReaders is null || ( count = otherApiVersionReaders.Length ) == 0 )
        {
            apiVersionReaders = [apiVersionReader];
        }
        else
        {
            apiVersionReaders = new IApiVersionReader[count + 1];
            apiVersionReaders[0] = apiVersionReader;
            System.Array.Copy( otherApiVersionReaders, 0, apiVersionReaders, 1, count );
        }

        return new CombinedApiVersionReader( apiVersionReaders );
    }

    /// <summary>
    /// Returns a new API version reader that is a combination of the specified set.
    /// </summary>
    /// <param name="apiVersionReaders">The <see cref="IEnumerable{T}">sequence</see> of
    /// <see cref="IApiVersionReader">API version readers</see> to combine.</param>
    /// <returns>A new, combined <see cref="IApiVersionReader">API version reader</see>.</returns>
    public static IApiVersionReader Combine( IEnumerable<IApiVersionReader> apiVersionReaders )
    {
        var readers = apiVersionReaders?.ToArray();

        if ( readers is null || readers.Length == 0 )
        {
            throw new System.ArgumentException( CommonSR.ZeroApiVersionReaders, nameof( apiVersionReaders ) );
        }

        return new CombinedApiVersionReader( readers );
    }

    private sealed class CombinedApiVersionReader : IApiVersionReader
    {
        private readonly IApiVersionReader[] apiVersionReaders;

        internal CombinedApiVersionReader( IApiVersionReader[] apiVersionReaders ) =>
            this.apiVersionReaders = apiVersionReaders;

        public IReadOnlyList<string> Read( HttpRequest request )
        {
            var count = apiVersionReaders.Length;
            var version = default( string );
            var versions = default( SortedSet<string> );

            for ( var i = 0; i < count; i++ )
            {
                var apiVersionReader = apiVersionReaders[i];
                var values = apiVersionReader.Read( request );

                for ( var j = 0; j < values.Count; j++ )
                {
                    var value = values[j];

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

        public void AddParameters( IApiVersionParameterDescriptionContext context )
        {
            for ( var i = 0; i < apiVersionReaders.Length; i++ )
            {
                apiVersionReaders[i].AddParameters( context );
            }
        }
    }
}