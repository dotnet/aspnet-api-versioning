namespace Microsoft.AspNetCore.OData.Conventions.Controllers
{
    using Microsoft.AspNet.OData;
    using Microsoft.AspNet.OData.Routing;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.OData.Models;

    [ODataRoutePrefix( "Customers" )]
    public class CustomersController : ODataController
    {
        [ODataRoute]
        public IActionResult Get() => Ok();

        [ODataRoute( "({key})" )]
        public IActionResult Get( [FromODataUri] int key ) => Ok();

        [ODataRoute]
        public IActionResult Post( [FromBody] Customer customer )
        {
            customer.Id = 42;
            return Created( customer );
        }

        [ODataRoute( "({key})" )]
        public IActionResult Put( [FromODataUri] int key, [FromBody] Customer customer ) => NoContent();

        [ODataRoute( "({key})" )]
        public IActionResult Delete( [FromODataUri] int key ) => NoContent();
    }
}