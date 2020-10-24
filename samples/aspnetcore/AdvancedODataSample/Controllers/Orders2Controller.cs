namespace Microsoft.Examples.Controllers
{
    using Microsoft.AspNet.OData;
    using Microsoft.AspNet.OData.Query;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Examples.Models;

    [ApiVersion( "2.0" )]
    [ControllerName( "Orders" )]
    public class Orders2Controller : ODataController
    {
        // GET ~/api/orders?api-version=2.0
        [HttpGet]
        public IActionResult Get( ODataQueryOptions<Order> options, ApiVersion version ) =>
            Ok( new[] { new Order() { Id = 1, Customer = $"Customer v{version}" } } );

        // GET ~/api/orders/{key}?api-version=2.0
        [HttpGet( "{key}" )]
        public IActionResult Get( int key, ODataQueryOptions<Order> options, ApiVersion version ) =>
            Ok( new Order() { Id = key, Customer = $"Customer v{version}" } );
    }
}