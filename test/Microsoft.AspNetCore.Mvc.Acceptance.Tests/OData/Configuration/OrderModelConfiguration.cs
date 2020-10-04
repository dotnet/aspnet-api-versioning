namespace Microsoft.AspNetCore.OData.Configuration
{
    using Microsoft.AspNet.OData.Builder;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.OData.Models;

    public class OrderModelConfiguration : IModelConfiguration
    {
        readonly ApiVersion supportedApiVersion;

        public OrderModelConfiguration() { }

        public OrderModelConfiguration( ApiVersion supportedApiVersion ) => this.supportedApiVersion = supportedApiVersion;

        EntityTypeConfiguration<Order> ConfigureCurrent( ODataModelBuilder builder )
        {
            var order = builder.EntitySet<Order>( "Orders" ).EntityType;
            order.HasKey( p => p.Id );
            return order;
        }

        public void Apply( ODataModelBuilder builder, ApiVersion apiVersion, string routePrefix )
        {
            if ( supportedApiVersion == null || supportedApiVersion == apiVersion )
            {
                ConfigureCurrent( builder );
            }
        }
    }
}