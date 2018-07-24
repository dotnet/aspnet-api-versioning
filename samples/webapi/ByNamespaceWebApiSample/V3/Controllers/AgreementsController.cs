namespace Microsoft.Examples.V3.Controllers
{
    using Microsoft.Web.Http;
    using Models;
    using System.Web.Http;

    public class AgreementsController : ApiController
    {
        // GET ~/v3/agreements/{accountId}
        // GET ~/agreements/{accountId}?api-version=3.0
        public IHttpActionResult Get( string accountId, ApiVersion apiVersion ) => Ok( new Agreement( GetType().FullName, accountId, apiVersion.ToString() ) );
    }
}