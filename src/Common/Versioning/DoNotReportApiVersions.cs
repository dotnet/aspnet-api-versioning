#if WEBAPI
namespace Microsoft.Web.Http.Versioning
#else
namespace Microsoft.AspNetCore.Mvc.Versioning
#endif
{
    using System;
#if WEBAPI
    using System.Net.Http.Headers;
#else
    using HttpResponseHeaders = Microsoft.AspNetCore.Http.IHeaderDictionary;
#endif

    sealed partial class DoNotReportApiVersions : IReportApiVersions
    {
        public void Report( HttpResponseHeaders headers, ApiVersionModel apiVersionModel ) { }

        public void Report( HttpResponseHeaders headers, Lazy<ApiVersionModel> apiVersionModel ) { }
    }
}