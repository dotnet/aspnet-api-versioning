namespace ApiVersioning.Examples.Controllers;

using ApiVersioning.Examples.Models;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

// note: since the application is configured with AssumeDefaultVersionWhenUnspecified, this controller
// is implicitly versioned to the DefaultApiVersion, which has the default value 1.0.
[ApiController]
[Route( "api/orders" )]
public class OrdersController : ControllerBase
{
    // GET ~/api/orders
    // GET ~/api/orders?api-version=1.0
    [HttpGet]
    public IActionResult Get( ApiVersion version ) =>
        Ok( new[] { new Order() { Id = 1, Customer = $"Customer v{version}" } } );

    // GET ~/api/orders/{id}
    // GET ~/api/orders/{id}?api-version=1.0
    [HttpGet( "{id}" )]
    public IActionResult Get( int id, ApiVersion version ) =>
        Ok( new Order() { Id = id, Customer = $"Customer v{version}" } );
}