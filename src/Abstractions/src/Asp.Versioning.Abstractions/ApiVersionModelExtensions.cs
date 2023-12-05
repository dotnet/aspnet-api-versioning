// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

/// <summary>
/// Provides extension methods for the <see cref="ApiVersionModel"/> class.
/// </summary>
public static class ApiVersionModelExtensions
{
    /// <summary>
    /// Aggregates the current version information with other version information.
    /// </summary>
    /// <param name="version">The <see cref="ApiVersionModel">API version information</see> that is the basis
    /// of the aggregation.</param>
    /// <param name="otherVersion">The other <see cref="ApiVersionModel">API version information</see> to aggregate.</param>
    /// <returns>A new <see cref="ApiVersionModel"/> that is the aggregated result of the
    /// <paramref name="otherVersion">other version information</paramref> and the current version information.</returns>
    public static ApiVersionModel Aggregate( this ApiVersionModel version, ApiVersionModel otherVersion )
    {
        ArgumentNullException.ThrowIfNull( version );
        ArgumentNullException.ThrowIfNull( otherVersion );

        var implemented = new SortedSet<ApiVersion>( version.ImplementedApiVersions );
        var supported = new SortedSet<ApiVersion>( version.SupportedApiVersions );
        var deprecated = new SortedSet<ApiVersion>( version.DeprecatedApiVersions );

        implemented.UnionWith( otherVersion.ImplementedApiVersions );
        supported.UnionWith( otherVersion.SupportedApiVersions );
        deprecated.UnionWith( otherVersion.DeprecatedApiVersions );
        deprecated.ExceptWith( supported );

        return new( version, implemented.ToArray(), supported.ToArray(), deprecated.ToArray() );
    }

    /// <summary>
    /// Aggregates the current version information with other version information.
    /// </summary>
    /// <param name="version">The <see cref="ApiVersionModel">API version information</see> that is the basis
    /// of the aggregation.</param>
    /// <param name="otherVersions">A <see cref="IEnumerable{T}">sequence</see> of other
    /// <see cref="ApiVersionModel">API version information</see> to aggregate.</param>
    /// <returns>A new <see cref="ApiVersionModel"/> that is the aggregated result of the
    /// <paramref name="otherVersions">other version information</paramref> and the current version information.</returns>
    public static ApiVersionModel Aggregate( this ApiVersionModel version, IEnumerable<ApiVersionModel> otherVersions )
    {
        ArgumentNullException.ThrowIfNull( version );
        ArgumentNullException.ThrowIfNull( otherVersions );

        if ( ( otherVersions is ICollection<ApiVersionModel> collection && collection.Count == 0 ) ||
             ( otherVersions is IReadOnlyCollection<ApiVersionModel> readOnlyCollection && readOnlyCollection.Count == 0 ) )
        {
            return version;
        }

        using var iterator = otherVersions.GetEnumerator();

        if ( !iterator.MoveNext() )
        {
            return version;
        }

        var implemented = new SortedSet<ApiVersion>( version.ImplementedApiVersions );
        var supported = new SortedSet<ApiVersion>( version.SupportedApiVersions );
        var deprecated = new SortedSet<ApiVersion>( version.DeprecatedApiVersions );

        do
        {
            var otherVersion = iterator.Current;

            implemented.UnionWith( otherVersion.ImplementedApiVersions );
            supported.UnionWith( otherVersion.SupportedApiVersions );
            deprecated.UnionWith( otherVersion.DeprecatedApiVersions );
        }
        while ( iterator.MoveNext() );

        deprecated.ExceptWith( supported );

        return new( version, implemented.ToArray(), supported.ToArray(), deprecated.ToArray() );
    }

    /// <summary>
    /// Aggregates a sequence of version information.
    /// </summary>
    /// <param name="versions">The <see cref="ApiVersionModel">API version information</see> to aggregate.</param>
    /// <returns>A new <see cref="ApiVersionModel"/> that is the aggregated result of the provided <paramref name="versions">version information</paramref>.</returns>
    public static ApiVersionModel Aggregate( this IEnumerable<ApiVersionModel> versions )
    {
        ArgumentNullException.ThrowIfNull( versions );

        if ( ( versions is ICollection<ApiVersionModel> collection && collection.Count == 0 ) ||
             ( versions is IReadOnlyCollection<ApiVersionModel> readOnlyCollection && readOnlyCollection.Count == 0 ) )
        {
            return ApiVersionModel.Empty;
        }

        using var iterator = versions.GetEnumerator();

        if ( !iterator.MoveNext() )
        {
            return ApiVersionModel.Empty;
        }

        var version = iterator.Current;
        var supported = new SortedSet<ApiVersion>( version.SupportedApiVersions );
        var deprecated = new SortedSet<ApiVersion>( version.DeprecatedApiVersions );

        while ( iterator.MoveNext() )
        {
            version = iterator.Current;
            supported.UnionWith( version.SupportedApiVersions );
            deprecated.UnionWith( version.DeprecatedApiVersions );
        }

        deprecated.ExceptWith( supported );

        return new( supported, deprecated );
    }
}