namespace ApiVersioning.Examples.Configuration;

using ApiVersioning.Examples.Models;
using Asp.Versioning;
using Asp.Versioning.OData;
using Microsoft.AspNet.OData.Builder;

public class OrderModelConfiguration : IModelConfiguration
{
    private static readonly ApiVersion V1 = new( 1, 0 );

    private EntityTypeConfiguration<Order> ConfigureCurrent( ODataModelBuilder builder )
    {
        var order = builder.EntitySet<Order>( "Orders" ).EntityType;

        order.HasKey( p => p.Id );

        return order;
    }

    public void Apply( ODataModelBuilder builder, ApiVersion apiVersion, string routePrefix )
    {
        if ( routePrefix != "api/v{apiVersion}" )
        {
            return;
        }

        // note: the EDM for orders is only available in version 1.0
        if ( apiVersion == V1 )
        {
            ConfigureCurrent( builder );
        }
    }
}