#if WEBAPI
namespace Microsoft.Web.Http
#else
namespace Microsoft.AspNetCore.Mvc
#endif
{
    using System;

    static class UriExtensions
    {
        internal static string SafeFullPath( this Uri uri ) => uri.GetLeftPart( UriPartial.Path );
    }
}