#if WEBAPI
namespace Microsoft.Web.Http
#else
namespace Microsoft.AspNetCore.Mvc
#endif
{
#if WEBAPI
    using Microsoft.Web.Http.Versioning;
#else
    using Microsoft.AspNetCore.Mvc.Versioning;
#endif
    using System;
    using static System.AttributeTargets;

    /// <summary>
    /// Represents the metadata that describes the <see cref="ApiVersion">API version</see>-specific implementation of a service.
    /// </summary>
#if !WEBAPI
    [CLSCompliant( false )]
#endif
    [AttributeUsage( Method, AllowMultiple = true, Inherited = false )]
    public class MapToApiVersionAttribute : ApiVersionsBaseAttribute, IApiVersionProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MapToApiVersionAttribute"/> class.
        /// </summary>
        /// <param name="version">The <see cref="ApiVersion">API version</see>.</param>
        protected MapToApiVersionAttribute( ApiVersion version ) : base( version ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MapToApiVersionAttribute"/> class.
        /// </summary>
        /// <param name="version">The API version string.</param>
        public MapToApiVersionAttribute( string version ) : base( version ) { }

#pragma warning disable CA1033 // Interface methods should be callable by child types
        ApiVersionProviderOptions IApiVersionProvider.Options => ApiVersionProviderOptions.Mapped;
#pragma warning restore CA1033 // Interface methods should be callable by child types
    }
}