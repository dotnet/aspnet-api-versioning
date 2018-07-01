namespace Microsoft.AspNet.OData.Basic.Controllers
{
    using Microsoft.AspNet.OData;
    using Microsoft.AspNet.OData.Models;
    using Microsoft.AspNet.OData.Query;
    using Microsoft.AspNet.OData.Routing;
    using Microsoft.Web.Http;
    using System.Web.Http;

    [ApiVersion( "1.0" )]
    [ApiVersion( "2.0" )]
    [ODataRoutePrefix( "People" )]
    public class PeopleController : ODataController
    {
        [ODataRoute]
        public IHttpActionResult Get( ODataQueryOptions<Person> options ) =>
            Ok( new[] { new Person() { Id = 1, FirstName = "Bill", LastName = "Mei", Email = "bill.mei@somewhere.com", Phone = "555-555-5555" } } );

        [ODataRoute( "({key})" )]
        public IHttpActionResult Get( [FromODataUri] int key, ODataQueryOptions<Person> options ) =>
            Ok( new Person() { Id = key, FirstName = "Bill", LastName = "Mei", Email = "bill.mei@somewhere.com", Phone = "555-555-5555" } );

        [MapToApiVersion( "2.0" )]
        [ODataRoute( "({key})" )]
        public IHttpActionResult Patch( [FromODataUri] int key, Delta<Person> delta, ODataQueryOptions<Person> options )
        {
            if ( !ModelState.IsValid )
            {
                return BadRequest( ModelState );
            }

            var person = new Person() { Id = key, FirstName = "Bill", LastName = "Mei", Email = "bill.mei@somewhere.com", Phone = "555-555-5555" };

            delta.Patch( person );

            return Updated( person );
        }
    }
}