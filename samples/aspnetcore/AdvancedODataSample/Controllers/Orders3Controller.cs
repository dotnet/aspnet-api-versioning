namespace Microsoft.Examples.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Examples.Models;

    [ApiController]
    [ApiVersion( "3.0" )]
    [ControllerName( "Orders" )]
    [Route( "api/orders" )]
    public class Orders3Controller : ControllerBase
    {
        // GET ~/api/orders?api-version=3.0
        [HttpGet]
        public IActionResult Get( ApiVersion version ) => Ok( new[] { new Order() { Id = 1, Customer = $"Customer v{version}" } } );

        // GET ~/api/orders/{id}?api-version=3.0
        [HttpGet( "{id}" )]
        public IActionResult Get( int id, ApiVersion version ) => Ok( new Order() { Id = id, Customer = $"Customer v{version}" } );
    }
}