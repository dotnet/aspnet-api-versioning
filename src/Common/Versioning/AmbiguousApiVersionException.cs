#if WEBAPI
namespace Microsoft.Web.Http.Versioning
#else
namespace Microsoft.AspNetCore.Mvc.Versioning
#endif
{
    // disable warnings for false positives targeting netstandard2.0
#pragma warning disable CA1032 // Implement standard exception constructors
#pragma warning disable CA2235 // Mark all non-serializable fields

    using System;
    using System.Collections.Generic;
    using System.Linq;
#if NET451 || WEBAPI
    using System.Runtime.Serialization;
#endif

    /// <summary>
    /// Represents the exception thrown when multiple, different API versions specified in a single request.
    /// </summary>
#if NET451 || WEBAPI
    [Serializable]
#endif
    public class AmbiguousApiVersionException : Exception
    {
        readonly string[] apiVersions;

        /// <summary>
        /// Initializes a new instance of the <see cref="AmbiguousApiVersionException"/> class.
        /// </summary>
        /// <param name="message">The associated error message.</param>
        /// <param name="apiVersions">The <see cref="IEnumerable{T}">sequence</see> of ambiguous API versions.</param>
        public AmbiguousApiVersionException( string message, IEnumerable<string> apiVersions )
            : base( message ) => this.apiVersions = apiVersions.ToArray();

        /// <summary>
        /// Initializes a new instance of the <see cref="AmbiguousApiVersionException"/> class.
        /// </summary>
        /// <param name="message">The associated error message.</param>
        /// <param name="apiVersions">The <see cref="IEnumerable{T}">sequence</see> of ambiguous API versions.</param>
        /// <param name="innerException">The inner <see cref="Exception">exception</see> that caused the current exception, if any.</param>
        public AmbiguousApiVersionException( string message, IEnumerable<string> apiVersions, Exception innerException )
            : base( message, innerException ) => this.apiVersions = apiVersions.ToArray();

        /// <summary>
        /// Gets a read-only list of the ambiguous API versions.
        /// </summary>
        /// <value>A <see cref="IReadOnlyList{T}">read-only list</see> of unparsed, ambiguous API versions.</value>
        public IReadOnlyList<string> ApiVersions => apiVersions;
#if NET451 || WEBAPI
        /// <summary>
        /// Initializes a new instance of the <see cref="AmbiguousApiVersionException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo">serialization info</see> the exception is being deserialized with.</param>
        /// <param name="context">The <see cref="StreamingContext">streaming context</see> the exception is being deserialized from.</param>
        protected AmbiguousApiVersionException( SerializationInfo info, StreamingContext context )
            : base( info, context ) => apiVersions = (string[]) info.GetValue( nameof( apiVersions ), typeof( string[] ) );

        /// <summary>
        /// Gets information about the exception being serialized.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo">serialization info</see> the exception is being serialized with.</param>
        /// <param name="context">The <see cref="StreamingContext">streaming context</see> the exception is being serialized in.</param>
        public override void GetObjectData( SerializationInfo info, StreamingContext context )
        {
            base.GetObjectData( info, context );
            info.AddValue( nameof( apiVersions ), apiVersions );
        }
#endif
    }
}