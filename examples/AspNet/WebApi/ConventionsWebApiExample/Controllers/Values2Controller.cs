namespace ApiVersioning.Examples.Controllers;

using System.Net.Http;
using System.Web.Http;

[RoutePrefix( "api/values" )]
public class Values2Controller : ApiController
{
    // GET api/values?api-version=2.0
    [Route]
    public IHttpActionResult Get() =>
        Ok( new
        {
            controller = GetType().Name,
            version = Request.GetRequestedApiVersion().ToString(),
        } );

    // GET api/values/{id}?api-version=2.0
    [Route( "{id:int}" )]
    public IHttpActionResult Get( int id ) =>
        Ok( new
        {
            controller = GetType().Name,
            id,
            version = Request.GetRequestedApiVersion().ToString(),
        } );

    // GET api/values?api-version=3.0
    [Route]
    public IHttpActionResult GetV3() =>
        Ok( new
        {
            controller = GetType().Name,
            version = Request.GetRequestedApiVersion().ToString(),
        } );

    // GET api/values/{id}?api-version=3.0
    [Route( "{id:int}" )]
    public IHttpActionResult GetV3( int id ) =>
        Ok( new
        {
            controller = GetType().Name,
            id,
            version = Request.GetRequestedApiVersion().ToString(),
        } );
}