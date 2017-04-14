﻿namespace Microsoft.Examples.Configuration
{
    using Microsoft.Web.Http;
    using Microsoft.Web.OData.Builder;
    using Models;
    using System.Web.OData.Builder;

    /// <summary>
    /// Represents the model configuration for <see cref="Order">orders</see>.
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
            var order = builder.EntitySet<Order>( "Orders" ).EntityType;

            order.HasKey( o => o.Id );

            if ( apiVersion < ApiVersions.V2 )
            {
                order.Ignore( o => o.EffectiveDate );
            }
        }
    }
}