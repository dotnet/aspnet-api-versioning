﻿namespace Microsoft.Examples.V2.Controllers
{
    using Microsoft.Web.Http;
    using Microsoft.Web.Http.Description;
    using Models;
    using System.Collections.Generic;
    using System.Web.Http;
    using System.Web.Http.Description;
    using System.Web.OData;
    using System.Web.OData.Routing;

    /// <summary>
    /// Represents a RESTful people service.
    /// </summary>
    [ApiVersion( "2.0" )]
    [ODataRoutePrefix( "People" )]
    public class PeopleController : ODataController
    {
        /// <summary>
        /// Gets all people.
        /// </summary>
        /// <returns>All available people.</returns>
        /// <response code="200">The successfully retrieved people.</response>
        [HttpGet]
        [ODataRoute]
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
        /// <param name="id">The requested person identifier.</param>
        /// <returns>The requested person.</returns>
        /// <response code="200">The person was successfully retrieved.</response>
        /// <response code="404">The person does not exist.</response>
        [HttpGet]
        [ODataRoute( "({id})" )]
        [ResponseType( typeof( Person ) )]
        public IHttpActionResult Get( int id ) =>
            Ok( new Person()
                {
                    Id = id,
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "john.doe@somewhere.com"
                }
            );
    }
}