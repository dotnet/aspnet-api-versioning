#if WEBAPI
namespace Microsoft.Web.Http.Versioning.Conventions
#else
namespace Microsoft.AspNetCore.Mvc.Versioning.Conventions
#endif
{
    using System;

    /// <summary>
    /// Defines the behavior of an API version convention builder.
    /// </summary>
    public partial interface IApiVersionConventionBuilder
    {
        /// <summary>
        /// Indicates that the controller is API version-neutral.
        /// </summary>
        void IsApiVersionNeutral();

        /// <summary>
        /// Indicates that the specified API version is supported by the configured controller.
        /// </summary>
        /// <param name="apiVersion">The supported <see cref="ApiVersion">API version</see> implemented by the controller.</param>
        void HasApiVersion( ApiVersion apiVersion );

        /// <summary>
        /// Indicates that the specified API version is deprecated by the configured controller.
        /// </summary>
        /// <param name="apiVersion">The deprecated <see cref="ApiVersion">API version</see> implemented by the controller.</param>
        void HasDeprecatedApiVersion( ApiVersion apiVersion );

        /// <summary>
        /// Indicates that the specified API version is advertised by the configured controller.
        /// </summary>
        /// <param name="apiVersion">The advertised <see cref="ApiVersion">API version</see> not directly implemented by the controller.</param>
        void AdvertisesApiVersion( ApiVersion apiVersion );

        /// <summary>
        /// Indicates that the specified API version is advertised and deprecated by the configured controller.
        /// </summary>
        /// <param name="apiVersion">The advertised, but deprecated <see cref="ApiVersion">API version</see> not directly implemented by the controller.</param>
        void AdvertisesDeprecatedApiVersion( ApiVersion apiVersion );
    }
}