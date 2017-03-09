#if WEBAPI
namespace Microsoft.Web
#else
namespace Microsoft.AspNetCore.Mvc
#endif
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;

    static class TaskExtensions
    {
        internal static async Task<HttpResponseMessage> EnsureSuccessStatusCode( this Task<HttpResponseMessage> task )
        {
            var response = await task;
            response.EnsureSuccessStatusCode();
            return response;
        }
    }
}