namespace ApiVersioning.Examples.Controllers;

using Asp.Versioning;
using System.Web.Http;

[RoutePrefix( "api/values" )]
public class ValuesController : ApiController
{
    // GET api/values?api-version=1.0
    [Route]
    public IHttpActionResult Get( ApiVersion apiVersion ) =>
        Ok( new { controller = GetType().Name, version = apiVersion.ToString() } );

    // GET api/values/{id}?api-version=1.0
    [Route( "{id:int}" )]
    public IHttpActionResult Get( int id, ApiVersion apiVersion ) =>
        Ok( new { controller = GetType().Name, id, version = apiVersion.ToString() } );
}