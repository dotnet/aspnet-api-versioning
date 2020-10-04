namespace System.Web.Http
{
    using Microsoft.OData;
    using Microsoft.Web.Http;
    using Microsoft.Web.Http.Versioning;
    using System.Net.Http;
    using static System.Net.HttpStatusCode;

    static class HttpRequestMessageExtensions
    {
        internal static ApiVersion? GetRequestedApiVersionOrReturnBadRequest( this HttpRequestMessage request )
        {
            var properties = request.ApiVersionProperties();

            try
            {
                return properties.RequestedApiVersion;
            }
            catch ( AmbiguousApiVersionException ex )
            {
                var error = new ODataError() { ErrorCode = "AmbiguousApiVersion", Message = ex.Message };
                throw new HttpResponseException( request.CreateResponse( BadRequest, error ) );
            }
        }
    }
}