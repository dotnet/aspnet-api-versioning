// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OData.Configuration;

using Asp.Versioning.OData.Models;
using Microsoft.OData.ModelBuilder;

public class PersonModelConfiguration : IModelConfiguration
{
    private static void ConfigureV1( ODataModelBuilder builder )
    {
        var person = ConfigureCurrent( builder );
        person.Ignore( p => p.Email );
        person.Ignore( p => p.Phone );
    }

    private static void ConfigureV2( ODataModelBuilder builder ) => ConfigureCurrent( builder ).Ignore( p => p.Phone );

    private static EntityTypeConfiguration<Person> ConfigureCurrent( ODataModelBuilder builder )
    {
        var person = builder.EntitySet<Person>( "People" ).EntityType;
        person.HasKey( p => p.Id );
        return person;
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