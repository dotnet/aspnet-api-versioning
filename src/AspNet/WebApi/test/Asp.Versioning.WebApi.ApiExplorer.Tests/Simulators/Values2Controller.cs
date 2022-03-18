// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable CA1062 // Validate arguments of public methods
#pragma warning disable CA1822 // Mark members as static

namespace Asp.Versioning.Simulators;

using Asp.Versioning.Models;
using System.Web.Http;
using System.Web.Http.Description;

[ControllerName( "Values" )]
public class Values2Controller : ApiController
{
    public string Get() => "Test";

    [ResponseType( typeof( string ) )]
    public IHttpActionResult GetV3() => Ok( "Test" );

    public IHttpActionResult Get( int id ) => Ok();

    public IHttpActionResult Post( ClassWithId resource )
    {
        resource.Id = 1;
        return Created( "values/1", resource );
    }
}