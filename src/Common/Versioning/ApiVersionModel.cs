#if WEBAPI
namespace Microsoft.Web.Http.Versioning
#else
namespace Microsoft.AspNetCore.Mvc.Versioning
#endif
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
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
#if WEBAPI
        static readonly IReadOnlyList<ApiVersion> emptyVersions = new ApiVersion[0];
#else
        static readonly IReadOnlyList<ApiVersion> emptyVersions = Array.Empty<ApiVersion>();
#endif
        static readonly IReadOnlyList<ApiVersion> defaultVersions = new[] { ApiVersion.Default };

        ApiVersionModel( int kind )
        {
            switch ( kind )
            {
                case DefaultModel:
                    DeclaredApiVersions = defaultVersions;
                    ImplementedApiVersions = defaultVersions;
                    SupportedApiVersions = defaultVersions;
                    DeprecatedApiVersions = emptyVersions;
                    break;
                case NeutralModel:
                    IsApiVersionNeutral = true;
                    goto case EmptyModel;
                case EmptyModel:
                    DeclaredApiVersions = emptyVersions;
                    ImplementedApiVersions = emptyVersions;
                    SupportedApiVersions = emptyVersions;
                    DeprecatedApiVersions = emptyVersions;
                    break;
                default:
                    throw new ArgumentException( $"The kind {kind} is not supported." );
            }
        }

        internal ApiVersionModel( ApiVersionModel original, IReadOnlyList<ApiVersion> implemented, IReadOnlyList<ApiVersion> supported, IReadOnlyList<ApiVersion> deprecated )
        {
            if ( IsApiVersionNeutral = implemented.Count == 0 )
            {
                DeclaredApiVersions = emptyVersions;
                ImplementedApiVersions = emptyVersions;
                SupportedApiVersions = emptyVersions;
                DeprecatedApiVersions = emptyVersions;
            }
            else
            {
                DeclaredApiVersions = original.DeclaredApiVersions;
                ImplementedApiVersions = implemented;
                SupportedApiVersions = supported;
                DeprecatedApiVersions = deprecated;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiVersionModel"/> class.
        /// </summary>
        /// <param name="declaredVersion">A single, declared <see cref="ApiVersion">API version</see>.</param>
        /// <remarks>The declared version also represents the only implemented and supported API version.</remarks>
        public ApiVersionModel( ApiVersion declaredVersion )
        {
            DeclaredApiVersions = new[] { declaredVersion };
            ImplementedApiVersions = DeclaredApiVersions;
            SupportedApiVersions = DeclaredApiVersions;
            DeprecatedApiVersions = emptyVersions;
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
            DeclaredApiVersions = emptyVersions;
            ImplementedApiVersions = supportedVersions.Union( deprecatedVersions ).Distinct().ToSortedReadOnlyList();
            SupportedApiVersions = supportedVersions.ToSortedReadOnlyList();
            DeprecatedApiVersions = deprecatedVersions.ToSortedReadOnlyList();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiVersionModel"/> class.
        /// </summary>
        /// <param name="supportedVersions">The supported <see cref="IEnumerable{T}">sequence</see> of <see cref="ApiVersion">API versions</see> on a controller.</param>
        /// <param name="deprecatedVersions">The deprecated <see cref="IEnumerable{T}">sequence</see> of <see cref="ApiVersion">API versions</see> on a controller.</param>
        /// <param name="advertisedVersions">The advertised <see cref="IEnumerable{T}">sequence</see> of <see cref="ApiVersion">API versions</see> on a controller.</param>
        /// <param name="deprecatedAdvertisedVersions">The deprecated, advertised <see cref="IEnumerable{T}">sequence</see> of <see cref="ApiVersion">API versions</see> on a controller.</param>
        public ApiVersionModel(
            IEnumerable<ApiVersion> supportedVersions,
            IEnumerable<ApiVersion> deprecatedVersions,
            IEnumerable<ApiVersion> advertisedVersions,
            IEnumerable<ApiVersion> deprecatedAdvertisedVersions )
            : this( supportedVersions.Union( deprecatedVersions ), supportedVersions, deprecatedVersions, advertisedVersions, deprecatedAdvertisedVersions ) { }

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
            DeclaredApiVersions = declaredVersions.ToSortedReadOnlyList();
            SupportedApiVersions = supportedVersions.Union( advertisedVersions ).ToSortedReadOnlyList();
            DeprecatedApiVersions = deprecatedVersions.Union( deprecatedAdvertisedVersions ).ToSortedReadOnlyList();
            ImplementedApiVersions = SupportedApiVersions.Union( DeprecatedApiVersions ).ToSortedReadOnlyList();
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
        public IReadOnlyList<ApiVersion> DeclaredApiVersions { get; }

        /// <summary>
        /// Gets the API versions implemented by the controller or action.
        /// </summary>
        /// <value>A <see cref="IReadOnlyList{T}">read-only list</see> of <see cref="ApiVersion">API versions</see>
        /// implemented by the controller or action.</value>
        /// <remarks>The implemented API versions include the supported and deprecated API versions.</remarks>
        public IReadOnlyList<ApiVersion> ImplementedApiVersions { get; }

        /// <summary>
        /// Gets the API versions supported by the controller.
        /// </summary>
        /// <value>A <see cref="IReadOnlyList{T}">read-only list</see> of <see cref="ApiVersion">API versions</see>
        /// supported by the controller.</value>
        public IReadOnlyList<ApiVersion> SupportedApiVersions { get; }

        /// <summary>
        /// Gets the API versions deprecated by the controller.
        /// </summary>
        /// <value>A <see cref="IReadOnlyList{T}">read-only list</see> of <see cref="ApiVersion">API versions</see>
        /// deprecated by the controller.</value>
        /// <remarks>A deprecated API version does not mean it is not supported by the controller. A deprecated API
        /// version is typically advertised six months or more before it becomes unsupported; in which case, the
        /// controller would no longer indicate that it is an <see cref="ImplementedApiVersions">implemented version</see>.</remarks>
        public IReadOnlyList<ApiVersion> DeprecatedApiVersions { get; }
    }
}