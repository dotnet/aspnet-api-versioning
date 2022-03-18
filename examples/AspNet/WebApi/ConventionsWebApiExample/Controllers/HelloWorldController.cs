namespace ApiVersioning.Examples.Controllers;

using Asp.Versioning;
using System.Web.Http;

[RoutePrefix( "api/v{version:apiVersion}/helloworld" )]
public class HelloWorldController : ApiController
{
    // GET api/v{version}/helloworld
    [Route]
    public IHttpActionResult Get( ApiVersion apiVersion ) =>
        Ok( new { controller = GetType().Name, version = apiVersion.ToString() } );

    // GET api/v{version}/helloworld/{id}
    [Route( "{id:int}" )]
    public IHttpActionResult Get( int id, ApiVersion apiVersion ) =>
        Ok( new { controller = GetType().Name, id, version = apiVersion.ToString() } );
}