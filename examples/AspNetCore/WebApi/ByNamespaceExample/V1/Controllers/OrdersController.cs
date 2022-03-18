namespace ApiVersioning.Examples.V1.Controllers;

using ApiVersioning.Examples.V1.Models;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

[Route( "[controller]" )]
[Route( "v{version:apiVersion}/[controller]" )]
public class OrdersController : ControllerBase
{
    // GET ~/v1/orders/{accountId}
    // GET ~/orders/{accountId}?api-version=1.0
    [HttpGet( "{accountId}" )]
    public IActionResult Get( string accountId, ApiVersion apiVersion ) =>
        Ok( new Order( GetType().FullName, accountId, apiVersion.ToString() ) );
}