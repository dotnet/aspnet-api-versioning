namespace Microsoft.Examples.Controllers
{
    using Microsoft.Examples.Models;
    using Microsoft.Web.Http;
    using System.Web.Http;

    [ApiVersion( "3.0" )]
    [ControllerName( "Orders" )]
    public class Orders3Controller : ApiController
    {
        // GET ~/api/orders?api-version=3.0
        public IHttpActionResult Get( ApiVersion version ) => Ok( new[] { new Order() { Id = 1, Customer = $"Customer v{version}" } } );

        // GET ~/api/orders/{id}?api-version=3.0
        public IHttpActionResult Get( int id, ApiVersion version ) => Ok( new Order() { Id = id, Customer = $"Customer v{version}" } );
    }
}