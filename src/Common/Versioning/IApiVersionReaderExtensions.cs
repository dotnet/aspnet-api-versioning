#if WEBAPI
namespace Microsoft.Web.Http.Versioning
#else
namespace Microsoft.AspNetCore.Mvc.Versioning
#endif
{
    using System;
    using static ApiVersionParameterLocation;

    internal static class IApiVersionReaderExtensions
    {
        internal static bool VersionsByMediaType( this IApiVersionReader reader )
        {
            var context = new DescriptionContext();
            reader.AddParameters( context );
            return context.HasMediaTypeApiVersion;
        }

        internal static string GetMediaTypeVersionParameter( this IApiVersionReader reader )
        {
            var context = new DescriptionContext();
            reader.AddParameters( context );
            return context.ParameterName;
        }

        sealed class DescriptionContext : IApiVersionParameterDescriptionContext
        {
            internal string ParameterName { get; private set; }

            internal bool HasMediaTypeApiVersion { get; private set; }

            public void AddParameter( string name, ApiVersionParameterLocation location )
            {
                if ( HasMediaTypeApiVersion |= location == MediaTypeParameter )
                {
                    ParameterName = name;
                }
            }
        }
    }
}