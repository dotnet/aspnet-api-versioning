namespace ApiVersioning.Examples.Configuration;

using ApiVersioning.Examples.Models;
using Asp.Versioning;
using Asp.Versioning.OData;
using Microsoft.AspNet.OData.Builder;

public class OrderModelConfiguration : IModelConfiguration
{
    private static readonly ApiVersion V2 = new( 2, 0 );

    private EntityTypeConfiguration<Order> ConfigureCurrent( ODataModelBuilder builder )
    {
        var order = builder.EntitySet<Order>( "Orders" ).EntityType;

        order.HasKey( p => p.Id );

        return order;
    }

    public void Apply( ODataModelBuilder builder, ApiVersion apiVersion, string routePrefix )
    {
        // note: the EDM for orders is only available in version 2.0
        if ( apiVersion == V2 )
        {
            ConfigureCurrent( builder );
        }
    }
}