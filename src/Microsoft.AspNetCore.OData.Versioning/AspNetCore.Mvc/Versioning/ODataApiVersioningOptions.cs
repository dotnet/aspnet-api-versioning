namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using Microsoft.AspNetCore.Mvc.Versioning.Conventions;
    using System;

    /// <summary>
    /// Represents the possible API versioning options for OData services.
    /// </summary>
    public class ODataApiVersioningOptions
    {
        /// <summary>
        /// Gets or sets the builder used to define API version conventions.
        /// </summary>
        /// <value>An <see cref="ApiVersionConventionBuilder">API version convention builder</see>.</value>
        /// <remarks>These conventions are meant to be applied to OData specific features and built-in
        /// services such as the metadata controller.</remarks>
        [CLSCompliant( false )]
        public ApiVersionConventionBuilder Conventions { get; set; } = new ApiVersionConventionBuilder();

        /// <summary>
        /// Gets or sets a value indicating whether qualified names are used when building URLs.
        /// </summary>
        /// <value>True if qualified names are used when building URLs; otherwise, false. The default value is <c>false</c>.</value>
        public bool UseQualifiedNames { get; set; }
    }
}