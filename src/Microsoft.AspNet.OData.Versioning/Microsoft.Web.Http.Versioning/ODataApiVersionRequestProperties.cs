namespace Microsoft.Web.Http.Versioning
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents current OData API versioning request properties.
    /// </summary>
    public class ODataApiVersionRequestProperties
    {
        /// <summary>
        /// Gets a collection of API version to route name mappings that have been matched in the current request.
        /// </summary>
        /// <value>A <see cref="IDictionary{TKey, TValue}">collection</see> of key/value pairs representing the mapping
        /// of <see cref="ApiVersion">API versions</see> to route names that have been matched in the current request.</value>
        public IDictionary<ApiVersion, string> MatchingRoutes { get; } = new Dictionary<ApiVersion, string>();
    }
}