namespace Microsoft.Examples.Controllers
{
    using Microsoft.Web.Http;
    using Models;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.OData;
    using System.Web.OData.Query;
    using System.Web.OData.Routing;

    [ApiVersion( "3.0" )]
    [ControllerName( "People" )]
    [ODataRoutePrefix( "People" )]
    public class People2Controller : ODataController
    {
        // GET ~/people?api-version=3.0
        [ODataRoute]
        public IHttpActionResult Get( ODataQueryOptions<Person> options ) =>
            Ok( new[] { new Person() { Id = 1, FirstName = "Bill", LastName = "Mei", Email = "bill.mei@somewhere.com", Phone = "555-555-5555" } } );

        // GET ~/people({id})?api-version=3.0
        [ODataRoute( "({id})" )]
        public IHttpActionResult Get( [FromODataUri] int id, ODataQueryOptions<Person> options ) =>
            Ok( new Person() { Id = id, FirstName = "Bill", LastName = "Mei", Email = "bill.mei@somewhere.com", Phone = "555-555-5555" } );
    }
}