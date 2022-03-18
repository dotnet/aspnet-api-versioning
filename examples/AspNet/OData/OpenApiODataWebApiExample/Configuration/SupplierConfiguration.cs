namespace ApiVersioning.Examples.Configuration;

using ApiVersioning.Examples.Models;
using Asp.Versioning;
using Asp.Versioning.OData;
using Microsoft.AspNet.OData.Builder;

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

        builder.EntitySet<Supplier>( "Suppliers" ).EntityType.HasKey( p => p.Id );
        builder.Singleton<Supplier>( "Acme" );
    }
}