namespace ApiVersioning.Examples.V2.Controllers;

using ApiVersioning.Examples.V2.Models;
using Asp.Versioning;
using System.Web.Http;
using System.Web.Http.Description;

/// <summary>
/// Represents a RESTful people service.
/// </summary>
[ApiVersion( 2.0 )]
[RoutePrefix( "api/v{version:apiVersion}/people" )]
public class PeopleController : ApiController
{
    private const string ByIdRouteName = "GetPersonById" + nameof( V2 );

    /// <summary>
    /// Gets all people.
    /// </summary>
    /// <returns>All available people.</returns>
    /// <response code="200">The successfully retrieved people.</response>
    [HttpGet]
    [Route]
    [ResponseType( typeof( IEnumerable<Person> ) )]
    public IHttpActionResult Get()
    {
        var people = new Person[]
        {
            new()
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@somewhere.com",
            },
            new()
            {
                Id = 2,
                FirstName = "Bob",
                LastName = "Smith",
                Email = "bob.smith@somewhere.com",
            },
            new()
            {
                Id = 3,
                FirstName = "Jane",
                LastName = "Doe",
                Email = "jane.doe@somewhere.com",
            },
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
    [Route( "{id:int}", Name = ByIdRouteName )]
    [ResponseType( typeof( Person ) )]
    public IHttpActionResult Get( int id ) =>
        Ok( new Person()
        {
            Id = id,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@somewhere.com",
        } );
}