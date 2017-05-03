namespace Microsoft.Web.Http.Description
{
    using System;
    using System.Web.Http;
    using System.Web.Http.Description;
    using System.Web.OData;

    /// <summary>
    /// Represents the possible API versioning options for an OData API explorer.
    /// </summary>
    public class ODataApiExplorerOptions : ApiExplorerOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataApiExplorerOptions"/> class.
        /// </summary>
        /// <param name="configuration">The current <see cref="HttpConfiguration">configuration</see> associated with the options.</param>
        public ODataApiExplorerOptions( HttpConfiguration configuration ) : base( configuration ) { }

        /// <summary>
        /// Gets or sets a value indicating whether the API explorer settings are honored.
        /// </summary>
        /// <value>True if the <see cref="ApiExplorerSettingsAttribute"/> is ignored; otherwise, false.
        /// The default value is <c>false</c>.</value>
        /// <remarks>Most OData services inherit from the <see cref="ODataController"/>, which excludes the controller
        /// from the <see cref="IApiExplorer">API explorer</see> by setting <see cref="ApiExplorerSettingsAttribute.IgnoreApi"/>
        /// to <c>true</c>. By setting this property to <c>false</c>, these settings are ignored instead of reapplying
        /// <see cref="ApiExplorerSettingsAttribute.IgnoreApi"/> with a value of <c>false</c> to all OData controllers.</remarks>
        public bool UseApiExplorerSettings { get; set; }
    }
}