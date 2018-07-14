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
    using System.Diagnostics.CodeAnalysis;
    using static System.AttributeTargets;

    /// <summary>
    /// Represents the metadata that describes the <see cref="ApiVersion">API versions</see> associated with a service.
    /// </summary>
    [AttributeUsage( Class, AllowMultiple = true, Inherited = false )]
    [SuppressMessage( "Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments", Justification = "An accessor property is provided, but the values are typed; not strings." )]
    [SuppressMessage( "Microsoft.Performance", "CA1813:AvoidUnsealedAttributes", Justification = "Allows extensibility." )]
    public partial class ApiVersionAttribute : ApiVersionsBaseAttribute, IApiVersionProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApiVersionAttribute"/> class.
        /// </summary>
        /// <param name="version">The <see cref="ApiVersion">API version</see>.</param>
        protected ApiVersionAttribute( ApiVersion version ) : base( version ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiVersionAttribute"/> class.
        /// </summary>
        /// <param name="version">The API version string.</param>
        public ApiVersionAttribute( string version ) : base( version ) { }

#pragma warning disable CA1033 // Interface methods should be callable by child types
        bool IApiVersionProvider.AdvertiseOnly => false;
#pragma warning restore CA1033 // Interface methods should be callable by child types

        /// <summary>
        /// Gets or sets a value indicating whether the specified set of API versions are deprecated.
        /// </summary>
        /// <value>True if the specified set of API versions are deprecated; otherwise, false.
        /// The default value is <c>false</c>.</value>
        public bool Deprecated { get; set; }

        /// <summary>
        /// Returns a hash code for the current instance.
        /// </summary>
        /// <returns>A hash code.</returns>
        public override int GetHashCode() => ( base.GetHashCode() * 397 ) ^ Deprecated.GetHashCode();
    }
}