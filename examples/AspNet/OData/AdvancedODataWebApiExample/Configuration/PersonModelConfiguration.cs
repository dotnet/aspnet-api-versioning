namespace ApiVersioning.Examples.Configuration;

using ApiVersioning.Examples.Models;
using Asp.Versioning;
using Asp.Versioning.OData;
using Microsoft.AspNet.OData.Builder;

public class PersonModelConfiguration : IModelConfiguration
{
    private void ConfigureV1( ODataModelBuilder builder )
    {
        var person = ConfigureCurrent( builder );
        person.Ignore( p => p.Email );
        person.Ignore( p => p.Phone );
    }

    private void ConfigureV2( ODataModelBuilder builder ) => ConfigureCurrent( builder ).Ignore( p => p.Phone );

    private EntityTypeConfiguration<Person> ConfigureCurrent( ODataModelBuilder builder )
    {
        var person = builder.EntitySet<Person>( "People" ).EntityType;

        person.HasKey( p => p.Id );

        return person;
    }

    public void Apply( ODataModelBuilder builder, ApiVersion apiVersion, string routePrefix )
    {
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