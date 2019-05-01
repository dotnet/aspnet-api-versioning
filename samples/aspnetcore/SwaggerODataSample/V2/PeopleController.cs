namespace Microsoft.Examples.V2
{
    using Microsoft.AspNet.OData;
    using Microsoft.AspNet.OData.Query;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Examples.Models;
    using Microsoft.OData;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using static Microsoft.AspNet.OData.Query.AllowedQueryOptions;
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
        /// <param name="options">The current OData query options.</param>
        /// <returns>All available people.</returns>
        /// <response code="200">The successfully retrieved people.</response>
        [Produces( "application/json" )]
        [ProducesResponseType( typeof( ODataValue<IEnumerable<Person>> ), Status200OK )]
        public IActionResult Get( ODataQueryOptions<Person> options )
        {
            var validationSettings = new ODataValidationSettings()
            {
                AllowedQueryOptions = Select | OrderBy | Top | Skip | Count,
                AllowedOrderByProperties = { "firstName", "lastName" },
                AllowedArithmeticOperators = AllowedArithmeticOperators.None,
                AllowedFunctions = AllowedFunctions.None,
                AllowedLogicalOperators = AllowedLogicalOperators.None,
                MaxOrderByNodeCount = 2,
                MaxTop = 100,
            };

            try
            {
                options.Validate( validationSettings );
            }
            catch ( ODataException )
            {
                return BadRequest();
            }

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

            return Ok( options.ApplyTo( people.AsQueryable() ) );
        }

        /// <summary>
        /// Gets a single person.
        /// </summary>
        /// <param name="key">The requested person identifier.</param>
        /// <param name="options">The current OData query options.</param>
        /// <returns>The requested person.</returns>
        /// <response code="200">The person was successfully retrieved.</response>
        /// <response code="404">The person does not exist.</response>
        [Produces( "application/json" )]
        [ProducesResponseType( typeof( Person ), Status200OK )]
        [ProducesResponseType( Status404NotFound )]
        public IActionResult Get( int key, ODataQueryOptions<Person> options )
        {
            var people = new[]
            {
                new Person()
                {
                    Id = key,
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "john.doe@somewhere.com",
                }
            };

            var person = options.ApplyTo( people.AsQueryable() ).SingleOrDefault();

            if ( person == null )
            {
                return NotFound();
            }

            return Ok( person );
        }

        /// <summary>
        /// Gets the new hires since the specified date.
        /// </summary>
        /// <param name="since">The date and time since people were hired.</param>
        /// <param name="options">The current OData query options.</param>
        /// <returns>The matching new hires.</returns>
        /// <response code="200">The people were successfully retrieved.</response>
        [HttpGet]
        [Produces( "application/json" )]
        [ProducesResponseType( typeof( ODataValue<IEnumerable<Person>> ), Status200OK )]
        public IActionResult NewHires( DateTime since, ODataQueryOptions<Person> options ) => Get( options );

        /// <summary>
        /// Gets the home address of a person.
        /// </summary>
        /// <param name="key">The person identifier.</param>
        /// <returns>The person's home address.</returns>
        /// <response code="200">The home address was successfully retrieved.</response>
        /// <response code="404">The person does not exist.</response>
        [HttpGet]
        [Produces( "application/json" )]
        [ProducesResponseType( typeof( Address ), Status200OK )]
        [ProducesResponseType( Status404NotFound )]
        public IActionResult GetHomeAddress( int key ) =>
            Ok( new Address()
            {
                Id = 42,
                Street = "123 Some Place",
                City = "Seattle",
                State = "WA",
                ZipCode = "98101"
            } );
    }
}