using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Examples.Models;

namespace Microsoft.Examples.V4
{
    [ApiVersion( "4.0" )]
    [ODataRoutePrefix( "Ten99Div" )]
    public class Ten99DivController : ODataController
    {
        /// <summary>
        /// Retrieves all orders.
        /// </summary>
        /// <returns>All available orders.</returns>
        /// <response code="200">Orders successfully retrieved.</response>
        /// <response code="400">The order is invalid.</response>
        [ODataRoute( "({year},{pan})" )]
        [ProducesResponseType( typeof( IQueryable<Ten99DivModel> ), StatusCodes.Status200OK )]
        [EnableQuery( MaxTop = 100, AllowedQueryOptions = AllowedQueryOptions.Select | AllowedQueryOptions.Top | AllowedQueryOptions.Skip | AllowedQueryOptions.Count )]
        [Produces( "application/json" )]
        public IQueryable<Ten99DivModel> Get( [FromODataUri] short year, [FromODataUri] string pan )
        {
            var orders = new[]
            {
                new Ten99DivModel {Pan = pan, TrustNo = "1", YrSeq = year, Account = new TrustModel {YrSeq = year, Details = new AccountDetails {Name1 = pan}}},
                new Ten99DivModel {Pan = pan, TrustNo = "2", YrSeq = year, Account = new TrustModel {YrSeq = year, Details = new AccountDetails {Name1 = pan}}},
                new Ten99DivModel {Pan = pan, TrustNo = "3", YrSeq = year, Account = new TrustModel {YrSeq = year, Details = new AccountDetails {Name1 = pan}}}
            };

            return orders.AsQueryable();
        }

    }
}