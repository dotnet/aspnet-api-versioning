namespace Microsoft.Web.Http.Simulators.Configuration
{
    using Microsoft.Web.Http;
    using Microsoft.Web.OData.Builder;
    using Models;
    using System.Web.OData.Builder;

    public class OrderModelConfiguration : IModelConfiguration
    {
        public void Apply( ODataModelBuilder builder, ApiVersion apiVersion ) => builder.EntitySet<Order>( "Orders" );
    }
}