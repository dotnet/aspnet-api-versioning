namespace Microsoft.Examples.Controllers
{
    using Microsoft.AspNet.OData;
    using Microsoft.AspNet.OData.Query;
    using Microsoft.AspNet.OData.Routing;
    using Microsoft.AspNetCore.Mvc;
    using Models;

    [ApiVersion( "3.0" )]
    [ControllerName( "People" )]
    [ODataRoutePrefix( "People" )]
    public class People2Controller : ODataController
    {
        // GET ~/v3/people
        // GET ~/api/people?api-version=3.0
        [ODataRoute]
        public IActionResult Get( ODataQueryOptions<Person> options ) =>
            Ok( new[] { new Person() { Id = 1, FirstName = "Bill", LastName = "Mei", Email = "bill.mei@somewhere.com", Phone = "555-555-5555" } } );

        // GET ~/v3/people(1)
        // GET ~/api/people(1)?api-version=3.0
        [ODataRoute( "({id})" )]
        public IActionResult Get( [FromODataUri] int id, ODataQueryOptions<Person> options ) =>
            Ok( new Person() { Id = id, FirstName = "Bill", LastName = "Mei", Email = "bill.mei@somewhere.com", Phone = "555-555-5555" } );
    }
}