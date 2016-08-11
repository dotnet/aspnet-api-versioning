namespace Microsoft.Examples.V1.Controllers
{
    using Microsoft.Web.Http;
    using Models;
    using System;
    using System.Web.Http;

    [ApiVersion( "1.0" )]
    public class AgreementsController : ApiController
    {
        // GET ~/v1/agreements
        // GET ~/agreements?api-version=1.0
        public IHttpActionResult Get( string accountId ) => Ok( new Agreement( GetType().FullName, accountId, Request.GetRequestedApiVersion().ToString() ) );
    }
}