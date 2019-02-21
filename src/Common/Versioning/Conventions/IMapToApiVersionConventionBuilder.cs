#if WEBAPI
namespace Microsoft.Web.Http.Versioning.Conventions
#else
namespace Microsoft.AspNetCore.Mvc.Versioning.Conventions
#endif
{
    using System;

    /// <summary>
    /// Defines the behavior of convention builder that builds mapped API versions.
    /// </summary>
    public interface IMapToApiVersionConventionBuilder : IDeclareApiVersionConventionBuilder
    {
        /// <summary>
        /// Maps the specified API version to the configured controller action.
        /// </summary>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> to map to the action.</param>
        void MapToApiVersion( ApiVersion apiVersion );
    }
}