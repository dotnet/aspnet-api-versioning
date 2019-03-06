namespace Microsoft.Examples.V2
{
    using Microsoft.AspNet.OData;
    using Microsoft.AspNet.OData.Query;
    using Microsoft.Examples.Models;
    using Microsoft.OData;
    using Microsoft.Web.Http;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Http;
    using System.Web.Http.Description;
    using static Microsoft.AspNet.OData.Query.AllowedQueryOptions;

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
        [HttpGet]
        [ResponseType( typeof( ODataValue<IEnumerable<Person>> ) )]
        public IHttpActionResult Get( ODataQueryOptions<Person> options )
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

            return this.Success( options.ApplyTo( people.AsQueryable() ) );
        }

        /// <summary>
        /// Gets a single person.
        /// </summary>
        /// <param name="key">The requested person identifier.</param>
        /// <param name="options">The current OData query options.</param>
        /// <returns>The requested person.</returns>
        /// <response code="200">The person was successfully retrieved.</response>
        /// <response code="404">The person does not exist.</response>
        [HttpGet]
        [ResponseType( typeof( Person ) )]
        public IHttpActionResult Get( int key, ODataQueryOptions<Person> options )
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

            return this.SuccessOrNotFound( options.ApplyTo( people.AsQueryable() ).SingleOrDefault() );
        }

        /// <summary>
        /// Gets the new hires since the specified date.
        /// </summary>
        /// <param name="since">The date and time since people were hired.</param>
        /// <param name="options">The current OData query options.</param>
        /// <returns>The matching new hires.</returns>
        /// <response code="200">The people were successfully retrieved.</response>
        [HttpGet]
        [ResponseType( typeof( ODataValue<IEnumerable<Person>> ) )]
        public IHttpActionResult NewHires( DateTime since, ODataQueryOptions<Person> options ) => Get( options );

        /// <summary>
        /// Gets the home address of a person.
        /// </summary>
        /// <param name="key">The person identifier.</param>
        /// <returns>The person's home address.</returns>
        /// <response code="200">The home address was successfully retrieved.</response>
        /// <response code="404">The person does not exist.</response>
        [HttpGet]
        [ResponseType( typeof( Address ) )]
        public IHttpActionResult GetHomeAddress( int key ) =>
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