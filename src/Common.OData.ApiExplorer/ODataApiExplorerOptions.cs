#if WEBAPI
namespace Microsoft.Web.Http.Description
#else
namespace Microsoft.AspNetCore.Mvc.ApiExplorer
#endif
{
    using Microsoft.AspNet.OData;
    using Microsoft.AspNet.OData.Builder;
    using System;
    using System.ComponentModel;
#if WEBAPI
    using System.Web.Http;
    using System.Web.Http.Description;
#else
    using Microsoft.AspNetCore.Mvc;
#endif
    using static System.ComponentModel.EditorBrowsableState;

    /// <summary>
    /// Represents the possible API versioning options for an OData API explorer.
    /// </summary>
    public partial class ODataApiExplorerOptions : ApiExplorerOptions
    {
        ODataQueryOptionsConventionBuilder queryOptions = new ODataQueryOptionsConventionBuilder();

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
        /// Gets or sets a value indicating whether qualified names are used when building URLs.
        /// </summary>
        /// <value>True if qualified names are used when building URLs; otherwise, false. The default value is <c>false</c>.</value>
        public bool UseQualifiedNames { get; set; }

        /// <summary>
        /// Gets or sets the convention builder used to describe OData query options.
        /// </summary>
        /// <value>An <see cref="ODataActionQueryOptionsConventionBuilder">OData query option convention builder</see>.</value>
#if !WEBAPI
        [CLSCompliant( false )]
#endif
        public ODataQueryOptionsConventionBuilder QueryOptions
        {
            get => queryOptions;
            set
            {
                Arg.NotNull( value, nameof( value ) );
                queryOptions = value;
            }
        }
    }
}