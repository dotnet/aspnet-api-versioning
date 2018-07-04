namespace Microsoft.Examples.V3.Controllers
{
    using Models;
    using System.Web.Http;

    public class AgreementsController : ApiController
    {
        // GET ~/v3/agreements/{accountId}
        // GET ~/agreements/{accountId}?api-version=3.0
        public IHttpActionResult Get( string accountId ) => Ok( new Agreement( GetType().FullName, accountId, Request.GetRequestedApiVersion().ToString() ) );
    }
}