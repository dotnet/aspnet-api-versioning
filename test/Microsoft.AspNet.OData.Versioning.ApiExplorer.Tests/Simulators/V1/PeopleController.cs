namespace Microsoft.Web.Http.Simulators.V1
{
    using Microsoft.Web.Http;
    using Microsoft.Web.Http.Description;
    using Models;
    using System.Web.Http;
    using System.Web.Http.Description;
    using System.Web.OData;
    using System.Web.OData.Routing;

    [ApiVersion( "0.9" )]
    [ApiVersion( "1.0" )]
    [ODataRoutePrefix( "People" )]
    public class PeopleController : ODataController
    {
        [HttpGet]
        [ODataRoute( "({id})" )]
        [ResponseType( typeof( ODataValue<Person> ) )]
        public IHttpActionResult Get( [FromODataUri] int id ) => Ok( new Person() { Id = id } );
    }
}