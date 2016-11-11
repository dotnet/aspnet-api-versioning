namespace Microsoft.Examples.V3.Controllers
{
    using Microsoft.Web.Http;
    using Models;
    using System;
    using System.Web.Http;

    [ApiVersion( "3.0" )]
    public class AgreementsController : ApiController
    {
        // GET ~/v3/agreements/{accountId}
        // GET ~/agreements/{accountId}?api-version=3.0
        public IHttpActionResult Get( string accountId ) => Ok( new Agreement( GetType().FullName, accountId, Request.GetRequestedApiVersion().ToString() ) );
    }
}