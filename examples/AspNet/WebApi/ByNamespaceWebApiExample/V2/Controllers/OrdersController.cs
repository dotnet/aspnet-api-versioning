namespace ApiVersioning.Examples.V2.Controllers;

using ApiVersioning.Examples.V2.Models;
using Asp.Versioning;
using System.Web.Http;

[RoutePrefix( "v{version:apiVersion}/[controller]" )]
public class OrdersController : ApiController
{
    // GET ~/v2/orders/{accountId}
    [Route( "{accountId}" )]
    public IHttpActionResult Get( string accountId, ApiVersion apiVersion ) =>
        Ok( new Order( GetType().FullName, accountId, apiVersion.ToString() ) );
}