namespace Microsoft.AspNetCore.OData.Basic.Controllers.Endpoint
{
    using Microsoft.AspNet.OData;
    using Microsoft.AspNet.OData.Query;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.OData.Models;

    [ApiVersion( "3.0" )]
    [ControllerName( "People" )]
    [Route( "api/people" )]
    public class People2Controller : ODataController
    {
        [HttpGet]
        public IActionResult Get( ODataQueryOptions<Person> options ) =>
            Ok( new[] { new Person() { Id = 1, FirstName = "Bill", LastName = "Mei", Email = "bill.mei@somewhere.com", Phone = "555-555-5555" } } );

        [HttpGet( "{key}" )]
        public IActionResult Get( int key, ODataQueryOptions<Person> options ) =>
            Ok( new Person() { Id = key, FirstName = "Bill", LastName = "Mei", Email = "bill.mei@somewhere.com", Phone = "555-555-5555" } );
    }
}