namespace ApiVersioning.Examples.Configuration;

using ApiVersioning.Examples.Models;
using Asp.Versioning;
using Asp.Versioning.OData;
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
        if ( routePrefix != "api" )
        {
            return;
        }

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