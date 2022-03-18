namespace ApiVersioning.Examples.Controllers;

using ApiVersioning.Examples.Models;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;

[ControllerName( "People" )]
public class People2Controller : ODataController
{
    // GET ~/api/people?api-version=3.0
    [EnableQuery]
    public IActionResult Get( ODataQueryOptions<Person> options ) =>
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

    // GET ~/api/people/{key}?api-version=3.0
    [EnableQuery]
    public IActionResult Get( int key, ODataQueryOptions<Person> options ) =>
        Ok( new Person()
        {
            Id = key, 
            FirstName = "Bill", 
            LastName = "Mei", 
            Email = "bill.mei@somewhere.com", 
            Phone = "555-555-5555",
        } );
}