namespace Microsoft.AspNet.OData.Simulators.Configuration
{
    using Microsoft.AspNet.OData.Builder;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Represents the model configuration for all configurations.
    /// </summary>
    public class AllConfigurations : IModelConfiguration
    {
        /// <inheritdoc />
        public void Apply( ODataModelBuilder builder, ApiVersion apiVersion, string routePrefix )
        {
            builder.Function( "GetSalesTaxRate" ).Returns<double>().Parameter<int>( "PostalCode" );
        }
    }
}