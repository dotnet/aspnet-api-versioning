// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

/// <summary>
/// Represents the base implementation for the metadata that describes the <see cref="ApiVersion">API versions</see> associated with a service.
/// </summary>
public abstract partial class ApiVersionsBaseAttribute : Attribute
{
    private int? hashCode;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiVersionsBaseAttribute"/> class.
    /// </summary>
    /// <param name="version">The <see cref="ApiVersion">API version</see>.</param>
    protected ApiVersionsBaseAttribute( ApiVersion version ) => Versions = new[] { version };

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiVersionsBaseAttribute"/> class.
    /// </summary>
    /// <param name="version">The <see cref="ApiVersion">API version</see>.</param>
    /// <param name="otherVersions">An array of other <see cref="ApiVersion">API versions</see>.</param>
    protected ApiVersionsBaseAttribute( ApiVersion version, params ApiVersion[] otherVersions )
    {
        int count;

        if ( otherVersions is null || ( count = otherVersions.Length ) == 0 )
        {
            Versions = new[] { version };
        }
        else
        {
            var versions = new ApiVersion[count + 1];
            versions[0] = version;
            System.Array.Copy( otherVersions, 0, versions, 1, count );
            Versions = versions;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiVersionsBaseAttribute"/> class.
    /// </summary>
    /// <param name="version">A numeric API version.</param>
    protected ApiVersionsBaseAttribute( double version ) => Versions = new ApiVersion[] { new( version ) };

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiVersionsBaseAttribute"/> class.
    /// </summary>
    /// <param name="version">A numeric API version.</param>
    /// <param name="otherVersions">An array of other numeric API versions.</param>
    protected ApiVersionsBaseAttribute( double version, params double[] otherVersions )
    {
        int count;

        if ( otherVersions is null || ( count = otherVersions.Length ) == 0 )
        {
            Versions = new ApiVersion[] { new( version ) };
        }
        else
        {
            var versions = new ApiVersion[count + 1];

            versions[0] = new( version );

            for ( var i = 0; i < count; i++ )
            {
#if NETSTANDARD
                var otherVersion = otherVersions[i];
#else
                ref readonly var otherVersion = ref otherVersions[i];
#endif
                versions[i + 1] = new( otherVersion );
            }

            Versions = versions;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiVersionsBaseAttribute"/> class.
    /// </summary>
    /// <param name="version">The API version string.</param>
    protected ApiVersionsBaseAttribute( string version ) : this( ApiVersionParser.Default, version ) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiVersionsBaseAttribute"/> class.
    /// </summary>
    /// <param name="version">The API version string.</param>
    /// <param name="otherVersions">An array of other API version strings.</param>
    protected ApiVersionsBaseAttribute( string version, params string[] otherVersions )
        : this( ApiVersionParser.Default, version, otherVersions ) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiVersionsBaseAttribute"/> class.
    /// </summary>
    /// <param name="parser">The parser used to parse the specified versions.</param>
    /// <param name="version">The API version string.</param>
    protected ApiVersionsBaseAttribute( IApiVersionParser parser, string version ) =>
        Versions = new[] { ( parser ?? throw new System.ArgumentNullException( nameof( parser ) ) ).Parse( version ) };

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiVersionsBaseAttribute"/> class.
    /// </summary>
    /// <param name="parser">The parser used to parse the specified versions.</param>
    /// <param name="version">The API version string.</param>
    /// <param name="otherVersions">An array of API other version strings.</param>
    protected ApiVersionsBaseAttribute( IApiVersionParser parser, string version, params string[] otherVersions )
    {
        ArgumentNullException.ThrowIfNull( parser );

        int count;

        if ( otherVersions is null || ( count = otherVersions.Length ) == 0 )
        {
            Versions = new[] { parser.Parse( version ) };
        }
        else
        {
            var versions = new ApiVersion[count + 1];

            versions[0] = parser.Parse( version );

            for ( var i = 0; i < count; i++ )
            {
                versions[i + 1] = parser.Parse( otherVersions[i] );
            }

            Versions = versions;
        }
    }

    /// <summary>
    /// Gets the API versions defined by the attribute.
    /// </summary>
    /// <value>A <see cref="IReadOnlyList{T}">read-only list</see> of <see cref="ApiVersion">API versions</see>.</value>
    public IReadOnlyList<ApiVersion> Versions { get; }

    /// <inheritdoc />
    public override bool Equals( object? obj ) => obj is ApiVersionsBaseAttribute && GetHashCode() == obj.GetHashCode();

    /// <inheritdoc />
    public override int GetHashCode()
    {
        if ( hashCode.HasValue )
        {
            return hashCode.Value;
        }

        if ( Versions.Count == 0 )
        {
            hashCode = 0;
            return 0;
        }

        var value = default( HashCode );

        value.Add( Versions[0] );

        for ( var i = 1; i < Versions.Count; i++ )
        {
            value.Add( Versions[i] );
        }

        hashCode = value.ToHashCode();
        return hashCode.Value;
    }
}