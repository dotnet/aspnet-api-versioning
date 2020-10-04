namespace Microsoft.AspNet.OData.Conventions.Controllers
{
    using Microsoft.AspNet.OData;
    using Microsoft.AspNet.OData.Models;
    using System.Web.Http;
    using static System.Net.HttpStatusCode;

    public class CustomersController : ODataController
    {
        public IHttpActionResult Get() => Ok();

        public IHttpActionResult Get( int key ) => Ok();

        public IHttpActionResult Post( Customer customer )
        {
            customer.Id = 42;
            return Created( customer );
        }

        public IHttpActionResult Put( int key, [FromBody] Customer customer ) => StatusCode( NoContent );

        public IHttpActionResult Delete( int key ) => StatusCode( NoContent );
    }
}