namespace Microsoft.Examples.V1.Controllers
{
    using Microsoft.Web.Http;
    using Models;
    using System.Web.Http;
    using System.Web.Http.Description;

    /// <summary>
    /// Represents a RESTful people service.
    /// </summary>
    [ApiVersion( "1.0" )]
    [ApiVersion( "0.9", Deprecated = true )]
    [RoutePrefix( "api/v{version:apiVersion}/people" )]
    public class PeopleController : ApiController
    {
        /// <summary>
        /// Gets a single person.
        /// </summary>
        /// <param name="id">The requested person identifier.</param>
        /// <returns>The requested person.</returns>
        /// <response code="200">The person was successfully retrieved.</response>
        /// <response code="404">The person does not exist.</response>
        [HttpGet]
        [Route( "{id:int}" )]
        [ResponseType( typeof( Person ) )]
        public IHttpActionResult Get( int id ) =>
            Ok( new Person()
                {
                    Id = id,
                    FirstName = "John",
                    LastName = "Doe"
                }
            );
    }
}