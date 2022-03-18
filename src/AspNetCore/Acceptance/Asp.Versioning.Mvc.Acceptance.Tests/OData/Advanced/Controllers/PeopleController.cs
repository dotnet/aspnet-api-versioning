// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0060 // Remove unused parameter

namespace Asp.Versioning.OData.Advanced.Controllers;

using Asp.Versioning.OData.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;

[ApiVersion( 1.0 )]
[ApiVersion( 2.0 )]
public class PeopleController : ODataController
{
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

    [EnableQuery]
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