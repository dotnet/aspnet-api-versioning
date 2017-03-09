#if WEBAPI
namespace Microsoft.Web
#else
namespace Microsoft.AspNetCore.Mvc
#endif
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;

    static class HttpContentExtensions
    {
        internal static Task<T> ReadAsExampleAsync<T>( this HttpContent content, T example ) => content.ReadAsAsync<T>();
    }
}