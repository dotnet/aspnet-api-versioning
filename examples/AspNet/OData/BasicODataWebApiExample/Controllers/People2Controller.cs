namespace ApiVersioning.Examples.Controllers;

using ApiVersioning.Examples.Models;
using Asp.Versioning;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Routing;
using System.Web.Http;

[ApiVersion( "3.0" )]
[ControllerName( "People" )]
[ODataRoutePrefix( "People" )]
public class People2Controller : ODataController
{
    // GET ~/api/people?api-version=3.0
    [ODataRoute]
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

    // GET ~/api/people/{id}?api-version=3.0
    [ODataRoute( "{id}" )]
    public IHttpActionResult Get( int id, ODataQueryOptions<Person> options ) =>
        Ok( new Person()
        {
            Id = id,
            FirstName = "Bill",
            LastName = "Mei",
            Email = "bill.mei@somewhere.com",
            Phone = "555-555-5555",
        } );
}