#if WEBAPI
namespace Microsoft.Web.Http.Versioning
#else
namespace Microsoft.AspNetCore.Mvc.Versioning
#endif
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;

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
            Arg.NotNull( version, nameof( version ) );
            Arg.NotNull( otherVersion, nameof( otherVersion ) );
            Contract.Ensures( Contract.Result<ApiVersionModel>() != null );

            return version.Aggregate( new[] { otherVersion } );
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
            Arg.NotNull( version, nameof( version ) );
            Arg.NotNull( otherVersions, nameof( otherVersions ) );
            Contract.Ensures( Contract.Result<ApiVersionModel>() != null );

            var implemented = new HashSet<ApiVersion>();
            var supported = new HashSet<ApiVersion>();
            var deprecated = new HashSet<ApiVersion>();

            implemented.UnionWith( version.ImplementedApiVersions );
            supported.UnionWith( version.SupportedApiVersions );
            deprecated.UnionWith( version.DeprecatedApiVersions );

            foreach ( var otherVersion in otherVersions )
            {
                implemented.UnionWith( otherVersion.ImplementedApiVersions );
                supported.UnionWith( otherVersion.SupportedApiVersions );
                deprecated.UnionWith( otherVersion.DeprecatedApiVersions );
            }

            return new ApiVersionModel( version, implemented.ToSortedReadOnlyList(), supported.ToSortedReadOnlyList(), deprecated.ToSortedReadOnlyList() );
        }

        /// <summary>
        /// Aggregates a sequence of version information.
        /// </summary>
        /// <param name="versions">The <see cref="ApiVersionModel">API version information</see> to aggregate.</param>
        /// <returns>A new <see cref="ApiVersionModel"/> that is the aggregated result of the provided <paramref name="versions">version information</paramref>.</returns>
        public static ApiVersionModel Aggregate( this IEnumerable<ApiVersionModel> versions )
        {
            Arg.NotNull( versions, nameof( versions ) );
            Contract.Ensures( Contract.Result<ApiVersionModel>() != null );

            var supported = new HashSet<ApiVersion>();
            var deprecated = new HashSet<ApiVersion>();

            using ( var iterator = versions.GetEnumerator() )
            {
                if ( !iterator.MoveNext() )
                {
                    return new ApiVersionModel( supported, deprecated );
                }

                supported.UnionWith( iterator.Current.SupportedApiVersions );
                deprecated.UnionWith( iterator.Current.DeprecatedApiVersions );

                while ( iterator.MoveNext() )
                {
                    supported.UnionWith( iterator.Current.SupportedApiVersions );
                    deprecated.UnionWith( iterator.Current.DeprecatedApiVersions );
                }
            }

            return new ApiVersionModel( supported, deprecated );
        }
    }
}