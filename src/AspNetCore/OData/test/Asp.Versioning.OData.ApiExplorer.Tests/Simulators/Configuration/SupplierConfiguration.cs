// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Simulators.Configuration;

using Asp.Versioning.OData;
using Asp.Versioning.Simulators.Models;
using Microsoft.OData.ModelBuilder;

/// <summary>
/// Represents the model configuration for suppliers.
/// </summary>
public class SupplierConfiguration : IModelConfiguration
{
    /// <inheritdoc />
    public void Apply( ODataModelBuilder builder, ApiVersion apiVersion, string routePrefix )
    {
        ArgumentNullException.ThrowIfNull( builder );

        if ( apiVersion < ApiVersions.V3 )
        {
            return;
        }

        var supplier = builder.EntitySet<Supplier>( "Suppliers" ).EntityType.HasKey( p => p.Id );
    }
}