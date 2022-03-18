namespace ApiVersioning.Examples.V1.Controllers;

using ApiVersioning.Examples.V1.Models;
using Asp.Versioning;
using System.Web.Http;

[RoutePrefix( "v{version:apiVersion}/orders" )]
public class OrdersController : ApiController
{
    // GET ~/v1/orders/{accountId}
    [Route( "{accountId}" )]
    public IHttpActionResult Get( string accountId, ApiVersion apiVersion ) =>
        Ok( new Order( GetType().FullName, accountId, apiVersion.ToString() ) );
}