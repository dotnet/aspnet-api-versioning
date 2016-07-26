namespace Microsoft.Web.OData.Builder
{
    using Http;
    using System;
    using System.Diagnostics.Contracts;
    using System.Web.OData.Builder;

    internal sealed class DelegatingModelConfiguration : IModelConfiguration
    {
        private readonly Action<ODataModelBuilder, ApiVersion> action;

        internal DelegatingModelConfiguration( Action<ODataModelBuilder, ApiVersion> action )
        {
            Contract.Requires( action != null );
            this.action = action;
        }

        public void Apply( ODataModelBuilder builder, ApiVersion apiVersion ) => action( builder, apiVersion );
    }
}
