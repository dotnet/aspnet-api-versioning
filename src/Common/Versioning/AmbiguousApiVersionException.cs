#if WEBAPI
namespace Microsoft.Web.Http.Versioning
#else
namespace Microsoft.AspNetCore.Mvc.Versioning
#endif
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;
    using static System.Runtime.CompilerServices.MethodImplOptions;

    /// <summary>
    /// Represents the exception thrown when multiple, different API versions specified in a single request.
    /// </summary>
    [Serializable]
    public class AmbiguousApiVersionException : Exception
    {
        readonly string[] apiVersions;

        /// <summary>
        /// Initializes a new instance of the <see cref="AmbiguousApiVersionException"/> class.
        /// </summary>
        public AmbiguousApiVersionException() => apiVersions = EmptyArray();

        /// <summary>
        /// Initializes a new instance of the <see cref="AmbiguousApiVersionException"/> class.
        /// </summary>
        /// <param name="message">The associated error message.</param>
        public AmbiguousApiVersionException( string message ) : base( message ) => apiVersions = EmptyArray();

        /// <summary>
        /// Initializes a new instance of the <see cref="AmbiguousApiVersionException"/> class.
        /// </summary>
        /// <param name="message">The associated error message.</param>
        /// <param name="innerException">The inner <see cref="Exception">exception</see> that caused the current exception, if any.</param>
        public AmbiguousApiVersionException( string message, Exception innerException )
            : base( message, innerException ) => apiVersions = EmptyArray();

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

        /// <summary>
        /// Initializes a new instance of the <see cref="AmbiguousApiVersionException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo">serialization info</see> the exception is being deserialized with.</param>
        /// <param name="context">The <see cref="StreamingContext">streaming context</see> the exception is being deserialized from.</param>
        protected AmbiguousApiVersionException( SerializationInfo info, StreamingContext context )
            : base( info, context ) => apiVersions = (string[]) info.GetValue( nameof( apiVersions ), typeof( string[] ) )!;

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

        [MethodImpl( AggressiveInlining )]
#if WEBAPI
        static string[] EmptyArray() => new string[0];
#else
        static string[] EmptyArray() => Array.Empty<string>();
#endif
    }
}