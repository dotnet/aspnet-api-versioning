#if WEBAPI
namespace Microsoft.Web.Http.Versioning
#else
namespace Microsoft.AspNetCore.Mvc.Versioning
#endif
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using static ApiVersion;

    /// <summary>
    /// Represents the base implementation for the metadata that describes the <see cref="ApiVersion">API versions</see> associated with a service.
    /// </summary>
    public abstract class ApiVersionsBaseAttribute : Attribute
    {
        readonly Lazy<int> computedHashCode;
        readonly Lazy<IReadOnlyList<ApiVersion>> versions;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiVersionsBaseAttribute"/> class.
        /// </summary>
        /// <param name="version">The <see cref="ApiVersion">API version</see>.</param>
        protected ApiVersionsBaseAttribute( ApiVersion version ) : this( new[] { version } ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiVersionsBaseAttribute"/> class.
        /// </summary>
        /// <param name="versions">An <see cref="Array">array</see> of <see cref="ApiVersion">API versions</see>.</param>
        protected ApiVersionsBaseAttribute( params ApiVersion[] versions )
        {
            computedHashCode = new Lazy<int>( () => ComputeHashCode( versions ) );
            this.versions = new Lazy<IReadOnlyList<ApiVersion>>( () => versions );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiVersionsBaseAttribute"/> class.
        /// </summary>
        /// <param name="version">The API version string.</param>
        public ApiVersionsBaseAttribute( string version ) : this( new[] { version } ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiVersionsBaseAttribute"/> class.
        /// </summary>
        /// <param name="versions">An <see cref="Array">array</see> of API version strings.</param>
        [CLSCompliant( false )]
        public ApiVersionsBaseAttribute( params string[] versions )
        {
            computedHashCode = new Lazy<int>( () => ComputeHashCode( Versions ) );
            this.versions = new Lazy<IReadOnlyList<ApiVersion>>( () => versions.Select( Parse ).Distinct().ToSortedReadOnlyList() );
        }

        static int ComputeHashCode( IEnumerable<ApiVersion> versions )
        {
            var hashCode = 0;

            using ( var iterator = versions.GetEnumerator() )
            {
                if ( !iterator.MoveNext() )
                {
                    return hashCode;
                }

                hashCode = iterator.Current.GetHashCode();

                unchecked
                {
                    while ( iterator.MoveNext() )
                    {
                        hashCode = ( hashCode * 397 ) ^ iterator.Current.GetHashCode();
                    }
                }
            }

            return hashCode;
        }

        /// <summary>
        /// Gets the API versions defined by the attribute.
        /// </summary>
        /// <value>A <see cref="IReadOnlyList{T}">read-only list</see> of <see cref="ApiVersion">API versions</see>.</value>
        public IReadOnlyList<ApiVersion> Versions => versions.Value;

        /// <summary>
        /// Returns a value indicating whether the specified object is equal to the current instance.
        /// </summary>
        /// <param name="obj">The <see cref="object">object</see> to be evaluated.</param>
        /// <returns>True if the current instance equals the specified object; otherwise, false.</returns>
        public override bool Equals( object? obj ) => ( obj is ApiVersionsBaseAttribute ) && GetHashCode() == obj.GetHashCode();

        /// <summary>
        /// Returns a hash code for the current instance.
        /// </summary>
        /// <returns>A hash code.</returns>
        public override int GetHashCode() => computedHashCode.Value;
    }
}