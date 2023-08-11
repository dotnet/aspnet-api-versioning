namespace ApiVersioning.Examples.Configuration;

using ApiVersioning.Examples.Models;
using Asp.Versioning;
using Asp.Versioning.OData;
using Microsoft.OData.ModelBuilder;

/// <summary>
/// Represents the model configuration for suppliers.
/// </summary>
public class SupplierConfiguration : IModelConfiguration
{
    /// <inheritdoc />
    public void Apply( ODataModelBuilder builder, ApiVersion apiVersion, string routePrefix )
    {
        if ( apiVersion < ApiVersions.V3 )
        {
            return;
        }

        var supplier = builder.EntitySet<Supplier>( "Suppliers" ).EntityType;


        supplier.HasKey( p => p.Id );
        supplier.Page( maxTopValue: 100, pageSizeValue: default );

        builder.Singleton<Supplier>( "Acme" );
    }
}