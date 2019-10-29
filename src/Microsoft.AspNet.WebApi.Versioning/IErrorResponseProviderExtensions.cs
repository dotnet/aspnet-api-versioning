namespace Microsoft.Web.Http
{
    using Microsoft.Web.Http.Versioning;
    using System.Net;
    using System.Net.Http;

    static class IErrorResponseProviderExtensions
    {
        internal static HttpResponseMessage BadRequest( this IErrorResponseProvider responseProvider, HttpRequestMessage request, string code, string message, string? messageDetail = null ) =>
            responseProvider.CreateResponse( new ErrorResponseContext( request, HttpStatusCode.BadRequest, code, message, messageDetail ) );

        internal static HttpResponseMessage MethodNotAllowed( this IErrorResponseProvider responseProvider, HttpRequestMessage request, string code, string message, string? messageDetail = null ) =>
            responseProvider.CreateResponse( new ErrorResponseContext( request, HttpStatusCode.MethodNotAllowed, code, message, messageDetail ) );
    }
}