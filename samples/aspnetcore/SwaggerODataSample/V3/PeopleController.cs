namespace Microsoft.Examples.V3
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
    [ApiVersion( "3.0" )]
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
                    Phone = "555-987-1234",
                },
                new Person()
                {
                    Id = 2,
                    FirstName = "Bob",
                    LastName = "Smith",
                    Email = "bob.smith@somewhere.com",
                    Phone = "555-654-4321",
                },
                new Person()
                {
                    Id = 3,
                    FirstName = "Jane",
                    LastName = "Doe",
                    Email = "jane.doe@somewhere.com",
                    Phone = "555-789-3456",
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
                    Phone = "555-987-1234",
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
        /// Creates a new person.
        /// </summary>
        /// <param name="person">The person to create.</param>
        /// <returns>The created person.</returns>
        /// <response code="201">The person was successfully created.</response>
        /// <response code="400">The person was invalid.</response>
        [Produces( "application/json" )]
        [ProducesResponseType( typeof( Person ), Status201Created )]
        [ProducesResponseType( Status400BadRequest )]
        public IActionResult Post( [FromBody] Person person )
        {
            if ( !ModelState.IsValid )
            {
                return BadRequest( ModelState );
            }

            person.Id = 42;

            return Created( person );
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
        /// Promotes a person.
        /// </summary>
        /// <param name="key">The identifier of the person to promote.</param>
        /// <param name="parameters">The action parameters.</param>
        /// <returns>None</returns>
        /// <response code="204">The person was successfully promoted.</response>
        /// <response code="400">The parameters are invalid.</response>
        /// <response code="404">The person does not exist.</response>
        [HttpPost]
        [ProducesResponseType( Status204NoContent )]
        [ProducesResponseType( Status400BadRequest )]
        [ProducesResponseType( Status404NotFound )]
        public IActionResult Promote( int key, ODataActionParameters parameters )
        {
            if ( !ModelState.IsValid )
            {
                return BadRequest( ModelState );
            }

            var title = (string) parameters["title"];
            return NoContent();
        }

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

        /// <summary>
        /// Gets the work address of a person.
        /// </summary>
        /// <param name="key">The person identifier.</param>
        /// <returns>The person's work address.</returns>
        /// <response code="200">The work address was successfully retrieved.</response>
        /// <response code="404">The person does not exist.</response>
        [HttpGet]
        [Produces( "application/json" )]
        [ProducesResponseType( typeof( Address ), Status200OK )]
        [ProducesResponseType( Status404NotFound )]
        public IActionResult GetWorkAddress( int key ) =>
            Ok( new Address()
            {
                Id = 42,
                Street = "1 Microsoft Way",
                City = "Redmond",
                State = "WA",
                ZipCode = "98052"
            } );
    }
}