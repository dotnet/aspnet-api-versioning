namespace Microsoft.AspNet.OData.Simulators.V2
{
    using Microsoft.AspNet.OData;
    using Microsoft.AspNet.OData.Simulators.Models;
    using Microsoft.AspNetCore.Mvc;
    using System;
    using System.Collections.Generic;
    using static Microsoft.AspNetCore.Http.StatusCodes;

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
        [Produces( "application/json" )]
        [ProducesResponseType( typeof( ODataValue<IEnumerable<Person>> ), Status200OK )]
        public IActionResult Get()
        {
            var people = new[]
            {
                new Person()
                {
                    Id = 1,
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "john.doe@somewhere.com",
                },
                new Person()
                {
                    Id = 2,
                    FirstName = "Bob",
                    LastName = "Smith",
                    Email = "bob.smith@somewhere.com",
                },
                new Person()
                {
                    Id = 3,
                    FirstName = "Jane",
                    LastName = "Doe",
                    Email = "jane.doe@somewhere.com",
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
        [Produces( "application/json" )]
        [ProducesResponseType( typeof( Person ), Status200OK )]
        [ProducesResponseType( Status404NotFound )]
        public IActionResult Get( int key ) =>
            Ok( new Person()
            {
                Id = key,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@somewhere.com",
            } );

        /// <summary>
        /// Gets the new hires since the specified date.
        /// </summary>
        /// <param name="since">The date and time since people were hired.</param>
        /// <returns>The matching new hires.</returns>
        /// <response code="200">The people were successfully retrieved.</response>
        [HttpGet]
        [Produces( "application/json" )]
        [ProducesResponseType( typeof( ODataValue<IEnumerable<Order>> ), Status200OK )]
        public IActionResult NewHires( DateTime since ) => Get();
    }
}