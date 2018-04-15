namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using Microsoft.AspNetCore.Mvc.Versioning.Conventions;
    using System;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Represents the possible API versioning options for OData services.
    /// </summary>
    public class ODataApiVersioningOptions
    {
        ApiVersionConventionBuilder conventions = new ApiVersionConventionBuilder();

        /// <summary>
        /// Gets or sets the builder used to define API version conventions.
        /// </summary>
        /// <value>An <see cref="ApiVersionConventionBuilder">API version convention builder</see>.</value>
        /// <remarks>These conventions are meant to be applied to OData specific features and built-in
        /// services such as the metadata controller.</remarks>
        [CLSCompliant( false )]
        public ApiVersionConventionBuilder Conventions
        {
            get
            {
                Contract.Ensures( conventions != null );
                return conventions;
            }
            set
            {
                Arg.NotNull( value, nameof( value ) );
                conventions = value;
            }
        }
    }
}