namespace Microsoft.Examples.V1.Controllers
{
    using Microsoft.Web.Http;
    using Models;
    using System.Web.Http;

    public class AgreementsController : ApiController
    {
        // GET ~/v1/agreements/{accountId}
        // GET ~/agreements/{accountId}?api-version=1.0
        public IHttpActionResult Get( string accountId, ApiVersion apiVersion ) => Ok( new Agreement( GetType().FullName, accountId, apiVersion.ToString() ) );
    }
}