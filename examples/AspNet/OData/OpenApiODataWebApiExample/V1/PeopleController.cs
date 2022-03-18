namespace ApiVersioning.Examples.V1;

using ApiVersioning.Examples.Models;
using Asp.Versioning;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Routing;
using System.Web.Http;
using System.Web.Http.Description;
using static Microsoft.AspNet.OData.Query.AllowedQueryOptions;

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
    [ResponseType( typeof( Person ) )]
    public IHttpActionResult Get( int key, ODataQueryOptions<Person> options )
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

        return this.SuccessOrNotFound( options.ApplyTo( people.AsQueryable() ).SingleOrDefault() );
    }

    /// <summary>
    /// Gets the most expensive person.
    /// </summary>
    /// <returns>The most expensive person.</returns>
    /// <response code="200">The person was successfully retrieved.</response>
    /// <response code="404">No people exist.</response>
    [HttpGet]
    [MapToApiVersion( "1.0" )]
    [ResponseType( typeof( Person ) )]
    [EnableQuery( AllowedQueryOptions = Select )]
    public SingleResult<Person> MostExpensive( ODataQueryOptions<Person> options, CancellationToken ct ) => 
        SingleResult.Create( new[] { new Person() { Id = 42, FirstName = "Elon", LastName = "Musk" } }.AsQueryable() );

    /// <summary>
    /// Gets the most expensive person.
    /// </summary>
    /// <returns>The most expensive person.</returns>
    /// <response code="200">The person was successfully retrieved.</response>
    /// <response code="404">The person does not exist.</response>
    [HttpGet]
    [MapToApiVersion( "1.0" )]
    [ResponseType( typeof( Person ) )]
    [EnableQuery( AllowedQueryOptions = Select )]
    public SingleResult<Person> MostExpensive( int key, ODataPath path, ODataQueryOptions<Person> options, CancellationToken ct ) =>
        SingleResult.Create( new[] { new Person() { Id = key, FirstName = "John", LastName = "Doe" } }.AsQueryable() );
}