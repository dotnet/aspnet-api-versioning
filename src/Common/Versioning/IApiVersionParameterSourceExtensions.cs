#if WEBAPI
namespace Microsoft.Web.Http.Versioning
#else
namespace Microsoft.AspNetCore.Mvc.Versioning
#endif
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using static ApiVersionParameterLocation;

    internal static class IApiVersionParameterSourceExtensions
    {
        internal static bool VersionsByUrlSegment( this IApiVersionParameterSource source )
        {
            var context = new UrlSegmentDescriptionContext();
            source.AddParameters( context );
            return context.HasPathApiVersion;
        }

        internal static bool VersionsOnlyByUrlSegment( this IApiVersionParameterSource source )
        {
            var context = new UrlSegmentDescriptionContext();
            source.AddParameters( context );
            return context.HasPathApiVersion && context.Locations == 1;
        }

        internal static bool VersionsByMediaType( this IApiVersionParameterSource source )
        {
            var context = new MediaTypeDescriptionContext();
            source.AddParameters( context );
            return context.HasMediaTypeApiVersion;
        }

        internal static string GetMediaTypeVersionParameter( this IApiVersionParameterSource source )
        {
            var context = new MediaTypeDescriptionContext();
            source.AddParameters( context );
            return context.ParameterName;
        }

        sealed class UrlSegmentDescriptionContext : IApiVersionParameterDescriptionContext
        {
            internal bool HasPathApiVersion { get; private set; }

            internal int Locations { get; private set; }

            public void AddParameter( string name, ApiVersionParameterLocation location )
            {
                HasPathApiVersion |= location == Path;
                ++Locations;
            }
        }

        sealed class MediaTypeDescriptionContext : IApiVersionParameterDescriptionContext
        {
            readonly StringComparer comparer = StringComparer.OrdinalIgnoreCase;
            readonly List<string> parameterNames = new();

            internal string ParameterName => parameterNames.Count == 0 ? string.Empty : parameterNames[0];

            internal IReadOnlyList<string> ParameterNames => parameterNames;

            internal bool HasMediaTypeApiVersion { get; private set; }

            public void AddParameter( string name, ApiVersionParameterLocation location )
            {
                var mediaTypeParameter = location == MediaTypeParameter && !string.IsNullOrEmpty( name );

                HasMediaTypeApiVersion |= mediaTypeParameter;

                if ( mediaTypeParameter && !parameterNames.Contains( name, comparer ) )
                {
                    parameterNames.Add( name );
                }
            }
        }
    }
}