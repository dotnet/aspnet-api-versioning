// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0060 // Remove unused parameter

namespace Asp.Versioning.Simulators.V3;

using Asp.Versioning.OData;
using Asp.Versioning.Simulators.Models;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Description;
using static System.Linq.Enumerable;
using static System.Net.HttpStatusCode;

[ApiVersion( "3.0" )]
[AdvertiseApiVersions( "4.0" )]
[ODataRoutePrefix( "People" )]
public class PeopleController : ODataController
{
    [HttpGet]
    [ODataRoute]
    [ResponseType( typeof( ODataValue<IEnumerable<Person>> ) )]
    public IHttpActionResult Get() => Ok( Empty<Person>() );

    [HttpGet]
    [ODataRoute( "({id})" )]
    [ResponseType( typeof( ODataValue<Person> ) )]
    public IHttpActionResult Get( [FromODataUri] int id ) => Ok( new Person() { Id = id } );

    [HttpPost]
    [ODataRoute]
    [ResponseType( typeof( ODataValue<Person> ) )]
    public IHttpActionResult Post( [FromBody] Person person )
    {
        person.Id = 42;
        return Created( person );
    }

    [HttpDelete]
    [ODataRoute( "({id})" )]
    public IHttpActionResult Delete( [FromODataUri] int id ) => StatusCode( NoContent );
}