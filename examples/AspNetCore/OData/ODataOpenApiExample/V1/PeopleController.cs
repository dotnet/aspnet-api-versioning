namespace ApiVersioning.Examples.V1;

using ApiVersioning.Examples.Models;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using static Microsoft.AspNetCore.Http.StatusCodes;
using static Microsoft.AspNetCore.OData.Query.AllowedQueryOptions;

/// <summary>
/// Represents a RESTful people service.
/// </summary>
[ApiVersion( 1.0 )]
[ApiVersion( 0.9, Deprecated = true )]
public class PeopleController : ODataController
{
    /// <summary>
    /// Gets a single person.
    /// </summary>
    /// <param name="key">The requested person identifier.</param>
    /// <param name="options">The current OData query options.</param>
    /// <returns>The requested person.</returns>
    /// <response code="200">The person was successfully retrieved.</response>
    /// <response code="404">The person does not exist.</response>
    [HttpGet]
    [Produces( "application/json" )]
    [ProducesResponseType( typeof( Person ), Status200OK )]
    [ProducesResponseType( Status404NotFound )]
    public IActionResult Get( int key, ODataQueryOptions<Person> options )
    {
        var people = new Person[]
        {
            new()
            {
                Id = key,
                FirstName = "John",
                LastName = "Doe",
            },
        };

        var person = options.ApplyTo( people.AsQueryable() ).SingleOrDefault();

        if ( person == null )
        {
            return NotFound();
        }

        return Ok( person );
    }

    /// <summary>
    /// Gets the most expensive person.
    /// </summary>
    /// <returns>The most expensive person.</returns>
    /// <response code="200">The person was successfully retrieved.</response>
    /// <response code="404">No people exist.</response>
    [HttpGet]
    [MapToApiVersion( 1.0 )]
    [Produces( "application/json" )]
    [ProducesResponseType( typeof( Person ), Status200OK )]
    [ProducesResponseType( Status404NotFound )]
    [EnableQuery( AllowedQueryOptions = Select )]
    public SingleResult<Person> MostExpensive( ODataQueryOptions<Person> options, CancellationToken ct ) =>
            SingleResult.Create(
                new Person[]
                {
                new()
                {
                    Id = 42,
                    FirstName = "Elon",
                    LastName = "Musk",
                },
                }.AsQueryable() );

    /// <summary>
    /// Gets the most expensive person.
    /// </summary>
    /// <returns>The most expensive person.</returns>
    /// <response code="200">The person was successfully retrieved.</response>
    /// <response code="404">The person does not exist.</response>
    [HttpGet]
    [MapToApiVersion( 1.0 )]
    [Produces( "application/json" )]
    [ProducesResponseType( typeof( Order ), Status200OK )]
    [ProducesResponseType( Status404NotFound )]
    [EnableQuery( AllowedQueryOptions = Select )]
    public SingleResult<Person> MostExpensive(
        int key,
        ODataQueryOptions<Person> options,
        CancellationToken ct ) =>
        SingleResult.Create(
            new Person[]
            {
                new()
                {
                    Id = key,
                    FirstName = "John",
                    LastName = "Doe",
                },
            }.AsQueryable() );
}