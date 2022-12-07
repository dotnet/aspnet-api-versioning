// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0060

namespace Asp.Versioning.Simulators;

using Asp.Versioning.Models;
using System.Web.Http;
using System.Web.Http.Description;

[ApiVersion( "2.0" )]
[ApiVersion( "3.0-beta", Deprecated = true )]
[ApiVersion( "3.0" )]
[RoutePrefix( "Values" )]
public class AttributeValues2Controller : ApiController
{
    [Route]
    public string Get() => "Test";

    [Route]
    [MapToApiVersion( "3.0" )]
    [ResponseType( typeof( string ) )]
    public IHttpActionResult GetV3() => Ok( "Test" );

    [Route( "{id:int}" )]
    public IHttpActionResult Get( int id ) => Ok();

    [Route]
    [MapToApiVersion( "3.0" )]
    public IHttpActionResult Post( ClassWithId resource )
    {
        resource.Id = 1;
        return Created( "values/1", resource );
    }
}