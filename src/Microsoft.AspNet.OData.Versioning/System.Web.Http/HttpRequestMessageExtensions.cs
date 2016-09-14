namespace System.Web.Http
{
    using Diagnostics.Contracts;
    using Microsoft.OData.Core;
    using Microsoft.Web.Http;
    using Microsoft.Web.Http.Versioning;
    using System.Net.Http;
    using static System.Net.HttpStatusCode;

    internal static class HttpRequestMessageExtensions
    {
        internal static ApiVersion GetRequestedApiVersionOrReturnBadRequest( this HttpRequestMessage request )
        {
            Contract.Requires( request != null );

            try
            {
                return request.GetRequestedApiVersion();
            }
            catch ( AmbiguousApiVersionException ex )
            {
                var error = new ODataError() { ErrorCode = "AmbiguousApiVersion", Message = ex.Message };
                throw new HttpResponseException( request.CreateResponse( BadRequest, error ) );
            }
        }
    }
}