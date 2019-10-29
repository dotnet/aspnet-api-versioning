#if WEBAPI
namespace Microsoft.Web.Http.Versioning
#else
namespace Microsoft.AspNetCore.Mvc.Versioning
#endif
{
#if !WEBAPI
    using Microsoft.AspNetCore.Http;
#endif
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using static System.String;
#if WEBAPI
    using HttpRequest = System.Net.Http.HttpRequestMessage;
#endif

    /// <summary>
    /// Provides utility functions for service API version readers.
    /// </summary>
    public static class ApiVersionReader
    {
        /// <summary>
        /// Returns a new API version reader that is a combination of the specified set.
        /// </summary>
        /// <param name="apiVersionReaders">The <see cref="Array">array</see> of
        /// <see cref="IApiVersionReader">API version readers</see> to combine.</param>
        /// <returns>A new, unioned <see cref="IApiVersionReader">API version reader</see>.</returns>
#if !WEBAPI
        [CLSCompliant( false )]
#endif
        public static IApiVersionReader Combine( params IApiVersionReader[] apiVersionReaders )
        {
            if ( apiVersionReaders.Length == 0 )
            {
                throw new ArgumentException( SR.ZeroApiVersionReaders, nameof( apiVersionReaders ) );
            }

            return new CombinedApiVersionReader( apiVersionReaders );
        }

        /// <summary>
        /// Returns a new API version reader that is a combination of the specified set.
        /// </summary>
        /// <param name="apiVersionReaders">The <see cref="IEnumerable{T}">sequence</see> of
        /// <see cref="IApiVersionReader">API version readers</see> to combine.</param>
        /// <returns>A new, unioned <see cref="IApiVersionReader">API version reader</see>.</returns>
#if !WEBAPI
        [CLSCompliant( false )]
#endif
        public static IApiVersionReader Combine( IEnumerable<IApiVersionReader> apiVersionReaders )
        {
            var items = apiVersionReaders.ToArray();

            if ( items.Length == 0 )
            {
                throw new ArgumentException( SR.ZeroApiVersionReaders, nameof( apiVersionReaders ) );
            }

            return new CombinedApiVersionReader( items );
        }

        sealed class CombinedApiVersionReader : IApiVersionReader
        {
            readonly IApiVersionReader[] apiVersionReaders;

            internal CombinedApiVersionReader( IApiVersionReader[] apiVersionReaders ) =>
                this.apiVersionReaders = apiVersionReaders;

            public string? Read( HttpRequest request )
            {
                var versions = new HashSet<string>( StringComparer.OrdinalIgnoreCase );

                foreach ( var apiVersionReader in apiVersionReaders )
                {
                    var version = apiVersionReader.Read( request );

                    if ( !IsNullOrEmpty( version ) )
                    {
                        versions.Add( version! );
                    }
                }

                return versions.EnsureZeroOrOneApiVersions();
            }

            public void AddParameters( IApiVersionParameterDescriptionContext context )
            {
                foreach ( var apiVersionReader in apiVersionReaders )
                {
                    apiVersionReader.AddParameters( context );
                }
            }
        }
    }
}