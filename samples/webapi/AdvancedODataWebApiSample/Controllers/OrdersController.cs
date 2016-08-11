namespace Microsoft.Examples.Controllers
{
    using Microsoft.Web.Http;
    using Models;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.OData;
    using System.Web.OData.Query;
    using System.Web.OData.Routing;

    // note: since the application is configured with AssumeDefaultVersionWhenUnspecifed, this controller
    // is implicitly versioned to the DefaultApiVersion, which has the default value 1.0.
    public class OrdersController : ApiController
    {
        // GET ~/orders
        // GET ~/orders?api-version=1.0
        public IHttpActionResult Get() => Ok( new[] { new Order() { Id = 1, Customer = $"Customer v{Request.GetRequestedApiVersion()}" } } );

        // GET ~/orders/{id}
        // GET ~/orders/{id}?api-version=1.0
        public IHttpActionResult Get( int id ) => Ok( new Order() { Id = id, Customer = $"Customer v{Request.GetRequestedApiVersion()}" } );
    }
}