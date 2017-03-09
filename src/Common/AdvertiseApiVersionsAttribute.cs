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
    /// Represents the metadata that describes the advertised <see cref="ApiVersion">API versions</see> for an ASP.NET controllers.
    /// </summary>
    /// <remarks>Advertised service API versions indicate the existence of other versioned services, but the implementation of those
    /// services are implemented elsewhere.</remarks>
    [AttributeUsage( Class, AllowMultiple = true, Inherited = false )]
    [SuppressMessage( "Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments", Justification = "An accessor property is provided, but the values are typed; not strings." )]
    [SuppressMessage( "Microsoft.Performance", "CA1813:AvoidUnsealedAttributes", Justification = "Allows extensibility." )]
    public partial class AdvertiseApiVersionsAttribute : ApiVersionsBaseAttribute, IApiVersionProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AdvertiseApiVersionsAttribute"/> class.
        /// </summary>
        /// <param name="version">The <see cref="ApiVersion">API version</see>.</param>
        protected AdvertiseApiVersionsAttribute( ApiVersion version ) : base( version ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="AdvertiseApiVersionsAttribute"/> class.
        /// </summary>
        /// <param name="versions">An <see cref="Array">array</see> of <see cref="ApiVersion">API versions</see>.</param>
        protected AdvertiseApiVersionsAttribute( params ApiVersion[] versions ) : base( versions ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="AdvertiseApiVersionsAttribute"/> class.
        /// </summary>
        /// <param name="version">The API version string.</param>
        public AdvertiseApiVersionsAttribute( string version ) : base( version ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="AdvertiseApiVersionsAttribute"/> class.
        /// </summary>
        /// <param name="versions">An <see cref="Array">array</see> of API version strings.</param>
        [CLSCompliant( false )]
        public AdvertiseApiVersionsAttribute( params string[] versions ) : base( versions ) { }

        bool IApiVersionProvider.AdvertiseOnly => true;

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
        public override int GetHashCode()
        {
            var hashCode = ( base.GetHashCode() * 397 ) ^ Deprecated.GetHashCode();
            hashCode = ( hashCode * 397 ) ^ 1;
            return hashCode;
        }
    }
}