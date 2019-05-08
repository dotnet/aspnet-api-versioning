namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using System;
    using System.Collections.Generic;

    sealed class ODataVersioningFeature : IODataVersioningFeature
    {
        public IDictionary<ApiVersion, string> MatchingRoutes { get; } = new Dictionary<ApiVersion, string>();
    }
}