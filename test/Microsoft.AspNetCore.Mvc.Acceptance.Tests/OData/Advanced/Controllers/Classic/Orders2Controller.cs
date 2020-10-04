namespace Microsoft.AspNetCore.OData.Advanced.Controllers.Classic
{
    using Microsoft.AspNet.OData;
    using Microsoft.AspNet.OData.Query;
    using Microsoft.AspNet.OData.Routing;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.OData.Models;

    [ApiVersion( "2.0" )]
    [ControllerName( "Orders" )]
    [ODataRoutePrefix( "Orders" )]
    public class Orders2Controller : ODataController
    {
        [ODataRoute]
        public IActionResult Get( ODataQueryOptions<Order> options, ApiVersion apiVersion ) =>
            Ok( new[] { new Order() { Id = 1, Customer = $"Customer v{apiVersion}" } } );

        [ODataRoute( "{key}" )]
        public IActionResult Get( int key, ODataQueryOptions<Order> options, ApiVersion apiVersion ) =>
            Ok( new Order() { Id = key, Customer = $"Customer v{apiVersion}" } );
    }
}