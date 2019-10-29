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
    /// Represents the metadata that describes the advertised <see cref="ApiVersion">API versions</see> for an ASP.NET controllers.
    /// </summary>
    /// <remarks>Advertised service API versions indicate the existence of other versioned services, but the implementation of those
    /// services are implemented elsewhere.</remarks>
    [AttributeUsage( Class | Method, AllowMultiple = true, Inherited = false )]
    public class AdvertiseApiVersionsAttribute : ApiVersionsBaseAttribute, IApiVersionProvider
    {
        ApiVersionProviderOptions options = ApiVersionProviderOptions.Advertised;

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

#pragma warning disable CA1033 // Interface methods should be callable by child types
        ApiVersionProviderOptions IApiVersionProvider.Options => options;
#pragma warning restore CA1033 // Interface methods should be callable by child types

        /// <summary>
        /// Gets or sets a value indicating whether the specified set of API versions are deprecated.
        /// </summary>
        /// <value>True if the specified set of API versions are deprecated; otherwise, false.
        /// The default value is <c>false</c>.</value>
        public bool Deprecated
        {
            get => ( options & ApiVersionProviderOptions.Deprecated ) == ApiVersionProviderOptions.Deprecated;
            set
            {
                if ( value )
                {
                    options |= ApiVersionProviderOptions.Deprecated;
                }
                else
                {
                    options &= ~ApiVersionProviderOptions.Deprecated;
                }
            }
        }

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