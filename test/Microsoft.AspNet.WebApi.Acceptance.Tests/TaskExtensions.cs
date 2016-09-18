namespace Microsoft.Web
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;

    internal static class TaskExtensions
    {
        internal static async Task<HttpResponseMessage> EnsureSuccessStatusCode( this Task<HttpResponseMessage> task )
        {
            var response = await task;
            response.EnsureSuccessStatusCode();
            return response;
        }
    }
}