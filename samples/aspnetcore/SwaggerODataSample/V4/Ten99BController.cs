using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Examples.Models;
using static Microsoft.AspNet.OData.Query.AllowedQueryOptions;
using static Microsoft.AspNetCore.Http.StatusCodes;

namespace Microsoft.Examples.V4
{
    [ApiVersion( "4.0" )]
    [ODataRoutePrefix( "Ten99B" )]
    public class Ten99BController : ODataController
    {
        /// <summary>
        /// Retrieves all orders.
        /// </summary>
        /// <param name="year">Tax year</param>
        /// <param name="pan">PAN</param>
        /// <returns>All available orders.</returns>
        /// <response code="200">Orders successfully retrieved.</response>
        /// <response code="400">The order is invalid.</response>
        [ODataRoute( "({year},{pan})" )]
        [Produces( "application/json" )]
        [ProducesResponseType( typeof( IQueryable<Ten99BModel> ), Status200OK )]
        [EnableQuery( MaxTop = 100, AllowedQueryOptions = Select | Top | Skip | Count )]
        public IQueryable<Ten99BModel> Get( [FromODataUri] short year, [FromODataUri] string pan )
        {
            var orders = new[]
            {
                new Ten99BModel {Pan = pan, TrustNo = "1", YrSeq = year, Account = new TrustModel {YrSeq = year, Details = new AccountDetails { Name1 = pan }}},
                new Ten99BModel {Pan = pan, TrustNo = "2", YrSeq = year, Account = new TrustModel {YrSeq = year, Details = new AccountDetails { Name1 = pan }}},
                new Ten99BModel {Pan = pan, TrustNo = "3", YrSeq = year, Account = new TrustModel {YrSeq = year, Details = new AccountDetails { Name1 = pan }}}
            };

            return orders.AsQueryable();
        }
    }
}