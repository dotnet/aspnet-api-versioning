// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Simulators.V2;

using Asp.Versioning.OData;
using Asp.Versioning.Simulators.Models;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Description;
using static System.Linq.Enumerable;

[ApiVersion( "2.0" )]
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
}