﻿namespace Microsoft.AspNet.OData.Builder
{
#if WEBAPI
    using Microsoft.Web.Http;
#else
    using Microsoft.AspNetCore.Mvc;
#endif
    using System;
    using System.Diagnostics.Contracts;

    sealed class DelegatingModelConfiguration : IModelConfiguration
    {
        readonly Action<ODataModelBuilder, ApiVersion> action;

        internal DelegatingModelConfiguration( Action<ODataModelBuilder, ApiVersion> action )
        {
            Contract.Requires( action != null );
            this.action = action;
        }

        public void Apply( ODataModelBuilder builder, ApiVersion apiVersion ) => action( builder, apiVersion );
    }
}