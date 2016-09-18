namespace Microsoft.Web.Http.ByNamespace.Controllers.V3
{
    using Microsoft.Web.Http;
    using Models;
    using System.Web.Http;

    [ApiVersion( "3.0" )]
    public class AgreementsController : ApiController
    {
        public IHttpActionResult Get( string accountId ) => Ok( new Agreement( GetType().FullName, accountId, Request.GetRequestedApiVersion().ToString() ) );
    }
}