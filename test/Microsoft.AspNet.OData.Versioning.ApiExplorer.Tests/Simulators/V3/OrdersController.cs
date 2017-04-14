﻿namespace Microsoft.Web.Http.Simulators.V3
{
    using Microsoft.Web.Http.Description;
    using Microsoft.Web.Http.Simulators.Models;
    using System.Collections.Generic;
    using System.Web.Http;
    using System.Web.Http.Description;
    using System.Web.OData;
    using static System.Linq.Enumerable;
    using static System.Net.HttpStatusCode;

    public class OrdersController : ODataController
    {
        [ResponseType( typeof( ODataValue<IEnumerable<Order>> ) )]
        public IHttpActionResult Get() => Ok( Empty<Person>() );

        [ResponseType( typeof( ODataValue<Order> ) )]
        public IHttpActionResult Get( int id ) => Ok( new Order() { Id = id } );

        [ResponseType( typeof( ODataValue<Order> ) )]
        public IHttpActionResult Post( [FromBody] Order order )
        {
            order.Id = 42;
            return Created( order );
        }

        public IHttpActionResult Delete( int id ) => StatusCode( NoContent );
    }
}