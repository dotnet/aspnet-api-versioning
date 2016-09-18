namespace Microsoft.Web.Http.ByNamespace.Controllers.V1
{
    using Microsoft.Web.Http;
    using Models;
    using System.Web.Http;

    [ApiVersion( "1.0" )]
    public class AgreementsController : ApiController
    {
        public IHttpActionResult Get( string accountId ) => Ok( new Agreement( GetType().FullName, accountId, Request.GetRequestedApiVersion().ToString() ) );
    }
}