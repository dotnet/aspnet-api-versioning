namespace Microsoft.Examples.Configuration
{
    using Microsoft.AspNet.OData.Builder;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Examples.Models;

    /// <summary>
    /// Represents the model configuration for orders.
    /// </summary>
    public class OrderModelConfiguration : IModelConfiguration
    {
        /// <inheritdoc />
        public void Apply( ODataModelBuilder builder, ApiVersion apiVersion, string routePrefix )
        {
            var order = builder.EntitySet<Order>( "Orders" ).EntityType.HasKey( o => o.Id );
            var lineItem = builder.EntityType<LineItem>().HasKey( li => li.Number );

            if ( apiVersion < ApiVersions.V2 )
            {
                order.Ignore( o => o.EffectiveDate );
                lineItem.Ignore( li => li.Fulfilled );
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