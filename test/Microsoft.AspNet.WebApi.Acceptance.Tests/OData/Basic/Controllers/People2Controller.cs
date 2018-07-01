namespace Microsoft.AspNet.OData.Basic.Controllers
{
    using Microsoft.AspNet.OData;
    using Microsoft.AspNet.OData.Models;
    using Microsoft.AspNet.OData.Query;
    using Microsoft.AspNet.OData.Routing;
    using Microsoft.Web.Http;
    using System.Web.Http;

    [ApiVersion( "3.0" )]
    [ControllerName( "People" )]
    [ODataRoutePrefix( "People" )]
    public class People2Controller : ODataController
    {
        [ODataRoute]
        public IHttpActionResult Get( ODataQueryOptions<Person> options ) =>
            Ok( new[] { new Person() { Id = 1, FirstName = "Bill", LastName = "Mei", Email = "bill.mei@somewhere.com", Phone = "555-555-5555" } } );

        [ODataRoute( "({key})" )]
        public IHttpActionResult Get( [FromODataUri] int key, ODataQueryOptions<Person> options ) =>
            Ok( new Person() { Id = key, FirstName = "Bill", LastName = "Mei", Email = "bill.mei@somewhere.com", Phone = "555-555-5555" } );
    }
}