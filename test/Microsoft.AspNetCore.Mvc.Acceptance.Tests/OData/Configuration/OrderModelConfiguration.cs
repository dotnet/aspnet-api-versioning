﻿namespace Microsoft.AspNetCore.OData.Configuration
{
    using Microsoft.AspNet.OData.Builder;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.OData.Models;

    public class OrderModelConfiguration : IModelConfiguration
    {
        static readonly ApiVersion V1 = new ApiVersion( 1, 0 );
        readonly ApiVersion supportedApiVersion;

        public OrderModelConfiguration() : this( V1 ) { }

        public OrderModelConfiguration( ApiVersion supportedApiVersion ) => this.supportedApiVersion = supportedApiVersion;

        EntityTypeConfiguration<Order> ConfigureCurrent( ODataModelBuilder builder )
        {
            var order = builder.EntitySet<Order>( "Orders" ).EntityType;
            order.HasKey( p => p.Id );
            return order;
        }

        public void Apply( ODataModelBuilder builder, ApiVersion apiVersion )
        {
            if ( supportedApiVersion == apiVersion )
            {
                ConfigureCurrent( builder );
            }
        }
    }
}