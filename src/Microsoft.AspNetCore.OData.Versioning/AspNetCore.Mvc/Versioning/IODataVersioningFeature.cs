namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Defines the behavior of the OData versioning feature.
    /// </summary>
    public interface IODataVersioningFeature
    {
        /// <summary>
        /// Gets a collection of API version to route name mappings that have been matched in the current request.
        /// </summary>
        /// <value>A <see cref="IDictionary{TKey, TValue}">collection</see> of key/value pairs representing the mapping
        /// of <see cref="ApiVersion">API versions</see> to route names that have been matched in the current request.</value>
        IDictionary<ApiVersion, string> MatchingRoutes { get; }
    }
}