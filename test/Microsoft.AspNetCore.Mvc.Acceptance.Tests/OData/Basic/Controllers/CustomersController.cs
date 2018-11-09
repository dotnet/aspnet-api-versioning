namespace Microsoft.AspNetCore.OData.Basic.Controllers
{
    using Microsoft.AspNet.OData;
    using Microsoft.AspNet.OData.Routing;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.OData.Models;

    [ODataRoutePrefix( "Customers" )]
    public class CustomersController : ODataController
    {
        [ODataRoute]
        [ApiVersion( "2.0" )]
        [ApiVersion( "3.0" )]
        public IActionResult Get() => Ok();

        [ODataRoute( "({key})" )]
        [ApiVersion( "1.0" )]
        [ApiVersion( "2.0" )]
        [ApiVersion( "3.0" )]
        public IActionResult Get( [FromODataUri] int key ) => Ok();

        [ODataRoute]
        [ApiVersion( "1.0" )]
        [ApiVersion( "2.0" )]
        [ApiVersion( "3.0" )]
        public IActionResult Post( [FromBody] Customer customer )
        {
            customer.Id = 42;
            return Created( customer );
        }

        [ODataRoute( "({key})" )]
        [ApiVersion( "3.0" )]
        public IActionResult Put( [FromODataUri] int key, [FromBody] Customer customer ) => NoContent();

        [ODataRoute( "({key})" )]
        [ApiVersionNeutral]
        public IActionResult Delete( [FromODataUri] int key ) => NoContent();
    }
}