namespace Microsoft.Web.Http.Simulators.V3
{
    using Microsoft.Web.Http;
    using Microsoft.Web.Http.Description;
    using Models;
    using System.Collections.Generic;
    using System.Web.Http;
    using System.Web.Http.Description;
    using System.Web.OData;
    using System.Web.OData.Routing;
    using static System.Linq.Enumerable;
    using static System.Net.HttpStatusCode;

    [ApiVersion( "3.0" )]
    [AdvertiseApiVersions( "4.0" )]
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

        [HttpPost]
        [ODataRoute]
        [ResponseType( typeof( ODataValue<Person> ) )]
        public IHttpActionResult Post( [FromBody] Person person )
        {
            person.Id = 42;
            return Created( person );
        }

        [HttpDelete]
        [ODataRoute( "({id})" )]
        public IHttpActionResult Delete( [FromODataUri] int id ) => StatusCode( NoContent );
    }
}