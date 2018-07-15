namespace Microsoft.AspNet.OData.Simulators.Configuration
{
    using Microsoft.AspNet.OData.Builder;
    using Microsoft.AspNet.OData.Simulators.Models;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Represents the model configuration for orders.
    /// </summary>
    public class OrderModelConfiguration : IModelConfiguration
    {
        /// <summary>
        /// Applies model configurations using the provided builder for the specified API version.
        /// </summary>
        /// <param name="builder">The <see cref="ODataModelBuilder">builder</see> used to apply configurations.</param>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> associated with the <paramref name="builder"/>.</param>
        public void Apply( ODataModelBuilder builder, ApiVersion apiVersion )
        {
            var order = builder.EntitySet<Order>( "Orders" ).EntityType.HasKey( o => o.Id );

            if ( apiVersion < ApiVersions.V2 )
            {
                order.Ignore( o => o.EffectiveDate );
            }

            if ( apiVersion < ApiVersions.V3 )
            {
                order.Ignore( o => o.Description );
            }

            if ( apiVersion >= ApiVersions.V1 )
            {
                order.Collection.Function( "MostExpensive" ).ReturnsFromEntitySet<Order>( "Orders" );
            }

            if ( apiVersion >= ApiVersions.V2 )
            {
                order.Action( "Rate" ).Parameter<int>( "rating" );
            }
        }
    }
}