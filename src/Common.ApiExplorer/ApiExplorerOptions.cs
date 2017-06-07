#if WEBAPI
namespace Microsoft.Web.Http.Description
#else
namespace Microsoft.AspNetCore.Mvc.ApiExplorer
#endif
{
    using System;
    using Versioning;
#if !WEBAPI
    using LocalSR = SR;
#endif

    /// <summary>
    /// Represents the possible API versioning options for the API explorer.
    /// </summary>
    public partial class ApiExplorerOptions
    {
        /// <summary>
        /// Gets or sets the format used to create group names from API versions.
        /// </summary>
        /// <value>The string format used to format an <see cref="ApiVersion">API version</see>
        /// as a group name. The default value is <c>null</c>.</value>
        /// <remarks>For information about API version formatting, review <see cref="ApiVersionFormatProvider"/>
        /// as well as the <see cref="ApiVersion.ToString(string)"/> and <see cref="ApiVersion.ToString(string, IFormatProvider)"/>
        /// methods.</remarks>
        public string GroupNameFormat { get; set; }
        
        /// <summary>
        /// Gets or sets the default description used for API version parameters.
        /// </summary>
        /// <value>The default description for API version parameters. The default value
        /// is "The requested API version".</value>
        public string DefaultApiVersionParameterDescription { get; set; } = LocalSR.DefaultApiVersionParamDesc;
    }
}