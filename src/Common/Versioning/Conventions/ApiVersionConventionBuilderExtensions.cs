#if WEBAPI
namespace Microsoft.Web.Http.Versioning.Conventions
#else
namespace Microsoft.AspNetCore.Mvc.Versioning.Conventions
#endif
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Provides extension methods for <see cref="IDeclareApiVersionConventionBuilder">convention builder</see> interface.
    /// </summary>
#if !WEBAPI
    [CLSCompliant( false )]
#endif
    public static class ApiVersionConventionBuilderExtensions
    {
        /// <summary>
        /// Indicates that the specified API version is supported by the configured controller.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="IDeclareApiVersionConventionBuilder"/>.</typeparam>
        /// <param name="builder">The extended <see cref="IDeclareApiVersionConventionBuilder">convention buileder</see>.</param>
        /// <param name="majorVersion">The value for a major version only scheme.</param>
        /// <returns>The original <paramref name="builder"/>.</returns>
        public static T HasApiVersion<T>( this T builder, int majorVersion ) where T : notnull, IDeclareApiVersionConventionBuilder
        {
            builder.HasApiVersion( new ApiVersion( majorVersion, 0 ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is supported by the configured controller.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="IDeclareApiVersionConventionBuilder"/>.</typeparam>
        /// <param name="builder">The extended <see cref="IDeclareApiVersionConventionBuilder">convention buileder</see>.</param>
        /// <param name="majorVersion">The value for a major version only scheme.</param>
        /// <param name="status">The version status.</param>
        /// <returns>The original <paramref name="builder"/>.</returns>
        public static T HasApiVersion<T>( this T builder, int majorVersion, string status ) where T : notnull, IDeclareApiVersionConventionBuilder
        {
            builder.HasApiVersion( new ApiVersion( majorVersion, 0, status ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is supported by the configured controller.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="IDeclareApiVersionConventionBuilder"/>.</typeparam>
        /// <param name="builder">The extended <see cref="IDeclareApiVersionConventionBuilder">convention buileder</see>.</param>
        /// <param name="majorVersion">The major version number.</param>
        /// <param name="minorVersion">The minor version number.</param>
        /// <returns>The original <paramref name="builder"/>.</returns>
        public static T HasApiVersion<T>( this T builder, int majorVersion, int minorVersion ) where T : notnull, IDeclareApiVersionConventionBuilder
        {
            builder.HasApiVersion( new ApiVersion( majorVersion, minorVersion ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is supported by the configured controller.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="IDeclareApiVersionConventionBuilder"/>.</typeparam>
        /// <param name="builder">The extended <see cref="IDeclareApiVersionConventionBuilder">convention buileder</see>.</param>
        /// <param name="majorVersion">The major version number.</param>
        /// <param name="minorVersion">The minor version number.</param>
        /// <param name="status">The version status.</param>
        /// <returns>The original <paramref name="builder"/>.</returns>
        public static T HasApiVersion<T>( this T builder, int majorVersion, int minorVersion, string status ) where T : notnull, IDeclareApiVersionConventionBuilder
        {
            builder.HasApiVersion( new ApiVersion( majorVersion, minorVersion, status ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is supported by the configured controller.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="IDeclareApiVersionConventionBuilder"/>.</typeparam>
        /// <param name="builder">The extended <see cref="IDeclareApiVersionConventionBuilder">convention buileder</see>.</param>
        /// <param name="year">The version year.</param>
        /// <param name="month">The version month.</param>
        /// <param name="day">The version day.</param>
        /// <returns>The original <paramref name="builder"/>.</returns>
        public static T HasApiVersion<T>( this T builder, int year, int month, int day ) where T : notnull, IDeclareApiVersionConventionBuilder
        {
            builder.HasApiVersion( new ApiVersion( new DateTime( year, month, day ) ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is supported by the configured controller.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="IDeclareApiVersionConventionBuilder"/>.</typeparam>
        /// <param name="builder">The extended <see cref="IDeclareApiVersionConventionBuilder">convention buileder</see>.</param>
        /// <param name="year">The version year.</param>
        /// <param name="month">The version month.</param>
        /// <param name="day">The version day.</param>
        /// <param name="status">The version status.</param>
        /// <returns>The original <paramref name="builder"/>.</returns>
        public static T HasApiVersion<T>( this T builder, int year, int month, int day, string status ) where T : notnull, IDeclareApiVersionConventionBuilder
        {
            builder.HasApiVersion( new ApiVersion( new DateTime( year, month, day ), status ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is supported by the configured controller.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="IDeclareApiVersionConventionBuilder"/>.</typeparam>
        /// <param name="builder">The extended <see cref="IDeclareApiVersionConventionBuilder">convention buileder</see>.</param>
        /// <param name="groupVersion">The group version.</param>
        /// <returns>The original <paramref name="builder"/>.</returns>
        public static T HasApiVersion<T>( this T builder, DateTime groupVersion ) where T : notnull, IDeclareApiVersionConventionBuilder
        {
            builder.HasApiVersion( new ApiVersion( groupVersion ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is supported by the configured controller.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="IDeclareApiVersionConventionBuilder"/>.</typeparam>
        /// <param name="builder">The extended <see cref="IDeclareApiVersionConventionBuilder">convention buileder</see>.</param>
        /// <param name="groupVersion">The group version.</param>
        /// <param name="status">The version status.</param>
        /// <returns>The original <paramref name="builder"/>.</returns>
        public static T HasApiVersion<T>( this T builder, DateTime groupVersion, string status ) where T : notnull, IDeclareApiVersionConventionBuilder
        {
            builder.HasApiVersion( new ApiVersion( groupVersion, status ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API versions are supported by the configured controller.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="IDeclareApiVersionConventionBuilder"/>.</typeparam>
        /// <param name="builder">The extended <see cref="IDeclareApiVersionConventionBuilder">convention buileder</see>.</param>
        /// <param name="apiVersions">The <see cref="IEnumerable{T}">sequence</see> of <see cref="ApiVersion">API versions</see> supported by the controller.</param>
        /// <returns>The original <paramref name="builder"/>.</returns>
        public static T HasApiVersions<T>( this T builder, IEnumerable<ApiVersion> apiVersions ) where T : notnull, IDeclareApiVersionConventionBuilder
        {
            if ( apiVersions == null )
            {
                throw new ArgumentNullException( nameof( apiVersions ) );
            }

            foreach ( var apiVersion in apiVersions )
            {
                builder.HasApiVersion( apiVersion );
            }

            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is deprecated by the configured controller.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="IDeclareApiVersionConventionBuilder"/>.</typeparam>
        /// <param name="builder">The extended <see cref="IDeclareApiVersionConventionBuilder">convention buileder</see>.</param>
        /// <param name="majorVersion">The value for a major version only scheme.</param>
        /// <returns>The original <paramref name="builder"/>.</returns>
        public static T HasDeprecatedApiVersion<T>( this T builder, int majorVersion ) where T : notnull, IDeclareApiVersionConventionBuilder
        {
            builder.HasDeprecatedApiVersion( new ApiVersion( majorVersion, 0 ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is deprecated by the configured controller.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="IDeclareApiVersionConventionBuilder"/>.</typeparam>
        /// <param name="builder">The extended <see cref="IDeclareApiVersionConventionBuilder">convention buileder</see>.</param>
        /// <param name="majorVersion">The value for a major version only scheme.</param>
        /// <param name="status">The version status.</param>
        /// <returns>The original <paramref name="builder"/>.</returns>
        public static T HasDeprecatedApiVersion<T>( this T builder, int majorVersion, string status ) where T : notnull, IDeclareApiVersionConventionBuilder
        {
            builder.HasDeprecatedApiVersion( new ApiVersion( majorVersion, 0, status ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is deprecated by the configured controller.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="IDeclareApiVersionConventionBuilder"/>.</typeparam>
        /// <param name="builder">The extended <see cref="IDeclareApiVersionConventionBuilder">convention buileder</see>.</param>
        /// <param name="majorVersion">The major version number.</param>
        /// <param name="minorVersion">The minor version number.</param>
        /// <returns>The original <paramref name="builder"/>.</returns>
        public static T HasDeprecatedApiVersion<T>( this T builder, int majorVersion, int minorVersion ) where T : notnull, IDeclareApiVersionConventionBuilder
        {
            builder.HasDeprecatedApiVersion( new ApiVersion( majorVersion, minorVersion ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is deprecated by the configured controller.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="IDeclareApiVersionConventionBuilder"/>.</typeparam>
        /// <param name="builder">The extended <see cref="IDeclareApiVersionConventionBuilder">convention buileder</see>.</param>
        /// <param name="majorVersion">The major version number.</param>
        /// <param name="minorVersion">The minor version number.</param>
        /// <param name="status">The version status.</param>
        /// <returns>The original <paramref name="builder"/>.</returns>
        public static T HasDeprecatedApiVersion<T>( this T builder, int majorVersion, int minorVersion, string status ) where T : notnull, IDeclareApiVersionConventionBuilder
        {
            builder.HasDeprecatedApiVersion( new ApiVersion( majorVersion, minorVersion, status ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is deprecated by the configured controller.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="IDeclareApiVersionConventionBuilder"/>.</typeparam>
        /// <param name="builder">The extended <see cref="IDeclareApiVersionConventionBuilder">convention buileder</see>.</param>
        /// <param name="year">The version year.</param>
        /// <param name="month">The version month.</param>
        /// <param name="day">The version day.</param>
        /// <returns>The original <paramref name="builder"/>.</returns>
        public static T HasDeprecatedApiVersion<T>( this T builder, int year, int month, int day ) where T : notnull, IDeclareApiVersionConventionBuilder
        {
            builder.HasDeprecatedApiVersion( new ApiVersion( new DateTime( year, month, day ) ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is deprecated by the configured controller.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="IDeclareApiVersionConventionBuilder"/>.</typeparam>
        /// <param name="builder">The extended <see cref="IDeclareApiVersionConventionBuilder">convention buileder</see>.</param>
        /// <param name="year">The version year.</param>
        /// <param name="month">The version month.</param>
        /// <param name="day">The version day.</param>
        /// <param name="status">The version status.</param>
        /// <returns>The original <paramref name="builder"/>.</returns>
        public static T HasDeprecatedApiVersion<T>( this T builder, int year, int month, int day, string status ) where T : notnull, IDeclareApiVersionConventionBuilder
        {
            builder.HasDeprecatedApiVersion( new ApiVersion( new DateTime( year, month, day ), status ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is deprecated by the configured controller.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="IDeclareApiVersionConventionBuilder"/>.</typeparam>
        /// <param name="builder">The extended <see cref="IDeclareApiVersionConventionBuilder">convention buileder</see>.</param>
        /// <param name="groupVersion">The group version.</param>
        /// <returns>The original <paramref name="builder"/>.</returns>
        public static T HasDeprecatedApiVersion<T>( this T builder, DateTime groupVersion ) where T : notnull, IDeclareApiVersionConventionBuilder
        {
            builder.HasDeprecatedApiVersion( new ApiVersion( groupVersion ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is deprecated by the configured controller.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="IDeclareApiVersionConventionBuilder"/>.</typeparam>
        /// <param name="builder">The extended <see cref="IDeclareApiVersionConventionBuilder">convention buileder</see>.</param>
        /// <param name="groupVersion">The group version.</param>
        /// <param name="status">The version status.</param>
        /// <returns>The original <paramref name="builder"/>.</returns>
        public static T HasDeprecatedApiVersion<T>( this T builder, DateTime groupVersion, string status ) where T : notnull, IDeclareApiVersionConventionBuilder
        {
            builder.HasDeprecatedApiVersion( new ApiVersion( groupVersion, status ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API versions are deprecated by the configured controller.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="IDeclareApiVersionConventionBuilder"/>.</typeparam>
        /// <param name="builder">The extended <see cref="IDeclareApiVersionConventionBuilder">convention buileder</see>.</param>
        /// <param name="apiVersions">The <see cref="IEnumerable{T}">sequence</see> of <see cref="ApiVersion">API versions</see> deprecated by the controller.</param>
        /// <returns>The original <paramref name="builder"/>.</returns>
        public static T HasDeprecatedApiVersions<T>( this T builder, IEnumerable<ApiVersion> apiVersions ) where T : notnull, IDeclareApiVersionConventionBuilder
        {
            if ( apiVersions == null )
            {
                throw new ArgumentNullException( nameof( apiVersions ) );
            }

            foreach ( var apiVersion in apiVersions )
            {
                builder.HasDeprecatedApiVersion( apiVersion );
            }

            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is advertised by the configured controller.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="IDeclareApiVersionConventionBuilder"/>.</typeparam>
        /// <param name="builder">The extended <see cref="IDeclareApiVersionConventionBuilder">convention buileder</see>.</param>
        /// <param name="majorVersion">The value for a major version only scheme.</param>
        /// <returns>The original <paramref name="builder"/>.</returns>
        public static T AdvertisesApiVersion<T>( this T builder, int majorVersion ) where T : notnull, IDeclareApiVersionConventionBuilder
        {
            builder.AdvertisesApiVersion( new ApiVersion( majorVersion, 0 ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is advertised by the configured controller.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="IDeclareApiVersionConventionBuilder"/>.</typeparam>
        /// <param name="builder">The extended <see cref="IDeclareApiVersionConventionBuilder">convention buileder</see>.</param>
        /// <param name="majorVersion">The value for a major version only scheme.</param>
        /// <param name="status">The version status.</param>
        /// <returns>The original <paramref name="builder"/>.</returns>
        public static T AdvertisesApiVersion<T>( this T builder, int majorVersion, string status ) where T : notnull, IDeclareApiVersionConventionBuilder
        {
            builder.AdvertisesApiVersion( new ApiVersion( majorVersion, 0, status ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is advertised by the configured controller.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="IDeclareApiVersionConventionBuilder"/>.</typeparam>
        /// <param name="builder">The extended <see cref="IDeclareApiVersionConventionBuilder">convention buileder</see>.</param>
        /// <param name="majorVersion">The major version number.</param>
        /// <param name="minorVersion">The minor version number.</param>
        /// <returns>The original <paramref name="builder"/>.</returns>
        public static T AdvertisesApiVersion<T>( this T builder, int majorVersion, int minorVersion ) where T : notnull, IDeclareApiVersionConventionBuilder
        {
            builder.AdvertisesApiVersion( new ApiVersion( majorVersion, minorVersion ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is advertised by the configured controller.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="IDeclareApiVersionConventionBuilder"/>.</typeparam>
        /// <param name="builder">The extended <see cref="IDeclareApiVersionConventionBuilder">convention buileder</see>.</param>
        /// <param name="majorVersion">The major version number.</param>
        /// <param name="minorVersion">The minor version number.</param>
        /// <param name="status">The version status.</param>
        /// <returns>The original <paramref name="builder"/>.</returns>
        public static T AdvertisesApiVersion<T>( this T builder, int majorVersion, int minorVersion, string status ) where T : notnull, IDeclareApiVersionConventionBuilder
        {
            builder.AdvertisesApiVersion( new ApiVersion( majorVersion, minorVersion, status ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is advertised by the configured controller.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="IDeclareApiVersionConventionBuilder"/>.</typeparam>
        /// <param name="builder">The extended <see cref="IDeclareApiVersionConventionBuilder">convention buileder</see>.</param>
        /// <param name="year">The version year.</param>
        /// <param name="month">The version month.</param>
        /// <param name="day">The version day.</param>
        /// <returns>The original <paramref name="builder"/>.</returns>
        public static T AdvertisesApiVersion<T>( this T builder, int year, int month, int day ) where T : notnull, IDeclareApiVersionConventionBuilder
        {
            builder.AdvertisesApiVersion( new ApiVersion( new DateTime( year, month, day ) ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is advertised by the configured controller.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="IDeclareApiVersionConventionBuilder"/>.</typeparam>
        /// <param name="builder">The extended <see cref="IDeclareApiVersionConventionBuilder">convention buileder</see>.</param>
        /// <param name="year">The version year.</param>
        /// <param name="month">The version month.</param>
        /// <param name="day">The version day.</param>
        /// <param name="status">The version status.</param>
        /// <returns>The original <paramref name="builder"/>.</returns>
        public static T AdvertisesApiVersion<T>( this T builder, int year, int month, int day, string status ) where T : notnull, IDeclareApiVersionConventionBuilder
        {
            builder.AdvertisesApiVersion( new ApiVersion( new DateTime( year, month, day ), status ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is advertised by the configured controller.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="IDeclareApiVersionConventionBuilder"/>.</typeparam>
        /// <param name="builder">The extended <see cref="IDeclareApiVersionConventionBuilder">convention buileder</see>.</param>
        /// <param name="groupVersion">The group version.</param>
        /// <returns>The original <paramref name="builder"/>.</returns>
        public static T AdvertisesApiVersion<T>( this T builder, DateTime groupVersion ) where T : notnull, IDeclareApiVersionConventionBuilder
        {
            builder.AdvertisesApiVersion( new ApiVersion( groupVersion ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is advertised by the configured controller.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="IDeclareApiVersionConventionBuilder"/>.</typeparam>
        /// <param name="builder">The extended <see cref="IDeclareApiVersionConventionBuilder">convention buileder</see>.</param>
        /// <param name="groupVersion">The group version.</param>
        /// <param name="status">The version status.</param>
        /// <returns>The original <paramref name="builder"/>.</returns>
        public static T AdvertisesApiVersion<T>( this T builder, DateTime groupVersion, string status ) where T : notnull, IDeclareApiVersionConventionBuilder
        {
            builder.AdvertisesApiVersion( new ApiVersion( groupVersion, status ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API versions are advertised by the configured controller.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="IDeclareApiVersionConventionBuilder"/>.</typeparam>
        /// <param name="builder">The extended <see cref="IDeclareApiVersionConventionBuilder">convention buileder</see>.</param>
        /// <param name="apiVersions">The <see cref="IEnumerable{T}">sequence</see> of <see cref="ApiVersion">API versions</see> advertised by the controller.</param>
        /// <returns>The original <paramref name="builder"/>.</returns>
        public static T AdvertisesApiVersions<T>( this T builder, IEnumerable<ApiVersion> apiVersions ) where T : notnull, IDeclareApiVersionConventionBuilder
        {
            if ( apiVersions == null )
            {
                throw new ArgumentNullException( nameof( apiVersions ) );
            }

            foreach ( var apiVersion in apiVersions )
            {
                builder.AdvertisesApiVersion( apiVersion );
            }

            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is advertised and deprecated by the configured controller.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="IDeclareApiVersionConventionBuilder"/>.</typeparam>
        /// <param name="builder">The extended <see cref="IDeclareApiVersionConventionBuilder">convention buileder</see>.</param>
        /// <param name="majorVersion">The value for a major version only scheme.</param>
        /// <returns>The original <paramref name="builder"/>.</returns>
        public static T AdvertisesDeprecatedApiVersion<T>( this T builder, int majorVersion ) where T : notnull, IDeclareApiVersionConventionBuilder
        {
            builder.AdvertisesDeprecatedApiVersion( new ApiVersion( majorVersion, 0 ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is advertised and deprecated by the configured controller.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="IDeclareApiVersionConventionBuilder"/>.</typeparam>
        /// <param name="builder">The extended <see cref="IDeclareApiVersionConventionBuilder">convention buileder</see>.</param>
        /// <param name="majorVersion">The value for a major version only scheme.</param>
        /// <param name="status">The version status.</param>
        /// <returns>The original <paramref name="builder"/>.</returns>
        public static T AdvertisesDeprecatedApiVersion<T>( this T builder, int majorVersion, string status ) where T : notnull, IDeclareApiVersionConventionBuilder
        {
            builder.AdvertisesDeprecatedApiVersion( new ApiVersion( majorVersion, 0, status ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is advertised and deprecated by the configured controller.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="IDeclareApiVersionConventionBuilder"/>.</typeparam>
        /// <param name="builder">The extended <see cref="IDeclareApiVersionConventionBuilder">convention buileder</see>.</param>
        /// <param name="majorVersion">The major version number.</param>
        /// <param name="minorVersion">The minor version number.</param>
        /// <returns>The original <paramref name="builder"/>.</returns>
        public static T AdvertisesDeprecatedApiVersion<T>( this T builder, int majorVersion, int minorVersion ) where T : notnull, IDeclareApiVersionConventionBuilder
        {
            builder.AdvertisesDeprecatedApiVersion( new ApiVersion( majorVersion, minorVersion ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is advertised and deprecated by the configured controller.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="IDeclareApiVersionConventionBuilder"/>.</typeparam>
        /// <param name="builder">The extended <see cref="IDeclareApiVersionConventionBuilder">convention buileder</see>.</param>
        /// <param name="majorVersion">The major version number.</param>
        /// <param name="minorVersion">The minor version number.</param>
        /// <param name="status">The version status.</param>
        /// <returns>The original <paramref name="builder"/>.</returns>
        public static T AdvertisesDeprecatedApiVersion<T>( this T builder, int majorVersion, int minorVersion, string status ) where T : notnull, IDeclareApiVersionConventionBuilder
        {
            builder.AdvertisesDeprecatedApiVersion( new ApiVersion( majorVersion, minorVersion, status ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is advertised and deprecated by the configured controller.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="IDeclareApiVersionConventionBuilder"/>.</typeparam>
        /// <param name="builder">The extended <see cref="IDeclareApiVersionConventionBuilder">convention buileder</see>.</param>
        /// <param name="year">The version year.</param>
        /// <param name="month">The version month.</param>
        /// <param name="day">The version day.</param>
        /// <returns>The original <paramref name="builder"/>.</returns>
        public static T AdvertisesDeprecatedApiVersion<T>( this T builder, int year, int month, int day ) where T : notnull, IDeclareApiVersionConventionBuilder
        {
            builder.AdvertisesDeprecatedApiVersion( new ApiVersion( new DateTime( year, month, day ) ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is advertised and deprecated by the configured controller.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="IDeclareApiVersionConventionBuilder"/>.</typeparam>
        /// <param name="builder">The extended <see cref="IDeclareApiVersionConventionBuilder">convention buileder</see>.</param>
        /// <param name="year">The version year.</param>
        /// <param name="month">The version month.</param>
        /// <param name="day">The version day.</param>
        /// <param name="status">The version status.</param>
        /// <returns>The original <paramref name="builder"/>.</returns>
        public static T AdvertisesDeprecatedApiVersion<T>( this T builder, int year, int month, int day, string status ) where T : notnull, IDeclareApiVersionConventionBuilder
        {
            builder.AdvertisesDeprecatedApiVersion( new ApiVersion( new DateTime( year, month, day ), status ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is advertised and deprecated by the configured controller.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="IDeclareApiVersionConventionBuilder"/>.</typeparam>
        /// <param name="builder">The extended <see cref="IDeclareApiVersionConventionBuilder">convention buileder</see>.</param>
        /// <param name="groupVersion">The group version.</param>
        /// <returns>The original <paramref name="builder"/>.</returns>
        public static T AdvertisesDeprecatedApiVersion<T>( this T builder, DateTime groupVersion ) where T : notnull, IDeclareApiVersionConventionBuilder
        {
            builder.AdvertisesDeprecatedApiVersion( new ApiVersion( groupVersion ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is advertised and deprecated by the configured controller.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="IDeclareApiVersionConventionBuilder"/>.</typeparam>
        /// <param name="builder">The extended <see cref="IDeclareApiVersionConventionBuilder">convention buileder</see>.</param>
        /// <param name="groupVersion">The group version.</param>
        /// <param name="status">The version status.</param>
        /// <returns>The original <paramref name="builder"/>.</returns>
        public static T AdvertisesDeprecatedApiVersion<T>( this T builder, DateTime groupVersion, string status ) where T : notnull, IDeclareApiVersionConventionBuilder
        {
            builder.AdvertisesDeprecatedApiVersion( new ApiVersion( groupVersion, status ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API versions are advertised and deprecated by the configured controller.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="IDeclareApiVersionConventionBuilder"/>.</typeparam>
        /// <param name="builder">The extended <see cref="IDeclareApiVersionConventionBuilder">convention buileder</see>.</param>
        /// <param name="apiVersions">The <see cref="IEnumerable{T}">sequence</see> of deprecated <see cref="ApiVersion">API versions</see> advertised by the controller.</param>
        /// <returns>The original <paramref name="builder"/>.</returns>
        public static T AdvertisesDeprecatedApiVersions<T>( this T builder, IEnumerable<ApiVersion> apiVersions ) where T : notnull, IDeclareApiVersionConventionBuilder
        {
            if ( apiVersions == null )
            {
                throw new ArgumentNullException( nameof( apiVersions ) );
            }

            foreach ( var apiVersion in apiVersions )
            {
                builder.AdvertisesDeprecatedApiVersion( apiVersion );
            }

            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is mapped to the configured controller action.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="IMapToApiVersionConventionBuilder"/>.</typeparam>
        /// <param name="builder">The extended <see cref="IMapToApiVersionConventionBuilder"/>.</param>
        /// <param name="majorVersion">The value for a major version only scheme.</param>
        /// <returns>The original <paramref name="builder"/>.</returns>
        public static T MapToApiVersion<T>( this T builder, int majorVersion ) where T : notnull, IMapToApiVersionConventionBuilder
        {
            builder.MapToApiVersion( new ApiVersion( majorVersion, 0 ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is mapped to the configured controller action.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="IMapToApiVersionConventionBuilder"/>.</typeparam>
        /// <param name="builder">The extended <see cref="IMapToApiVersionConventionBuilder"/>.</param>
        /// <param name="majorVersion">The value for a major version only scheme.</param>
        /// <param name="status">The version status.</param>
        /// <returns>The original <paramref name="builder"/>.</returns>
        public static T MapToApiVersion<T>( this T builder, int majorVersion, string status ) where T : notnull, IMapToApiVersionConventionBuilder
        {
            builder.MapToApiVersion( new ApiVersion( majorVersion, 0, status ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is mapped to the configured controller action.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="IMapToApiVersionConventionBuilder"/>.</typeparam>
        /// <param name="builder">The extended <see cref="IMapToApiVersionConventionBuilder"/>.</param>
        /// <param name="majorVersion">The major version number.</param>
        /// <param name="minorVersion">The minor version number.</param>
        /// <returns>The original <paramref name="builder"/>.</returns>
        public static T MapToApiVersion<T>( this T builder, int majorVersion, int minorVersion ) where T : notnull, IMapToApiVersionConventionBuilder
        {
            builder.MapToApiVersion( new ApiVersion( majorVersion, minorVersion ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is mapped to the configured controller action.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="IMapToApiVersionConventionBuilder"/>.</typeparam>
        /// <param name="builder">The extended <see cref="IMapToApiVersionConventionBuilder"/>.</param>
        /// <param name="majorVersion">The major version number.</param>
        /// <param name="minorVersion">The minor version number.</param>
        /// <param name="status">The version status.</param>
        /// <returns>The original <paramref name="builder"/>.</returns>
        public static T MapToApiVersion<T>( this T builder, int majorVersion, int minorVersion, string status ) where T : notnull, IMapToApiVersionConventionBuilder
        {
            builder.MapToApiVersion( new ApiVersion( majorVersion, minorVersion, status ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is mapped to the configured controller action.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="IMapToApiVersionConventionBuilder"/>.</typeparam>
        /// <param name="builder">The extended <see cref="IMapToApiVersionConventionBuilder"/>.</param>
        /// <param name="year">The version year.</param>
        /// <param name="month">The version month.</param>
        /// <param name="day">The version day.</param>
        /// <returns>The original <paramref name="builder"/>.</returns>
        public static T MapToApiVersion<T>( this T builder, int year, int month, int day ) where T : notnull, IMapToApiVersionConventionBuilder
        {
            builder.MapToApiVersion( new ApiVersion( new DateTime( year, month, day ) ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is mapped to the configured controller action.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="IMapToApiVersionConventionBuilder"/>.</typeparam>
        /// <param name="builder">The extended <see cref="IMapToApiVersionConventionBuilder"/>.</param>
        /// <param name="year">The version year.</param>
        /// <param name="month">The version month.</param>
        /// <param name="day">The version day.</param>
        /// <param name="status">The version status.</param>
        /// <returns>The original <paramref name="builder"/>.</returns>
        public static T MapToApiVersion<T>( this T builder, int year, int month, int day, string status ) where T : notnull, IMapToApiVersionConventionBuilder
        {
            builder.MapToApiVersion( new ApiVersion( new DateTime( year, month, day ), status ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is mapped to the configured controller action.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="IMapToApiVersionConventionBuilder"/>.</typeparam>
        /// <param name="builder">The extended <see cref="IMapToApiVersionConventionBuilder"/>.</param>
        /// <param name="groupVersion">The group version.</param>
        /// <returns>The original <paramref name="builder"/>.</returns>
        public static T MapToApiVersion<T>( this T builder, DateTime groupVersion ) where T : notnull, IMapToApiVersionConventionBuilder
        {
            builder.MapToApiVersion( new ApiVersion( groupVersion ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is mapped to the configured controller action.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="IMapToApiVersionConventionBuilder"/>.</typeparam>
        /// <param name="builder">The extended <see cref="IMapToApiVersionConventionBuilder"/>.</param>
        /// <param name="groupVersion">The group version.</param>
        /// <param name="status">The version status.</param>
        /// <returns>The original <paramref name="builder"/>.</returns>
        public static T MapToApiVersion<T>( this T builder, DateTime groupVersion, string status ) where T : notnull, IMapToApiVersionConventionBuilder
        {
            builder.MapToApiVersion( new ApiVersion( groupVersion, status ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API versions are mapped to the configured controller action.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="IMapToApiVersionConventionBuilder"/>.</typeparam>
        /// <param name="builder">The extended <see cref="IMapToApiVersionConventionBuilder"/>.</param>
        /// <param name="apiVersions">The <see cref="IEnumerable{T}">sequence</see> of <see cref="ApiVersion">API versions</see> supported by the controller.</param>
        /// <returns>The original <paramref name="builder"/>.</returns>
        public static T MapToApiVersions<T>( this T builder, IEnumerable<ApiVersion> apiVersions ) where T : notnull, IMapToApiVersionConventionBuilder
        {
            if ( apiVersions == null)
            {
                throw new ArgumentNullException( nameof( apiVersions ) );
            }

            foreach ( var apiVersion in apiVersions )
            {
                builder.MapToApiVersion( apiVersion );
            }

            return builder;
        }
    }
}