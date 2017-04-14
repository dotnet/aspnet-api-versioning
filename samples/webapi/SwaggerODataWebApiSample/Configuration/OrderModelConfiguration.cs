namespace Microsoft.Examples.Configuration
{
    using Microsoft.Web.Http;
    using Microsoft.Web.OData.Builder;
    using System.Web.OData.Builder;

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
            if ( apiVersion <= ApiVersions.V1 )
            {
                builder.EntitySet<V1.Models.Order>( "Orders" ).EntityType.HasKey( o => o.Id );
            }
            else if ( apiVersion == ApiVersions.V2 )
            {
                builder.EntitySet<V2.Models.Order>( "Orders" ).EntityType.HasKey( o => o.Id );
            }
            else if ( apiVersion == ApiVersions.V3 )
            {
                builder.EntitySet<V3.Models.Order>( "Orders" ).EntityType.HasKey( o => o.Id );
            }
        }
    }
}