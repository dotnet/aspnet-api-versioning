﻿namespace ApiVersioning.Examples.Controllers;

using ApiVersioning.Examples.Models;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;

[ApiVersion( 1.0 )]
[ApiVersion( 2.0 )]
public class PeopleController : ODataController
{
    // GET ~/api/people?api-version=[1.0|2.0]
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
            new() {
                Id = 2,
                FirstName = "Xavier",
                LastName = "John",
                Email = "xavier@heaven.com",
                Phone = "666-666-6666",
            }
        } );

    // GET ~/api/people/{key}?api-version=[1.0|2.0]
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

    // PATCH ~/api/people/{key}?api-version=2.0
    [MapToApiVersion( 2.0 )]
    public IActionResult Patch( int key, Delta<Person> delta, ODataQueryOptions<Person> options )
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