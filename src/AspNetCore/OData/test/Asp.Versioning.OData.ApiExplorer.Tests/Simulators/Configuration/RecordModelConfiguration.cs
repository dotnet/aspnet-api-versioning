// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Simulators.Configuration;

using Asp.Versioning.OData;
using Asp.Versioning.Simulators.Models;
using Microsoft.OData.ModelBuilder;

/// <summary>
/// Represents the model configuration for records.
/// </summary>
public class RecordModelConfiguration : IModelConfiguration
{
    /// <inheritdoc />
    public void Apply( ODataModelBuilder builder, ApiVersion apiVersion, string routePrefix )
    {
        ArgumentNullException.ThrowIfNull( builder );

        if ( apiVersion == ApiVersions.V1 )
        {
            builder.EntitySet<Record>( "Records" ).EntityType.HasKey( r => new { r.Id, r.Source } );
        }
    }
}