namespace Microsoft.AspNet.OData.Simulators.Configuration
{
    using Microsoft.AspNet.OData.Builder;
    using Microsoft.AspNet.OData.Simulators.Models;
    using Microsoft.AspNetCore.Mvc;
    using System;

    /// <summary>
    /// Represents the model configuration for products.
    /// </summary>
    public class ProductConfiguration : IModelConfiguration
    {
        /// <summary>
        /// Applies model configurations using the provided builder for the specified API version.
        /// </summary>
        /// <param name="builder">The <see cref="ODataModelBuilder">builder</see> used to apply configurations.</param>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> associated with the <paramref name="builder"/>.</param>
        public void Apply( ODataModelBuilder builder, ApiVersion apiVersion )
        {
            if ( apiVersion < ApiVersions.V3 )
            {
                return;
            }

            var product = builder.EntitySet<Product>( "Products" ).EntityType.HasKey( p => p.Id );
        }
    }
}