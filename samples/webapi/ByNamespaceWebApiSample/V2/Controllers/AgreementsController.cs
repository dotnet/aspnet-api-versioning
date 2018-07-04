namespace Microsoft.Examples.V2.Controllers
{
    using Models;
    using System.Web.Http;

    public class AgreementsController : ApiController
    {
        // GET ~/v2/agreements/{accountId}
        // GET ~/agreements/{accountId}?api-version=2.0
        public IHttpActionResult Get( string accountId ) => Ok( new Agreement( GetType().FullName, accountId, Request.GetRequestedApiVersion().ToString() ) );
    }
}