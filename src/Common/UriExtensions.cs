#if WEBAPI
namespace Microsoft.Web.Http
#else
namespace Microsoft.AspNetCore.Mvc
#endif
{
    using System;
    using static System.String;

    static class UriExtensions
    {
        internal static string SafeFullPath(this Uri uri)
        {
            var safeUrl = IsNullOrWhiteSpace( uri.Query ) ? uri.AbsoluteUri : uri.AbsoluteUri.Replace( uri.Query, Empty );
            return safeUrl;
        }
    }
}