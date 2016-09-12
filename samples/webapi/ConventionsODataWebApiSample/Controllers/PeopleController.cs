namespace Microsoft.Examples.Controllers
{
    using Microsoft.Web.Http;
    using Models;
    using System.Web.Http;
    using System.Web.OData;
    using System.Web.OData.Query;
    using System.Web.OData.Routing;

    [ODataRoutePrefix( "People" )]
    public class PeopleController : ODataController
    {
        // GET ~/v1/people
        // GET ~/people?api-version=[1.0|2.0]
        [ODataRoute]
        public IHttpActionResult Get( ODataQueryOptions<Person> options ) =>
            Ok( new[] { new Person() { Id = 1, FirstName = "Bill", LastName = "Mei", Email = "bill.mei@somewhere.com", Phone = "555-555-5555" } } );

        // GET ~/v1/people(1)
        // GET ~/people(1)?api-version=[1.0|2.0]
        [ODataRoute( "({id})" )]
        public IHttpActionResult Get( [FromODataUri] int id, ODataQueryOptions<Person> options ) =>
            Ok( new Person() { Id = id, FirstName = "Bill", LastName = "Mei", Email = "bill.mei@somewhere.com", Phone = "555-555-5555" } );

        // PATCH ~/people(1)?api-version=2.0
        [MapToApiVersion( "2.0" )]
        [ODataRoute( "({id})" )]
        public IHttpActionResult Patch( [FromODataUri] int id, Delta<Person> delta, ODataQueryOptions<Person> options )
        {
            if ( !ModelState.IsValid )
            {
                return BadRequest( ModelState );
            }

            var person = new Person() { Id = id, FirstName = "Bill", LastName = "Mei", Email = "bill.mei@somewhere.com", Phone = "555-555-5555" };

            delta.Patch( person );

            return Updated( person );
        }
    }
}