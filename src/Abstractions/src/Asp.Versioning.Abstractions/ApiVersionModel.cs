// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using System.Diagnostics;

/// <summary>
/// Represents the version information for an API.
/// </summary>
[DebuggerDisplay( "{DebuggerDisplayText}" )]
[DebuggerTypeProxy( typeof( ApiVersionModelDebugView ) )]
public sealed class ApiVersionModel
{
    private const int DefaultModel = 0;
    private const int NeutralModel = 1;
    private const int EmptyModel = 2;
    private static readonly IReadOnlyList<ApiVersion> emptyVersions = Array.Empty<ApiVersion>();
    private static readonly IReadOnlyList<ApiVersion> defaultVersions = new[] { ApiVersion.Default };
    private static ApiVersionModel? defaultVersion;
    private static ApiVersionModel? neutralVersion;
    private static ApiVersionModel? emptyVersion;

    private ApiVersionModel( int kind )
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
        if ( IsApiVersionNeutral = original.IsApiVersionNeutral || implemented.Count == 0 )
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
        SupportedApiVersions = supportedVersions.Distinct().OrderBy( v => v ).ToArray();
        DeprecatedApiVersions = deprecatedVersions.Distinct().OrderBy( v => v ).ToArray();
        ImplementedApiVersions = SupportedApiVersions.Union( DeprecatedApiVersions ).OrderBy( v => v ).ToArray();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiVersionModel"/> class.
    /// </summary>
    /// <param name="supportedVersions">The supported <see cref="IEnumerable{T}">sequence</see> of <see cref="ApiVersion">API versions</see>.</param>
    /// <param name="deprecatedVersions">The deprecated <see cref="IEnumerable{T}">sequence</see> of <see cref="ApiVersion">API versions</see>.</param>
    /// <param name="advertisedVersions">The advertised <see cref="IEnumerable{T}">sequence</see> of <see cref="ApiVersion">API versions</see>.</param>
    /// <param name="deprecatedAdvertisedVersions">The deprecated, advertised <see cref="IEnumerable{T}">sequence</see> of <see cref="ApiVersion">API versions</see>.</param>
    public ApiVersionModel(
        IEnumerable<ApiVersion> supportedVersions,
        IEnumerable<ApiVersion> deprecatedVersions,
        IEnumerable<ApiVersion> advertisedVersions,
        IEnumerable<ApiVersion> deprecatedAdvertisedVersions )
        : this( supportedVersions.Union( deprecatedVersions ), supportedVersions, deprecatedVersions, advertisedVersions, deprecatedAdvertisedVersions ) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiVersionModel"/> class.
    /// </summary>
    /// <param name="declaredVersions">The declared <see cref="IEnumerable{T}">sequence</see> of <see cref="ApiVersion">API versions</see> by an implementation.</param>
    /// <param name="supportedVersions">The supported <see cref="IEnumerable{T}">sequence</see> of <see cref="ApiVersion">API versions</see>.</param>
    /// <param name="deprecatedVersions">The deprecated <see cref="IEnumerable{T}">sequence</see> of <see cref="ApiVersion">API versions</see>.</param>
    /// <param name="advertisedVersions">The advertised <see cref="IEnumerable{T}">sequence</see> of <see cref="ApiVersion">API versions</see>.</param>
    /// <param name="deprecatedAdvertisedVersions">The deprecated, advertised <see cref="IEnumerable{T}">sequence</see> of <see cref="ApiVersion">API versions</see>.</param>
    public ApiVersionModel(
        IEnumerable<ApiVersion> declaredVersions,
        IEnumerable<ApiVersion> supportedVersions,
        IEnumerable<ApiVersion> deprecatedVersions,
        IEnumerable<ApiVersion> advertisedVersions,
        IEnumerable<ApiVersion> deprecatedAdvertisedVersions )
    {
        DeclaredApiVersions = declaredVersions.Distinct().OrderBy( v => v ).ToArray();
        SupportedApiVersions = supportedVersions.Union( advertisedVersions ).OrderBy( v => v ).ToArray();
        DeprecatedApiVersions = deprecatedVersions.Union( deprecatedAdvertisedVersions ).OrderBy( v => v ).ToArray();
        ImplementedApiVersions = SupportedApiVersions.Union( DeprecatedApiVersions ).OrderBy( v => v ).ToArray();
    }

    private string DebuggerDisplayText => IsApiVersionNeutral ? "*.*" : string.Join( ", ", DeclaredApiVersions );

    /// <summary>
    /// Gets the default API version information.
    /// </summary>
    /// <value>The default <see cref="ApiVersionModel">API version information</see>.</value>
    public static ApiVersionModel Default => defaultVersion ??= new( DefaultModel );

    /// <summary>
    /// Gets the neutral API version information.
    /// </summary>
    /// <value>The neutral <see cref="ApiVersionModel">API version information</see>.</value>
    public static ApiVersionModel Neutral => neutralVersion ??= new( NeutralModel );

    /// <summary>
    /// Gets empty API version information.
    /// </summary>
    /// <value>The empty <see cref="ApiVersionModel">API version information</see>.</value>
    public static ApiVersionModel Empty => emptyVersion ??= new( EmptyModel );

    /// <summary>
    /// Gets a value indicating whether the API is version-neutral.
    /// </summary>
    /// <value>True if the API is version-neutral; otherwise, false.</value>
    public bool IsApiVersionNeutral { get; }

    /// <summary>
    /// Gets the API versions declared by an implementation.
    /// </summary>
    /// <value>A <see cref="IReadOnlyList{T}">read-only list</see> of <see cref="ApiVersion">API versions</see> declared by an implementation.</value>
    /// <remarks>The declared API versions are constrained to the versions declared explicitly by an implementation.</remarks>
    public IReadOnlyList<ApiVersion> DeclaredApiVersions { get; }

    /// <summary>
    /// Gets the versions implemented by the API.
    /// </summary>
    /// <value>A <see cref="IReadOnlyList{T}">read-only list</see> of implemented <see cref="ApiVersion">API versions</see>.</value>
    /// <remarks>The implemented API versions include the supported and deprecated API versions.</remarks>
    public IReadOnlyList<ApiVersion> ImplementedApiVersions { get; }

    /// <summary>
    /// Gets the versions supported by the API.
    /// </summary>
    /// <value>A <see cref="IReadOnlyList{T}">read-only list</see> of support <see cref="ApiVersion">API versions</see>.</value>
    public IReadOnlyList<ApiVersion> SupportedApiVersions { get; }

    /// <summary>
    /// Gets the versions deprecated by the API.
    /// </summary>
    /// <value>A <see cref="IReadOnlyList{T}">read-only list</see> of deprecated <see cref="ApiVersion">API versions</see>.</value>
    /// <remarks>A deprecated API version does not mean it is not supported. A deprecated API version is typically advertised six
    /// months or more before it becomes unsupported; in which case, the API would no longer indicate that it is an
    /// <see cref="ImplementedApiVersions">implemented version</see>.</remarks>
    public IReadOnlyList<ApiVersion> DeprecatedApiVersions { get; }
}