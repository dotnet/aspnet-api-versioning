﻿namespace Microsoft.Examples.Controllers
{
    using Microsoft.Examples.Models;
    using System.Web.Http;

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