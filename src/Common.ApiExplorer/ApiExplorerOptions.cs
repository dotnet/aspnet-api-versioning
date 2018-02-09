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
        /// Gets or sets the format used to format the API version value substituted in route templates.
        /// </summary>
        /// <value>The string format used to format an <see cref="ApiVersion">API version</see>
        /// in a route template. The default value is "VVV", which formats the major version number
        /// and optional minor version.</value>
        /// <remarks>For information about API version formatting, review <see cref="ApiVersionFormatProvider"/>
        /// as well as the <see cref="ApiVersion.ToString(string)"/> and <see cref="ApiVersion.ToString(string, IFormatProvider)"/>
        /// methods.</remarks>
        public string SubstitutionFormat { get; set; } = "VVV";

        /// <summary>
        /// Gets or sets a value indicating whether the API version parameter should be substituted in route templates.
        /// </summary>
        /// <value>True if the API version parameter should be substituted in route templates; otherwise, false.
        /// The default value is <c>false</c>.</value>
        /// <remarks>Setting this property to <c>true</c> will also remove the API version parameter from the
        /// corresponding API description.</remarks>
        public bool SubstituteApiVersionInUrl { get; set; }

        /// <summary>
        /// Gets or sets the default description used for API version parameters.
        /// </summary>
        /// <value>The default description for API version parameters. The default value
        /// is "The requested API version".</value>
        public string DefaultApiVersionParameterDescription { get; set; } = LocalSR.DefaultApiVersionParamDesc;
    }
}