namespace Microsoft.AspNetCore.OData.Configuration
{
    using Microsoft.AspNet.OData.Builder;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.OData.Models;

    public class CustomerModelConfiguration : IModelConfiguration
    {
        void ConfigureV1( ODataModelBuilder builder )
        {
            var customer = ConfigureCurrent( builder );
            customer.Ignore( c => c.Email );
            customer.Ignore( c => c.Phone );
        }

        void ConfigureV2( ODataModelBuilder builder ) => ConfigureCurrent( builder ).Ignore( c => c.Phone );

        EntityTypeConfiguration<Customer> ConfigureCurrent( ODataModelBuilder builder )
        {
            var customer = builder.EntitySet<Customer>( "Customers" ).EntityType;
            customer.HasKey( p => p.Id );
            return customer;
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