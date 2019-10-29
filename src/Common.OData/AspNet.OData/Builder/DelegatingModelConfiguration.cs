namespace Microsoft.AspNet.OData.Builder
{
#if WEBAPI
    using Microsoft.Web.Http;
#else
    using Microsoft.AspNetCore.Mvc;
#endif
    using System;

    sealed class DelegatingModelConfiguration : IModelConfiguration
    {
        readonly Action<ODataModelBuilder, ApiVersion> action;

        internal DelegatingModelConfiguration( Action<ODataModelBuilder, ApiVersion> action ) => this.action = action;

        public void Apply( ODataModelBuilder builder, ApiVersion apiVersion ) => action( builder, apiVersion );
    }
}