// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OData.Configuration;

using Asp.Versioning.OData.Models;
using Microsoft.OData.ModelBuilder;

public class CustomerModelConfiguration : IModelConfiguration
{
    private static void ConfigureV1( ODataModelBuilder builder )
    {
        var customer = ConfigureCurrent( builder );
        customer.Ignore( c => c.Email );
        customer.Ignore( c => c.Phone );
    }

    private static void ConfigureV2( ODataModelBuilder builder ) => ConfigureCurrent( builder ).Ignore( c => c.Phone );

    private static EntityTypeConfiguration<Customer> ConfigureCurrent( ODataModelBuilder builder )
    {
        var customer = builder.EntitySet<Customer>( "Customers" ).EntityType;
        customer.HasKey( p => p.Id );
        return customer;
    }

    public void Apply( ODataModelBuilder builder, ApiVersion apiVersion, string routePrefix )
    {
        ArgumentNullException.ThrowIfNull( builder );
        ArgumentNullException.ThrowIfNull( apiVersion );

        switch ( apiVersion.MajorVersion )
        {
            case 1:
                ConfigureV1( builder );
                break;
            case 2:
                ConfigureV2( builder );
                break;
            default:
                ConfigureCurrent( builder );
                break;
        }
    }
}