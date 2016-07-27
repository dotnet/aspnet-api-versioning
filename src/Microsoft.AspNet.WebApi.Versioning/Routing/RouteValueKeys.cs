namespace Microsoft.Web.Http.Routing
{
    using System;

    /// <summary>
    /// Provides keys for looking up route values.
    /// </summary>
    internal static class RouteValueKeys
    {
        /// <summary>
        /// Used to provide the action name.
        /// </summary>
        public const string Action = "action";

        /// <summary>
        /// Used to provide the controller name.
        /// </summary>
        public const string Controller = "controller";
    }
}
