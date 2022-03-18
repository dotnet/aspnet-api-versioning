namespace ApiVersioning.Examples.Controllers;

using ApiVersioning.Examples.Models;
using Asp.Versioning;
using System.Web.Http;

// note: since the application is configured with AssumeDefaultVersionWhenUnspecified, this controller
// is implicitly versioned to the DefaultApiVersion, which has the default value 1.0.
public class OrdersController : ApiController
{
    // GET ~/api/orders
    // GET ~/api/orders?api-version=1.0
    public IHttpActionResult Get( ApiVersion version ) => 
        Ok( new[] { new Order() { Id = 1, Customer = $"Customer v{version}" } } );

    // GET ~/api/orders/{id}
    // GET ~/api/orders/{id}?api-version=1.0
    public IHttpActionResult Get( int id, ApiVersion version ) =>
        Ok( new Order() { Id = id, Customer = $"Customer v{version}" } );
}