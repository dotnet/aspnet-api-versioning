namespace Microsoft.AspNetCore.OData.Basic.Controllers.Endpoint
{
    using Microsoft.AspNet.OData;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.OData.Models;

    [Route( "api/[controller]" )]
    public class CustomersController : ODataController
    {
        [HttpGet]
        [ApiVersion( "2.0" )]
        [ApiVersion( "3.0" )]
        public IActionResult Get() => Ok();

        [HttpGet( "{key}" )]
        [ApiVersion( "1.0" )]
        [ApiVersion( "2.0" )]
        [ApiVersion( "3.0" )]
        public IActionResult Get( int key ) => Ok();

        [HttpPost]
        [ApiVersion( "1.0" )]
        [ApiVersion( "2.0" )]
        [ApiVersion( "3.0" )]
        public IActionResult Post( [FromBody] Customer customer )
        {
            customer.Id = 42;
            return Created( customer );
        }

        [HttpPut( "{key}" )]
        [ApiVersion( "3.0" )]
        public IActionResult Put( int key, [FromBody] Customer customer ) => NoContent();

        [HttpDelete( "{key}" )]
        [ApiVersionNeutral]
        public IActionResult Delete( int key ) => NoContent();
    }
}