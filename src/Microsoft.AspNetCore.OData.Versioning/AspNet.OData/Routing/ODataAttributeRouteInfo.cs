namespace Microsoft.AspNet.OData.Routing
{
    using Microsoft.AspNet.OData.Routing.Template;
    using Microsoft.AspNetCore.Mvc.Routing;
    using System;

    /// <summary>
    /// Represents the routing information for an OData action.
    /// </summary>
    [CLSCompliant( false )]
    public class ODataAttributeRouteInfo : AttributeRouteInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataAttributeRouteInfo"/> class.
        /// </summary>
        public ODataAttributeRouteInfo()
        {
            SuppressLinkGeneration = true;
            SuppressPathMatching = true;
        }

        /// <summary>
        /// Gets or sets the corresponding OData template.
        /// </summary>
        /// <value>The <see cref="ODataPathTemplate">OData path template</see> for the action.</value>
        public ODataPathTemplate? ODataTemplate { get; set; }
    }
}