#if WEBAPI
namespace Microsoft.Web.Http.Versioning
#else
namespace Microsoft.AspNetCore.Mvc.Versioning
#endif
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using static System.String;
#if WEBAPI
    using System.Net.Http.Headers;
#else
    using HttpResponseHeaders = Microsoft.AspNetCore.Http.IHeaderDictionary;
#endif

    sealed partial class DefaultApiVersionReporter : IReportApiVersions
    {
        const string ApiSupportedVersions = "api-supported-versions";
        const string ApiDeprecatedVersions = "api-deprecated-versions";
        const string ValueSeparator = ", ";

        public void Report( HttpResponseHeaders headers, Lazy<ApiVersionModel> apiVersionModel ) =>
            Report( headers, apiVersionModel.Value );

        public void Report( HttpResponseHeaders headers, ApiVersionModel apiVersionModel )
        {
            AddApiVersionHeader( headers, ApiSupportedVersions, apiVersionModel.SupportedApiVersions );
            AddApiVersionHeader( headers, ApiDeprecatedVersions, apiVersionModel.DeprecatedApiVersions );
        }

        static void AddApiVersionHeader( HttpResponseHeaders headers, string headerName, IReadOnlyList<ApiVersion> versions )
        {
            if ( versions.Count > 0 &&
#if WEBAPI
                !headers.Contains( headerName ) )
#else
                !headers.ContainsKey( headerName ) )
#endif
            {
                headers.Add( headerName, Join( ValueSeparator, versions.Select( v => v.ToString() ).ToArray() ) );
            }
        }
    }
}