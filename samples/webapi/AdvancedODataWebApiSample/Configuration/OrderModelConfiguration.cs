namespace Microsoft.Examples.Configuration
{
    using Microsoft.Web.Http;
    using Microsoft.Web.OData.Builder;
    using Models;
    using System.Web.OData.Builder;

    public class OrderModelConfiguration : IModelConfiguration
    {
        private static readonly ApiVersion V2 = new ApiVersion( 2, 0 );

        private EntityTypeConfiguration<Order> ConfigureCurrent( ODataModelBuilder builder )
        {
            var order = builder.EntitySet<Order>( "Orders" ).EntityType;

            order.HasKey( p => p.Id );

            return order;
        }

        public void Apply( ODataModelBuilder builder, ApiVersion apiVersion )
        {
            // note: the EDM for orders is only available in version 2.0
            if ( apiVersion == V2 )
            {
                ConfigureCurrent( builder );
            }
        }
    }
}