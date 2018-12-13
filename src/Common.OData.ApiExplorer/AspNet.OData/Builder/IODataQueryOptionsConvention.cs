namespace Microsoft.AspNet.OData.Builder
{
#if WEBAPI
    using System;
    using System.Web.Http.Description;
#else
    using Microsoft.AspNetCore.Mvc.ApiExplorer;
    using System;
#endif

    /// <summary>
    /// Defines the behavior of an OData query options convention.
    /// </summary>
#if !WEBAPI
    [CLSCompliant( false )]
#endif
    public interface IODataQueryOptionsConvention
    {
        /// <summary>
        /// Applies the convention to the specified API description.
        /// </summary>
        /// <param name="apiDescription">The <see cref="ApiDescription">API description</see> to apply the convention to.</param>
        void ApplyTo( ApiDescription apiDescription );
    }
}