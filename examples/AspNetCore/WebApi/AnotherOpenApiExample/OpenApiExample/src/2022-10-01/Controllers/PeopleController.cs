﻿namespace AspVersioning.Examples._2022_10_01.Controllers;

using AspVersioning.Examples._2022_10_01.Models;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Represents a RESTful people service.
/// </summary>
[ApiVersion("2022-10-01")]
[Route("v{version:apiVersion}/[controller]")]
public class PeopleController : ControllerBase
{
    /// <summary>
    /// Gets all people.
    /// </summary>
    /// <returns>All available people.</returns>
    /// <response code="200">The successfully retrieved people.</response>
    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IEnumerable<Person>), 200)]
    public IActionResult Get()
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

        return Ok(people);
    }

    /// <summary>
    /// Gets a single person.
    /// </summary>
    /// <param name="id">The requested person identifier.</param>
    /// <returns>The requested person.</returns>
    /// <response code="200">The person was successfully retrieved.</response>
    /// <response code="404">The person does not exist.</response>
    [HttpGet("{id:int}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(Person), 200)]
    [ProducesResponseType(404)]
    public IActionResult Get(int id) =>
        Ok(new Person()
        {
            Id = id,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@somewhere.com",
        });
}
