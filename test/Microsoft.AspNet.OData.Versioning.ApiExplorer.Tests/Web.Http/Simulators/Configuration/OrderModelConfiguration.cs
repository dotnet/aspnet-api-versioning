namespace Microsoft.Web.Http.Simulators.Configuration
{
    using Microsoft.AspNet.OData.Builder;
    using Microsoft.Web.Http;
    using Models;

    public class OrderModelConfiguration : IModelConfiguration
    {
        public void Apply( ODataModelBuilder builder, ApiVersion apiVersion ) => builder.EntitySet<Order>( "Orders" );
    }
}