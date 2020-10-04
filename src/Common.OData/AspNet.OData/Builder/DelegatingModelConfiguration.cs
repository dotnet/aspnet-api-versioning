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
        readonly Action<ODataModelBuilder, ApiVersion, string?> action;

        internal DelegatingModelConfiguration( Action<ODataModelBuilder, ApiVersion, string?> action ) => this.action = action;

        public void Apply( ODataModelBuilder builder, ApiVersion apiVersion, string? routePrefix ) => action( builder, apiVersion, routePrefix );
    }
}