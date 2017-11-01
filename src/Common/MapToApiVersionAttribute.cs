#if WEBAPI
namespace Microsoft.Web.Http
#else
namespace Microsoft.AspNetCore.Mvc
#endif
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Versioning;
    using static System.AttributeTargets;

    /// <summary>
    /// Represents the metadata that describes the <see cref="ApiVersion">API version</see>-specific implementation of a service.
    /// </summary>
    [AttributeUsage( Method, AllowMultiple = true, Inherited = false )]
    [SuppressMessage( "Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments", Justification = "An accessor property is provided, but the values are typed; not strings." )]
    [SuppressMessage( "Microsoft.Performance", "CA1813:AvoidUnsealedAttributes", Justification = "Allows extensibility." )]
    public partial class MapToApiVersionAttribute : ApiVersionsBaseAttribute, IApiVersionProvider
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
        bool IApiVersionProvider.AdvertiseOnly => false;

        bool IApiVersionProvider.Deprecated => false;
#pragma warning restore CA1033 // Interface methods should be callable by child types
    }
}