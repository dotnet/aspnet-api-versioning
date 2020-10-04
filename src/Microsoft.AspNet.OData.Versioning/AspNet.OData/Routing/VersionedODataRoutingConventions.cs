namespace Microsoft.AspNet.OData.Routing
{
    using Microsoft.AspNet.OData.Routing.Conventions;
    using System.Collections.Generic;
    using System.Web.Http;

    /// <content>
    /// Provides additional implementation specific to ASP.NET Web API.
    /// </content>
    public static partial class VersionedODataRoutingConventions
    {
        /// <summary>
        /// Creates a mutable list of the default OData routing conventions with attribute routing enabled.
        /// </summary>
        /// <param name="routeName">The name of the route.</param>
        /// <param name="configuration">The current <see cref="HttpConfiguration">configuration</see>.</param>
        /// <returns>A mutable list of the default OData routing conventions.</returns>
        public static IList<IODataRoutingConvention> CreateDefaultWithAttributeRouting( string routeName, HttpConfiguration configuration ) =>
            EnsureConventions( ODataRoutingConventions.CreateDefault(), new VersionedAttributeRoutingConvention( routeName, configuration ) );
    }
}