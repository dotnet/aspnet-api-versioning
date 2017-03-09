namespace Microsoft.Web.OData.Configuration
{
    using Microsoft.Web.Http;
    using Microsoft.Web.OData.Builder;
    using Models;
    using System.Web.OData.Builder;

    public class PersonModelConfiguration : IModelConfiguration
    {
        void ConfigureV1( ODataModelBuilder builder )
        {
            var person = ConfigureCurrent( builder );
            person.Ignore( p => p.Email );
            person.Ignore( p => p.Phone );
        }

        void ConfigureV2( ODataModelBuilder builder ) => ConfigureCurrent( builder ).Ignore( p => p.Phone );

        EntityTypeConfiguration<Person> ConfigureCurrent( ODataModelBuilder builder )
        {
            var person = builder.EntitySet<Person>( "People" ).EntityType;
            person.HasKey( p => p.Id );
            return person;
        }

        public void Apply( ODataModelBuilder builder, ApiVersion apiVersion )
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
}