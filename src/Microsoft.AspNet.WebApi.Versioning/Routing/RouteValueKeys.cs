namespace Microsoft.Web.Http.Routing
{
    using System;

    /// <summary>
    /// Provides keys for looking up route values.
    /// </summary>
    static class RouteValueKeys
    {
        /// <summary>
        /// Used to provide the action name.
        /// </summary>
        internal const string Action = "action";

        /// <summary>
        /// Used to provide the controller name.
        /// </summary>
        internal const string Controller = "controller";
    }
}