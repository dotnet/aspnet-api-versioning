#if WEBAPI
namespace Microsoft.Web.Http.Versioning
#else
namespace Microsoft.AspNetCore.Mvc.Versioning
#endif
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using System.Linq;

    /// <summary>
    /// Represents the API version information for an ASP.NET controller or action.
    /// </summary>
    [DebuggerDisplay( "{DebuggerDisplayText}" )]
    [DebuggerTypeProxy( typeof( ApiVersionModelDebugView ) )]
    public sealed partial class ApiVersionModel
    {
        const int DefaultModel = 0;
        const int NeutralModel = 1;
        const int EmptyModel = 2;
        static readonly Lazy<ApiVersionModel> defaultVersion = new Lazy<ApiVersionModel>( () => new ApiVersionModel( DefaultModel ) );
        static readonly Lazy<ApiVersionModel> neutralVersion = new Lazy<ApiVersionModel>( () => new ApiVersionModel( NeutralModel ) );
        static readonly Lazy<ApiVersionModel> emptyVersion = new Lazy<ApiVersionModel>( () => new ApiVersionModel( EmptyModel ) );
        static readonly Lazy<IReadOnlyList<ApiVersion>> emptyVersions = new Lazy<IReadOnlyList<ApiVersion>>( () => new ApiVersion[0] );
        static readonly Lazy<IReadOnlyList<ApiVersion>> defaultVersions = new Lazy<IReadOnlyList<ApiVersion>>( () => new[] { ApiVersion.Default } );
        readonly Lazy<IReadOnlyList<ApiVersion>> declaredVersions;
        readonly Lazy<IReadOnlyList<ApiVersion>> implementedVersions;
        readonly Lazy<IReadOnlyList<ApiVersion>> supportedVersions;
        readonly Lazy<IReadOnlyList<ApiVersion>> deprecatedVersions;

        ApiVersionModel( int kind )
        {
            switch ( kind )
            {
                case DefaultModel:
                    declaredVersions = defaultVersions;
                    implementedVersions = defaultVersions;
                    supportedVersions = defaultVersions;
                    deprecatedVersions = emptyVersions;
                    break;
                case NeutralModel:
                    IsApiVersionNeutral = true;
                    goto case EmptyModel;
                case EmptyModel:
                    declaredVersions = emptyVersions;
                    implementedVersions = emptyVersions;
                    supportedVersions = emptyVersions;
                    deprecatedVersions = emptyVersions;
                    break;
            }
        }

        internal ApiVersionModel( ApiVersionModel original, IReadOnlyList<ApiVersion> implemented, IReadOnlyList<ApiVersion> supported, IReadOnlyList<ApiVersion> deprecated )
        {
            Contract.Requires( original != null );
            Contract.Requires( implemented != null );
            Contract.Requires( supported != null );
            Contract.Requires( deprecated != null );

            if ( IsApiVersionNeutral = ( implemented.Count == 0 ) )
            {
                declaredVersions = emptyVersions;
                implementedVersions = emptyVersions;
                supportedVersions = emptyVersions;
                deprecatedVersions = emptyVersions;
            }
            else
            {
                declaredVersions = original.declaredVersions;
                implementedVersions = new Lazy<IReadOnlyList<ApiVersion>>( () => implemented );
                supportedVersions = new Lazy<IReadOnlyList<ApiVersion>>( () => supported );
                deprecatedVersions = new Lazy<IReadOnlyList<ApiVersion>>( () => deprecated );
            }
        }

        internal ApiVersionModel(
            bool apiVersionNeutral,
            IEnumerable<ApiVersion> supported,
            IEnumerable<ApiVersion> deprecated,
            IEnumerable<ApiVersion> advertised,
            IEnumerable<ApiVersion> deprecatedAdvertised )
        {
            Contract.Requires( supported != null );
            Contract.Requires( deprecated != null );
            Contract.Requires( advertised != null );
            Contract.Requires( deprecatedAdvertised != null );

            if ( IsApiVersionNeutral = apiVersionNeutral )
            {
                declaredVersions = emptyVersions;
                implementedVersions = emptyVersions;
                supportedVersions = emptyVersions;
                deprecatedVersions = emptyVersions;
            }
            else
            {
                declaredVersions = new Lazy<IReadOnlyList<ApiVersion>>( supported.Union( deprecated ).ToSortedReadOnlyList );
                supportedVersions = new Lazy<IReadOnlyList<ApiVersion>>( supported.Union( advertised ).ToSortedReadOnlyList );
                deprecatedVersions = new Lazy<IReadOnlyList<ApiVersion>>( deprecated.Union( deprecatedAdvertised ).ToSortedReadOnlyList );
                implementedVersions = new Lazy<IReadOnlyList<ApiVersion>>( () => supportedVersions.Value.Union( deprecatedVersions.Value ).ToSortedReadOnlyList() );
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiVersionModel"/> class.
        /// </summary>
        /// <param name="declaredVersion">A single, declared <see cref="ApiVersion">API version</see>.</param>
        /// <remarks>The declared version also represents the only implemented and supported API version.</remarks>
        public ApiVersionModel( ApiVersion declaredVersion )
        {
            Arg.NotNull( declaredVersion, nameof( declaredVersion ) );

            declaredVersions = new Lazy<IReadOnlyList<ApiVersion>>( () => new[] { declaredVersion } );
            implementedVersions = declaredVersions;
            supportedVersions = declaredVersions;
            deprecatedVersions = emptyVersions;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiVersionModel"/> class.
        /// </summary>
        /// <param name="supportedVersions">A <see cref="IEnumerable{T}">sequence</see> of supported <see cref="ApiVersion">API versions</see>.</param>
        /// <param name="deprecatedVersions">A <see cref="IEnumerable{T}">sequence</see> of deprecated <see cref="ApiVersion">API versions</see>.</param>
        /// <remarks>The constructed <see cref="ApiVersionModel">API version information</see> is never version-neutral,
        /// <see cref="ImplementedApiVersions">implemented API versions</see> are a union between the
        /// <paramref name="supportedVersions">supported versions</paramref> and <paramref name="deprecatedVersions">deprecated versions</paramref>,
        /// and the <see cref="DeclaredApiVersions">declared API versions</see> are always empty since no controller or action has been specified.</remarks>
        public ApiVersionModel( IEnumerable<ApiVersion> supportedVersions, IEnumerable<ApiVersion> deprecatedVersions )
        {
            Arg.NotNull( supportedVersions, nameof( supportedVersions ) );
            Arg.NotNull( deprecatedVersions, nameof( deprecatedVersions ) );

            declaredVersions = emptyVersions;
            implementedVersions = new Lazy<IReadOnlyList<ApiVersion>>( supportedVersions.Union( deprecatedVersions ).Distinct().ToSortedReadOnlyList );
            this.supportedVersions = new Lazy<IReadOnlyList<ApiVersion>>( supportedVersions.ToSortedReadOnlyList );
            this.deprecatedVersions = new Lazy<IReadOnlyList<ApiVersion>>( deprecatedVersions.ToSortedReadOnlyList );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiVersionModel"/> class.
        /// </summary>
        /// <param name="declaredVersions">The declared <see cref="IEnumerable{T}">sequence</see> of <see cref="ApiVersion">API versions</see> on a controller or action.</param>
        /// <param name="supportedVersions">The supported <see cref="IEnumerable{T}">sequence</see> of <see cref="ApiVersion">API versions</see> on a controller.</param>
        /// <param name="deprecatedVersions">The deprecated <see cref="IEnumerable{T}">sequence</see> of <see cref="ApiVersion">API versions</see> on a controller.</param>
        /// <param name="advertisedVersions">The advertised <see cref="IEnumerable{T}">sequence</see> of <see cref="ApiVersion">API versions</see> on a controller.</param>
        /// <param name="deprecatedAdvertisedVersions">The deprecated, advertised <see cref="IEnumerable{T}">sequence</see> of <see cref="ApiVersion">API versions</see> on a controller.</param>
        public ApiVersionModel(
            IEnumerable<ApiVersion> declaredVersions,
            IEnumerable<ApiVersion> supportedVersions,
            IEnumerable<ApiVersion> deprecatedVersions,
            IEnumerable<ApiVersion> advertisedVersions,
            IEnumerable<ApiVersion> deprecatedAdvertisedVersions )
        {
            Arg.NotNull( declaredVersions, nameof( declaredVersions ) );
            Arg.NotNull( supportedVersions, nameof( supportedVersions ) );
            Arg.NotNull( deprecatedVersions, nameof( deprecatedVersions ) );
            Arg.NotNull( advertisedVersions, nameof( advertisedVersions ) );
            Arg.NotNull( deprecatedAdvertisedVersions, nameof( deprecatedAdvertisedVersions ) );

            this.declaredVersions = new Lazy<IReadOnlyList<ApiVersion>>( declaredVersions.ToSortedReadOnlyList );
            this.supportedVersions = new Lazy<IReadOnlyList<ApiVersion>>( supportedVersions.Union( advertisedVersions ).ToSortedReadOnlyList );
            this.deprecatedVersions = new Lazy<IReadOnlyList<ApiVersion>>( deprecatedVersions.Union( deprecatedAdvertisedVersions ).ToSortedReadOnlyList );
            implementedVersions = new Lazy<IReadOnlyList<ApiVersion>>( () => this.supportedVersions.Value.Union( this.deprecatedVersions.Value ).ToSortedReadOnlyList() );
        }

        string DebuggerDisplayText => IsApiVersionNeutral ? "*.*" : string.Join( ", ", DeclaredApiVersions );

        /// <summary>
        /// Gets the default API version information.
        /// </summary>
        /// <value>The default <see cref="ApiVersionModel">API version information</see>.</value>
        public static ApiVersionModel Default => defaultVersion.Value;

        /// <summary>
        /// Gets the neutral API version information.
        /// </summary>
        /// <value>The neutral <see cref="ApiVersionModel">API version information</see>.</value>
        public static ApiVersionModel Neutral => neutralVersion.Value;

        /// <summary>
        /// Gets empty API version information.
        /// </summary>
        /// <value>The empty <see cref="ApiVersionModel">API version information</see>.</value>
        public static ApiVersionModel Empty => emptyVersion.Value;

        /// <summary>
        /// Gets a value indicating whether the controller is API version neutral.
        /// </summary>
        /// <value>True if the controller is API version neutral (e.g. "unaware"); otherwise, false.</value>
        /// <remarks>A controller is API version neutral only if the <see cref="ApiVersionNeutralAttribute"/> has been applied.</remarks>
        public bool IsApiVersionNeutral { get; }

        /// <summary>
        /// Gets the API versions declared by the controller or action.
        /// </summary>
        /// <value>A <see cref="IReadOnlyList{T}">read-only list</see> of <see cref="ApiVersion">API versions</see>
        /// declared by the controller or action.</value>
        /// <remarks>The declared API versions are constrained to the versions declared explicitly by the specified controller or action.</remarks>
        public IReadOnlyList<ApiVersion> DeclaredApiVersions => declaredVersions.Value;

        /// <summary>
        /// Gets the API versions implemented by the controller or action.
        /// </summary>
        /// <value>A <see cref="IReadOnlyList{T}">read-only list</see> of <see cref="ApiVersion">API versions</see>
        /// implemented by the controller or action.</value>
        /// <remarks>The implemented API versions include the supported and deprecated API versions.</remarks>
        public IReadOnlyList<ApiVersion> ImplementedApiVersions => implementedVersions.Value;

        /// <summary>
        /// Gets the API versions supported by the controller.
        /// </summary>
        /// <value>A <see cref="IReadOnlyList{T}">read-only list</see> of <see cref="ApiVersion">API versions</see>
        /// supported by the controller.</value>
        public IReadOnlyList<ApiVersion> SupportedApiVersions => supportedVersions.Value;

        /// <summary>
        /// Gets the API versions deprecated by the controller.
        /// </summary>
        /// <value>A <see cref="IReadOnlyList{T}">read-only list</see> of <see cref="ApiVersion">API versions</see>
        /// deprecated by the controller.</value>
        /// <remarks>A deprecated API version does not mean it is not supported by the controller. A deprecated API
        /// version is typically advertised six months or more before it becomes unsupported; in which case, the
        /// controller would no longer indicate that it is an <see cref="ImplementedApiVersions">implemented version</see>.</remarks>
        public IReadOnlyList<ApiVersion> DeprecatedApiVersions => deprecatedVersions.Value;
    }
}