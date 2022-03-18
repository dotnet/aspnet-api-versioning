namespace ApiVersioning.Examples.Controllers;

using ApiVersioning.Examples.Models;
using Asp.Versioning;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Query;
using System.Web.Http;

public class PeopleController : ODataController
{
    // GET ~/api/people?api-version=[1.0|2.0]
    public IHttpActionResult Get( ODataQueryOptions<Person> options ) =>
        Ok( new Person[]
        {
            new()
            {
                Id = 1,
                FirstName = "Bill",
                LastName = "Mei",
                Email = "bill.mei@somewhere.com",
                Phone = "555-555-5555",
            },
        } );

    // GET ~/api/people/{key}?api-version=[1.0|2.0]
    public IHttpActionResult Get( int key, ODataQueryOptions<Person> options ) =>
        Ok( new Person()
        {
            Id = key,
            FirstName = "Bill",
            LastName = "Mei",
            Email = "bill.mei@somewhere.com",
            Phone = "555-555-5555",
        } );

    // PATCH ~/api/people/{key}?api-version=2.0
    [MapToApiVersion( 2.0 )]
    public IHttpActionResult Patch( int key, Delta<Person> delta, ODataQueryOptions<Person> options )
    {
        if ( !ModelState.IsValid )
        {
            return BadRequest( ModelState );
        }

        var person = new Person()
        {
            Id = key,
            FirstName = "Bill",
            LastName = "Mei",
            Email = "bill.mei@somewhere.com",
            Phone = "555-555-5555",
        };

        delta.Patch( person );

        return Updated( person );
    }
}