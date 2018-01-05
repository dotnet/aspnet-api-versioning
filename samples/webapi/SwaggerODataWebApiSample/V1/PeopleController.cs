namespace Microsoft.Examples.V1
{
    using Microsoft.Web.Http;
    using Models;
    using System.Web.Http;
    using System.Web.Http.Description;
    using System.Web.OData;

    /// <summary>
    /// Represents a RESTful people service.
    /// </summary>
    [ApiVersion( "1.0" )]
    [ApiVersion( "0.9", Deprecated = true )]
    public class PeopleController : ODataController
    {
        /// <summary>
        /// Gets a single person.
        /// </summary>
        /// <param name="key">The requested person identifier.</param>
        /// <returns>The requested person.</returns>
        /// <response code="200">The person was successfully retrieved.</response>
        /// <response code="404">The person does not exist.</response>
        [HttpGet]
        [ResponseType( typeof( Person ) )]
        public IHttpActionResult Get( int key ) =>
            Ok( new Person()
                {
                    Id = key,
                    FirstName = "John",
                    LastName = "Doe"
                }
            );
    }
}