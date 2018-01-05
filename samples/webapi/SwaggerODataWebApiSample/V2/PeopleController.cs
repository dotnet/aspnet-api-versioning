namespace Microsoft.Examples.V2
{
    using Microsoft.Web.Http;
    using Microsoft.Web.Http.Description;
    using Models;
    using System;
    using System.Collections.Generic;
    using System.Web.Http;
    using System.Web.Http.Description;
    using System.Web.OData;

    /// <summary>
    /// Represents a RESTful people service.
    /// </summary>
    [ApiVersion( "2.0" )]
    public class PeopleController : ODataController
    {
        /// <summary>
        /// Gets all people.
        /// </summary>
        /// <returns>All available people.</returns>
        /// <response code="200">The successfully retrieved people.</response>
        [HttpGet]
        [ResponseType( typeof( ODataValue<IEnumerable<Person>> ) )]
        public IHttpActionResult Get()
        {
            var people = new[]
            {
                new Person()
                {
                    Id = 1,
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "john.doe@somewhere.com"
                },
                new Person()
                {
                    Id = 2,
                    FirstName = "Bob",
                    LastName = "Smith",
                    Email = "bob.smith@somewhere.com"
                },
                new Person()
                {
                    Id = 3,
                    FirstName = "Jane",
                    LastName = "Doe",
                    Email = "jane.doe@somewhere.com"
                }
            };

            return Ok( people );
        }

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
                LastName = "Doe",
                Email = "john.doe@somewhere.com"
            }
            );

        /// <summary>
        /// Gets the new hires since the specified date.
        /// </summary>
        /// <param name="since">The date and time since people were hired.</param>
        /// <returns>The matching new hires.</returns>
        /// <response code="200">The people were successfully retrieved.</response>
        [HttpGet]
        [ResponseType( typeof( ODataValue<IEnumerable<Order>> ) )]
        public IHttpActionResult NewHires( DateTime since ) => Get();
    }
}