namespace Microsoft.AspNet.OData.Simulators.Configuration
{
    using Microsoft.AspNet.OData.Builder;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Represents the model configuration for all configurations.
    /// </summary>
    public class AllConfigurations : IModelConfiguration
    {
        /// <summary>
        /// Applies model configurations using the provided builder for the specified API version.
        /// </summary>
        /// <param name="builder">The <see cref="ODataModelBuilder">builder</see> used to apply configurations.</param>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> associated with the <paramref name="builder"/>.</param>
        public void Apply( ODataModelBuilder builder, ApiVersion apiVersion )
        {
            builder.Function( "GetSalesTaxRate" ).Returns<double>().Parameter<int>( "PostalCode" );
        }
    }
}