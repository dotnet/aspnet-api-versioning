namespace Microsoft.AspNet.OData.Conventions.Controllers
{
    using Microsoft.AspNet.OData;
    using Microsoft.AspNet.OData.Models;
    using Microsoft.AspNet.OData.Routing;
    using System.Web.Http;
    using static System.Net.HttpStatusCode;

    [ODataRoutePrefix( "Customers" )]
    public class CustomersController : ODataController
    {
        [ODataRoute]
        public IHttpActionResult Get() => Ok();

        [ODataRoute( "({key})" )]
        public IHttpActionResult Get( [FromODataUri] int key ) => Ok();

        [ODataRoute]
        public IHttpActionResult Post( [FromBody] Customer customer )
        {
            customer.Id = 42;
            return Created( customer );
        }

        [ODataRoute( "({key})" )]
        public IHttpActionResult Put( [FromODataUri] int key, [FromBody] Customer customer ) => StatusCode( NoContent );

        [ODataRoute( "({key})" )]
        public IHttpActionResult Delete( [FromODataUri] int key ) => StatusCode( NoContent );
    }
}