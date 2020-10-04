namespace Microsoft.Web.Http.Simulators.Configuration
{
    using Microsoft.AspNet.OData.Builder;
    using Microsoft.Web.Http;
    using Models;

    public class PersonModelConfiguration : IModelConfiguration
    {
        public void Apply( ODataModelBuilder builder, ApiVersion apiVersion, string routePrefix ) => builder.EntitySet<Person>( "People" );
    }
}