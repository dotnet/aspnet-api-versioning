namespace Microsoft.Web.Http.Routing
{
    using System;

    /// <summary>
    /// Provides keys for looking up route values and data tokens.
    /// </summary>
    static class RouteDataTokenKeys
    {
        /// <summary>
        /// Used to provide the action descriptors to consider for attribute routing.
        /// </summary>
        internal const string Actions = "actions";

        /// <summary>
        /// Used to indicate that a route is a controller-level attribute route.
        /// </summary>
        internal const string Controller = "controller";

        /// <summary>
        /// Used to allow customer-provided disambiguation between multiple matching attribute routes
        /// </summary>
        internal const string Order = "order";

        /// <summary>
        /// Used to allow URI constraint-based disambiguation between multiple matching attribute routes
        /// </summary>
        internal const string Precedence = "precedence";
    }
}