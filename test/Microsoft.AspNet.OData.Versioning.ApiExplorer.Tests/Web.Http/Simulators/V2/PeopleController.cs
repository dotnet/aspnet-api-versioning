namespace Microsoft.Web.Http.Simulators.V2
{
    using Microsoft.AspNet.OData;
    using Microsoft.AspNet.OData.Routing;
    using Microsoft.Web.Http;
    using Microsoft.Web.Http.Description;
    using Microsoft.Web.Http.Simulators.Models;
    using System.Collections.Generic;
    using System.Web.Http;
    using System.Web.Http.Description;
    using static System.Linq.Enumerable;

    [ApiVersion( "2.0" )]
    [ODataRoutePrefix( "People" )]
    public class PeopleController : ODataController
    {
        [HttpGet]
        [ODataRoute]
        [ResponseType( typeof( ODataValue<IEnumerable<Person>> ) )]
        public IHttpActionResult Get() => Ok( Empty<Person>() );

        [HttpGet]
        [ODataRoute( "({id})" )]
        [ResponseType( typeof( ODataValue<Person> ) )]
        public IHttpActionResult Get( [FromODataUri] int id ) => Ok( new Person() { Id = id } );
    }
}