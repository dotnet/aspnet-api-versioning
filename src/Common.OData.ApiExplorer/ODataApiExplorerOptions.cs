#if WEBAPI
namespace Microsoft.Web.Http.Description
#else
namespace Microsoft.AspNetCore.Mvc.ApiExplorer
#endif
{
    using Microsoft.AspNet.OData;
#if WEBAPI
    using System.Web.Http;
    using System.Web.Http.Description;
#else
    using Microsoft.AspNetCore.Mvc;
#endif

    /// <summary>
    /// Represents the possible API versioning options for an OData API explorer.
    /// </summary>
    public partial class ODataApiExplorerOptions : ApiExplorerOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether the API explorer settings are honored.
        /// </summary>
        /// <value>True if the <see cref="ApiExplorerSettingsAttribute"/> is ignored; otherwise, false.
        /// The default value is <c>false</c>.</value>
        /// <remarks>Most OData services inherit from the <see cref="ODataController"/>, which excludes the controller
        /// from the API explorer by setting <see cref="ApiExplorerSettingsAttribute.IgnoreApi"/>
        /// to <c>true</c>. By setting this property to <c>false</c>, these settings are ignored instead of reapplying
        /// <see cref="ApiExplorerSettingsAttribute.IgnoreApi"/> with a value of <c>false</c> to all OData controllers.</remarks>
        public bool UseApiExplorerSettings { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether qualified names are used when building URLs for operations (e.g. actions and functions).
        /// </summary>
        /// <value>True if qualified names are used when building URLs for operations; otherwise, false. The default value is <c>false</c>.</value>
        public bool UseQualifiedOperationNames { get; set; }
    }
}