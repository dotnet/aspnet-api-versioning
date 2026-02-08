// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Conventions;

#if NETSTANDARD
using DateOnly = System.DateTime;
#endif

/// <summary>
/// Provides extension methods for convention builder interfaces.
/// </summary>
public static class ApiVersionConventionBuilderExtensions
{
    /// <typeparam name="T">The type of <see cref="IDeclareApiVersionConventionBuilder"/>.</typeparam>
    /// <param name="builder">The extended <see cref="IDeclareApiVersionConventionBuilder">convention builder</see>.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    extension<T>( T builder ) where T : notnull, IDeclareApiVersionConventionBuilder
    {
        /// <summary>
        /// Indicates that the specified API version is supported by the configured controller.
        /// </summary>
        /// <param name="majorVersion">The major version number.</param>
        /// <param name="minorVersion">The optional minor version number.</param>
        /// <param name="status">The optional version status.</param>
        public T HasApiVersion( int majorVersion, int? minorVersion = default, string? status = default )
        {
            builder.HasApiVersion( new ApiVersion( majorVersion, minorVersion, status ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is supported by the configured controller.
        /// </summary>
        /// <param name="version">The version number.</param>
        /// <param name="status">The optional version status.</param>
        public T HasApiVersion( double version, string? status = default )
        {
            builder.HasApiVersion( new ApiVersion( version, status ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is supported by the configured controller.
        /// </summary>
        /// <param name="year">The version year.</param>
        /// <param name="month">The version month.</param>
        /// <param name="day">The version day.</param>
        /// <param name="status">The optional version status.</param>
        public T HasApiVersion( int year, int month, int day, string? status = default )
        {
            builder.HasApiVersion( new ApiVersion( new DateOnly( year, month, day ), status ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is supported by the configured controller.
        /// </summary>
        /// <param name="groupVersion">The group version.</param>
        /// <param name="status">The optional version status.</param>
        public T HasApiVersion( DateOnly groupVersion, string? status = default )
        {
            builder.HasApiVersion( new ApiVersion( groupVersion, status ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API versions are supported by the configured controller.
        /// </summary>
        /// <param name="apiVersions">The <see cref="IEnumerable{T}">sequence</see> of <see cref="ApiVersion">API versions</see> supported by the controller.</param>
        public T HasApiVersions( IEnumerable<ApiVersion> apiVersions )
        {
            ArgumentNullException.ThrowIfNull( apiVersions );

            foreach ( var apiVersion in apiVersions )
            {
                builder.HasApiVersion( apiVersion );
            }

            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is deprecated by the configured controller.
        /// </summary>
        /// <param name="majorVersion">The major version number.</param>
        /// <param name="minorVersion">The optional minor version number.</param>
        /// <param name="status">The optional version status.</param>
        public T HasDeprecatedApiVersion( int majorVersion, int? minorVersion = default, string? status = default )
        {
            builder.HasDeprecatedApiVersion( new ApiVersion( majorVersion, minorVersion, status ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is deprecated by the configured controller.
        /// </summary>
        /// <param name="version">The version number.</param>
        /// <param name="status">The optional version status.</param>
        public T HasDeprecatedApiVersion( double version, string? status = default )
        {
            builder.HasDeprecatedApiVersion( new ApiVersion( version, status ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is deprecated by the configured controller.
        /// </summary>
        /// <param name="year">The version year.</param>
        /// <param name="month">The version month.</param>
        /// <param name="day">The version day.</param>
        /// <param name="status">The optional version status.</param>
        public T HasDeprecatedApiVersion( int year, int month, int day, string? status = default )
        {
            builder.HasDeprecatedApiVersion( new ApiVersion( new DateOnly( year, month, day ), status ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is deprecated by the configured controller.
        /// </summary>
        /// <param name="groupVersion">The group version.</param>
        /// <param name="status">The optional version status.</param>
        public T HasDeprecatedApiVersion( DateOnly groupVersion, string? status = default )
        {
            builder.HasDeprecatedApiVersion( new ApiVersion( groupVersion, status ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API versions are deprecated by the configured controller.
        /// </summary>
        /// <param name="apiVersions">The <see cref="IEnumerable{T}">sequence</see> of <see cref="ApiVersion">API versions</see> deprecated by the controller.</param>
        public T HasDeprecatedApiVersions( IEnumerable<ApiVersion> apiVersions )
        {
            ArgumentNullException.ThrowIfNull( apiVersions );

            foreach ( var apiVersion in apiVersions )
            {
                builder.HasDeprecatedApiVersion( apiVersion );
            }

            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is advertised by the configured controller.
        /// </summary>
        /// <param name="majorVersion">The major version number.</param>
        /// <param name="minorVersion">The optional minor version number.</param>
        /// <param name="status">The optional version status.</param>
        public T AdvertisesApiVersion( int majorVersion, int? minorVersion = default, string? status = default )
        {
            builder.AdvertisesApiVersion( new ApiVersion( majorVersion, minorVersion, status ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is advertised by the configured controller.
        /// </summary>
        /// <param name="version">The version number.</param>
        /// <param name="status">The optional version status.</param>
        public T AdvertisesApiVersion( double version, string? status = default )
        {
            builder.AdvertisesApiVersion( new ApiVersion( version, status ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is advertised by the configured controller.
        /// </summary>
        /// <param name="year">The version year.</param>
        /// <param name="month">The version month.</param>
        /// <param name="day">The version day.</param>
        /// <param name="status">The optional version status.</param>
        public T AdvertisesApiVersion( int year, int month, int day, string? status = default )
        {
            builder.AdvertisesApiVersion( new ApiVersion( new DateOnly( year, month, day ), status ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is advertised by the configured controller.
        /// </summary>
        /// <param name="groupVersion">The group version.</param>
        /// <param name="status">The optional version status.</param>
        public T AdvertisesApiVersion( DateOnly groupVersion, string? status = default )
        {
            builder.AdvertisesApiVersion( new ApiVersion( groupVersion, status ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API versions are advertised by the configured controller.
        /// </summary>
        /// <param name="apiVersions">The <see cref="IEnumerable{T}">sequence</see> of <see cref="ApiVersion">API versions</see> advertised by the controller.</param>
        public T AdvertisesApiVersions( IEnumerable<ApiVersion> apiVersions )
        {
            ArgumentNullException.ThrowIfNull( apiVersions );

            foreach ( var apiVersion in apiVersions )
            {
                builder.AdvertisesApiVersion( apiVersion );
            }

            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is advertised and deprecated by the configured controller.
        /// </summary>
        /// <param name="majorVersion">The major version number.</param>
        /// <param name="minorVersion">The optional minor version number.</param>
        /// <param name="status">The optional version status.</param>
        public T AdvertisesDeprecatedApiVersion( int majorVersion, int? minorVersion = default, string? status = default )
        {
            builder.AdvertisesDeprecatedApiVersion( new ApiVersion( majorVersion, minorVersion, status ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is advertised and deprecated by the configured controller.
        /// </summary>
        /// <param name="version">The version number.</param>
        /// <param name="status">The optional version status.</param>
        public T AdvertisesDeprecatedApiVersion( double version, string? status = default )
        {
            builder.AdvertisesDeprecatedApiVersion( new ApiVersion( version, status ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is advertised and deprecated by the configured controller.
        /// </summary>
        /// <param name="year">The version year.</param>
        /// <param name="month">The version month.</param>
        /// <param name="day">The version day.</param>
        /// <param name="status">The version status.</param>
        public T AdvertisesDeprecatedApiVersion( int year, int month, int day, string? status = default )
        {
            builder.AdvertisesDeprecatedApiVersion( new ApiVersion( new DateOnly( year, month, day ), status ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is advertised and deprecated by the configured controller.
        /// </summary>
        /// <param name="groupVersion">The group version.</param>
        /// <param name="status">The optional version status.</param>
        public T AdvertisesDeprecatedApiVersion( DateOnly groupVersion, string? status = default )
        {
            builder.AdvertisesDeprecatedApiVersion( new ApiVersion( groupVersion, status ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API versions are advertised and deprecated by the configured controller.
        /// </summary>
        /// <param name="apiVersions">The <see cref="IEnumerable{T}">sequence</see> of deprecated <see cref="ApiVersion">API versions</see> advertised by the controller.</param>
        public T AdvertisesDeprecatedApiVersions( IEnumerable<ApiVersion> apiVersions )
        {
            ArgumentNullException.ThrowIfNull( apiVersions );

            foreach ( var apiVersion in apiVersions )
            {
                builder.AdvertisesDeprecatedApiVersion( apiVersion );
            }

            return builder;
        }
    }

    /// <typeparam name="T">The type of <see cref="IMapToApiVersionConventionBuilder"/>.</typeparam>
    /// <param name="builder">The extended <see cref="IMapToApiVersionConventionBuilder"/>.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    extension<T>( T builder )
        where T : notnull, IMapToApiVersionConventionBuilder
    {
        /// <summary>
        /// Indicates that the specified API version is mapped to the configured controller action.
        /// </summary>
        /// <param name="majorVersion">The major version number.</param>
        /// <param name="minorVersion">The optional minor version number.</param>
        /// <param name="status">The optional version status.</param>
        public T MapToApiVersion( int majorVersion, int? minorVersion = default, string? status = default )
        {
            builder.MapToApiVersion( new ApiVersion( majorVersion, minorVersion, status ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is mapped to the configured controller action.
        /// </summary>
        /// <param name="version">The version number.</param>
        /// <param name="status">The optional version status.</param>
        public T MapToApiVersion( double version, string? status = default )
        {
            builder.MapToApiVersion( new ApiVersion( version, status ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is mapped to the configured controller action.
        /// </summary>
        /// <param name="year">The version year.</param>
        /// <param name="month">The version month.</param>
        /// <param name="day">The version day.</param>
        /// <param name="status">The optional version status.</param>
        public T MapToApiVersion( int year, int month, int day, string? status = default )
        {
            builder.MapToApiVersion( new ApiVersion( new DateOnly( year, month, day ), status ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is mapped to the configured controller action.
        /// </summary>
        /// <param name="groupVersion">The group version.</param>
        /// <param name="status">The optional version status.</param>
        public T MapToApiVersion( DateOnly groupVersion, string? status = default )
        {
            builder.MapToApiVersion( new ApiVersion( groupVersion, status ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API versions are mapped to the configured controller action.
        /// </summary>
        /// <param name="apiVersions">The <see cref="IEnumerable{T}">sequence</see> of <see cref="ApiVersion">API versions</see> supported by the controller.</param>
        public T MapToApiVersions( IEnumerable<ApiVersion> apiVersions )
        {
            ArgumentNullException.ThrowIfNull( apiVersions );

            foreach ( var apiVersion in apiVersions )
            {
                builder.MapToApiVersion( apiVersion );
            }

            return builder;
        }
    }
}