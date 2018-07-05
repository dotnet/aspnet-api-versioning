﻿namespace Microsoft.AspNet.OData.Builder
{
#if WEBAPI
    using Microsoft.Web.Http;
#else
    using Microsoft.AspNetCore.Mvc;
    using System;
#endif

    /// <summary>
    /// Defines the behavior of a model configuration.
    /// </summary>
#if !WEBAPI
    [CLSCompliant( false )]
#endif
    public interface IModelConfiguration
    {
        /// <summary>
        /// Applies model configurations using the provided builder for the specified API version.
        /// </summary>
        /// <param name="builder">The <see cref="ODataModelBuilder">builder</see> used to apply configurations.</param>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> associated with the <paramref name="builder"/>.</param>
        void Apply( ODataModelBuilder builder, ApiVersion apiVersion );
    }
}