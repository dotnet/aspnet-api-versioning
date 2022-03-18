// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Simulators.V1;

using Asp.Versioning.OData;
using Asp.Versioning.Simulators.Models;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using System.Web.Http;
using System.Web.Http.Description;

[ApiVersion( "0.9" )]
[ApiVersion( "1.0" )]
[ODataRoutePrefix( "People" )]
public class PeopleController : ODataController
{
    [HttpGet]
    [ODataRoute( "({id})" )]
    [ResponseType( typeof( ODataValue<Person> ) )]
    public IHttpActionResult Get( [FromODataUri] int id ) => Ok( new Person() { Id = id } );
}