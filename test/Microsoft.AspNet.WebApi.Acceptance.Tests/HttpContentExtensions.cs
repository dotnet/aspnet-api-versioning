namespace Microsoft.Web
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;

    internal static class HttpContentExtensions
    {
        internal static Task<T> ReadAsExampleAsync<T>( this HttpContent content, T example ) => content.ReadAsAsync<T>();
    }
}
