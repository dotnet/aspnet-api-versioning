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
        /// <inheritdoc />
        public void Apply( ODataModelBuilder builder, ApiVersion apiVersion, string routePrefix )
        {
            if ( apiVersion < ApiVersions.V3 )
            {
                return;
            }

            var product = builder.EntitySet<Product>( "Products" ).EntityType.HasKey( p => p.Id );
        }
    }
}