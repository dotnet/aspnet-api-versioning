#if WEBAPI
namespace Microsoft.Web.Http.Versioning.Conventions
#else
namespace Microsoft.AspNetCore.Mvc.Versioning.Conventions
#endif
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Provides extension methods for <see cref="ControllerApiVersionConventionBuilder"/> and <see cref="ControllerApiVersionConventionBuilder{T}"/> types.
    /// </summary>
#if !WEBAPI
    [CLSCompliant( false )]
#endif
    public static partial class ControllerApiVersionConventionBuilderExtensions
    {
        /// <summary>
        /// Indicates that the specified API version is supported by the configured controller.
        /// </summary>
        /// <param name="builder">The extended <see cref="ControllerApiVersionConventionBuilder"/>.</param>
        /// <param name="majorVersion">The value for a major version only scheme.</param>
        /// <returns>The original <see cref="ControllerApiVersionConventionBuilder"/>.</returns>
        public static ControllerApiVersionConventionBuilder HasApiVersion( this ControllerApiVersionConventionBuilder builder, int majorVersion )
        {
            Arg.NotNull( builder, nameof( builder ) );
            Contract.Ensures( Contract.Result<ControllerApiVersionConventionBuilder>() != null );
            Arg.GreaterThanOrEqualTo( majorVersion, 0, nameof( majorVersion ) );

            builder.HasApiVersion( new ApiVersion( majorVersion, 0 ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is supported by the configured controller.
        /// </summary>
        /// <param name="builder">The extended <see cref="ControllerApiVersionConventionBuilder"/>.</param>
        /// <param name="majorVersion">The value for a major version only scheme.</param>
        /// <param name="status">The version status.</param>
        /// <returns>The original <see cref="ControllerApiVersionConventionBuilder"/>.</returns>
        public static ControllerApiVersionConventionBuilder HasApiVersion( this ControllerApiVersionConventionBuilder builder, int majorVersion, string status )
        {
            Arg.NotNull( builder, nameof( builder ) );
            Contract.Ensures( Contract.Result<ControllerApiVersionConventionBuilder>() != null );
            Arg.GreaterThanOrEqualTo( majorVersion, 0, nameof( majorVersion ) );

            builder.HasApiVersion( new ApiVersion( majorVersion, 0, status ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is supported by the configured controller.
        /// </summary>
        /// <param name="builder">The extended <see cref="ControllerApiVersionConventionBuilder"/>.</param>
        /// <param name="majorVersion">The major version number.</param>
        /// <param name="minorVersion">The minor version number.</param>
        /// <returns>The original <see cref="ControllerApiVersionConventionBuilder"/>.</returns>
        public static ControllerApiVersionConventionBuilder HasApiVersion( this ControllerApiVersionConventionBuilder builder, int majorVersion, int minorVersion )
        {
            Arg.NotNull( builder, nameof( builder ) );
            Contract.Ensures( Contract.Result<ControllerApiVersionConventionBuilder>() != null );
            Arg.GreaterThanOrEqualTo( majorVersion, 0, nameof( majorVersion ) );
            Arg.GreaterThanOrEqualTo( minorVersion, 0, nameof( minorVersion ) );

            builder.HasApiVersion( new ApiVersion( majorVersion, minorVersion ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is supported by the configured controller.
        /// </summary>
        /// <param name="builder">The extended <see cref="ControllerApiVersionConventionBuilder"/>.</param>
        /// <param name="majorVersion">The major version number.</param>
        /// <param name="minorVersion">The minor version number.</param>
        /// <param name="status">The version status.</param>
        /// <returns>The original <see cref="ControllerApiVersionConventionBuilder"/>.</returns>
        public static ControllerApiVersionConventionBuilder HasApiVersion( this ControllerApiVersionConventionBuilder builder, int majorVersion, int minorVersion, string status )
        {
            Arg.NotNull( builder, nameof( builder ) );
            Arg.NotNullOrEmpty( status, nameof( status ) );
            Contract.Ensures( Contract.Result<ControllerApiVersionConventionBuilder>() != null );
            Arg.GreaterThanOrEqualTo( majorVersion, 0, nameof( majorVersion ) );
            Arg.GreaterThanOrEqualTo( minorVersion, 0, nameof( minorVersion ) );

            builder.HasApiVersion( new ApiVersion( majorVersion, minorVersion, status ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is supported by the configured controller.
        /// </summary>
        /// <param name="builder">The extended <see cref="ControllerApiVersionConventionBuilder"/>.</param>
        /// <param name="year">The version year.</param>
        /// <param name="month">The version month.</param>
        /// <param name="day">The version day.</param>
        /// <returns>The original <see cref="ControllerApiVersionConventionBuilder"/>.</returns>
        public static ControllerApiVersionConventionBuilder HasApiVersion( this ControllerApiVersionConventionBuilder builder, int year, int month, int day )
        {
            Arg.NotNull( builder, nameof( builder ) );
            Contract.Ensures( Contract.Result<ControllerApiVersionConventionBuilder>() != null );
            Arg.InRange( year, 1, 9999, nameof( year ) );
            Arg.InRange( month, 1, 12, nameof( month ) );
            Arg.InRange( day, 1, 31, nameof( day ) );

            builder.HasApiVersion( new ApiVersion( new DateTime( year, month, day ) ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is supported by the configured controller.
        /// </summary>
        /// <param name="builder">The extended <see cref="ControllerApiVersionConventionBuilder"/>.</param>
        /// <param name="year">The version year.</param>
        /// <param name="month">The version month.</param>
        /// <param name="day">The version day.</param>
        /// <param name="status">The version status.</param>
        /// <returns>The original <see cref="ControllerApiVersionConventionBuilder"/>.</returns>
        public static ControllerApiVersionConventionBuilder HasApiVersion( this ControllerApiVersionConventionBuilder builder, int year, int month, int day, string status )
        {
            Arg.NotNull( builder, nameof( builder ) );
            Arg.NotNullOrEmpty( status, nameof( status ) );
            Contract.Ensures( Contract.Result<ControllerApiVersionConventionBuilder>() != null );
            Arg.InRange( year, 1, 9999, nameof( year ) );
            Arg.InRange( month, 1, 12, nameof( month ) );
            Arg.InRange( day, 1, 31, nameof( day ) );

            builder.HasApiVersion( new ApiVersion( new DateTime( year, month, day ), status ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is supported by the configured controller.
        /// </summary>
        /// <param name="builder">The extended <see cref="ControllerApiVersionConventionBuilder"/>.</param>
        /// <param name="groupVersion">The group version.</param>
        /// <returns>The original <see cref="ControllerApiVersionConventionBuilder"/>.</returns>
        public static ControllerApiVersionConventionBuilder HasApiVersion( this ControllerApiVersionConventionBuilder builder, DateTime groupVersion )
        {
            Arg.NotNull( builder, nameof( builder ) );
            Contract.Ensures( Contract.Result<ControllerApiVersionConventionBuilder>() != null );

            builder.HasApiVersion( new ApiVersion( groupVersion ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is supported by the configured controller.
        /// </summary>
        /// <param name="builder">The extended <see cref="ControllerApiVersionConventionBuilder"/>.</param>
        /// <param name="groupVersion">The group version.</param>
        /// <param name="status">The version status.</param>
        /// <returns>The original <see cref="ControllerApiVersionConventionBuilder"/>.</returns>
        public static ControllerApiVersionConventionBuilder HasApiVersion( this ControllerApiVersionConventionBuilder builder, DateTime groupVersion, string status )
        {
            Arg.NotNull( builder, nameof( builder ) );
            Arg.NotNullOrEmpty( status, nameof( status ) );
            Contract.Ensures( Contract.Result<ControllerApiVersionConventionBuilder>() != null );

            builder.HasApiVersion( new ApiVersion( groupVersion, status ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API versions are supported by the configured controller.
        /// </summary>
        /// <param name="builder">The extended <see cref="ControllerApiVersionConventionBuilder"/>.</param>
        /// <param name="apiVersions">The <see cref="IEnumerable{T}">sequence</see> of <see cref="ApiVersion">API versions</see> supported by the controller.</param>
        /// <returns>The original <see cref="ControllerApiVersionConventionBuilder"/>.</returns>
        public static ControllerApiVersionConventionBuilder HasApiVersions( this ControllerApiVersionConventionBuilder builder, IEnumerable<ApiVersion> apiVersions )
        {
            Arg.NotNull( builder, nameof( builder ) );
            Arg.NotNull( apiVersions, nameof( apiVersions ) );
            Contract.Ensures( Contract.Result<ControllerApiVersionConventionBuilder>() != null );

            foreach ( var apiVersion in apiVersions )
            {
                builder.HasApiVersion( apiVersion );
            }

            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is deprecated by the configured controller.
        /// </summary>
        /// <param name="builder">The extended <see cref="ControllerApiVersionConventionBuilder"/>.</param>
        /// <param name="majorVersion">The value for a major version only scheme.</param>
        /// <returns>The original <see cref="ControllerApiVersionConventionBuilder"/>.</returns>
        public static ControllerApiVersionConventionBuilder HasDeprecatedApiVersion( this ControllerApiVersionConventionBuilder builder, int majorVersion )
        {
            Arg.NotNull( builder, nameof( builder ) );
            Contract.Ensures( Contract.Result<ControllerApiVersionConventionBuilder>() != null );
            Arg.GreaterThanOrEqualTo( majorVersion, 0, nameof( majorVersion ) );

            builder.HasDeprecatedApiVersion( new ApiVersion( majorVersion, 0 ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is deprecated by the configured controller.
        /// </summary>
        /// <param name="builder">The extended <see cref="ControllerApiVersionConventionBuilder"/>.</param>
        /// <param name="majorVersion">The value for a major version only scheme.</param>
        /// <param name="status">The version status.</param>
        /// <returns>The original <see cref="ControllerApiVersionConventionBuilder"/>.</returns>
        public static ControllerApiVersionConventionBuilder HasDeprecatedApiVersion( this ControllerApiVersionConventionBuilder builder, int majorVersion, string status )
        {
            Arg.NotNull( builder, nameof( builder ) );
            Contract.Ensures( Contract.Result<ControllerApiVersionConventionBuilder>() != null );
            Arg.GreaterThanOrEqualTo( majorVersion, 0, nameof( majorVersion ) );

            builder.HasDeprecatedApiVersion( new ApiVersion( majorVersion, 0, status ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is deprecated by the configured controller.
        /// </summary>
        /// <param name="builder">The extended <see cref="ControllerApiVersionConventionBuilder"/>.</param>
        /// <param name="majorVersion">The major version number.</param>
        /// <param name="minorVersion">The minor version number.</param>
        /// <returns>The original <see cref="ControllerApiVersionConventionBuilder"/>.</returns>
        public static ControllerApiVersionConventionBuilder HasDeprecatedApiVersion( this ControllerApiVersionConventionBuilder builder, int majorVersion, int minorVersion )
        {
            Arg.NotNull( builder, nameof( builder ) );
            Contract.Ensures( Contract.Result<ControllerApiVersionConventionBuilder>() != null );
            Arg.GreaterThanOrEqualTo( majorVersion, 0, nameof( majorVersion ) );
            Arg.GreaterThanOrEqualTo( minorVersion, 0, nameof( minorVersion ) );

            builder.HasDeprecatedApiVersion( new ApiVersion( majorVersion, minorVersion ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is deprecated by the configured controller.
        /// </summary>
        /// <param name="builder">The extended <see cref="ControllerApiVersionConventionBuilder"/>.</param>
        /// <param name="majorVersion">The major version number.</param>
        /// <param name="minorVersion">The minor version number.</param>
        /// <param name="status">The version status.</param>
        /// <returns>The original <see cref="ControllerApiVersionConventionBuilder"/>.</returns>
        public static ControllerApiVersionConventionBuilder HasDeprecatedApiVersion( this ControllerApiVersionConventionBuilder builder, int majorVersion, int minorVersion, string status )
        {
            Arg.NotNull( builder, nameof( builder ) );
            Arg.NotNullOrEmpty( status, nameof( status ) );
            Contract.Ensures( Contract.Result<ControllerApiVersionConventionBuilder>() != null );
            Arg.GreaterThanOrEqualTo( majorVersion, 0, nameof( majorVersion ) );
            Arg.GreaterThanOrEqualTo( minorVersion, 0, nameof( minorVersion ) );

            builder.HasDeprecatedApiVersion( new ApiVersion( majorVersion, minorVersion, status ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is deprecated by the configured controller.
        /// </summary>
        /// <param name="builder">The extended <see cref="ControllerApiVersionConventionBuilder"/>.</param>
        /// <param name="year">The version year.</param>
        /// <param name="month">The version month.</param>
        /// <param name="day">The version day.</param>
        /// <returns>The original <see cref="ControllerApiVersionConventionBuilder"/>.</returns>
        public static ControllerApiVersionConventionBuilder HasDeprecatedApiVersion( this ControllerApiVersionConventionBuilder builder, int year, int month, int day )
        {
            Arg.NotNull( builder, nameof( builder ) );
            Contract.Ensures( Contract.Result<ControllerApiVersionConventionBuilder>() != null );
            Arg.InRange( year, 1, 9999, nameof( year ) );
            Arg.InRange( month, 1, 12, nameof( month ) );
            Arg.InRange( day, 1, 31, nameof( day ) );

            builder.HasDeprecatedApiVersion( new ApiVersion( new DateTime( year, month, day ) ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is deprecated by the configured controller.
        /// </summary>
        /// <param name="builder">The extended <see cref="ControllerApiVersionConventionBuilder"/>.</param>
        /// <param name="year">The version year.</param>
        /// <param name="month">The version month.</param>
        /// <param name="day">The version day.</param>
        /// <param name="status">The version status.</param>
        /// <returns>The original <see cref="ControllerApiVersionConventionBuilder"/>.</returns>
        public static ControllerApiVersionConventionBuilder HasDeprecatedApiVersion( this ControllerApiVersionConventionBuilder builder, int year, int month, int day, string status )
        {
            Arg.NotNull( builder, nameof( builder ) );
            Arg.NotNullOrEmpty( status, nameof( status ) );
            Contract.Ensures( Contract.Result<ControllerApiVersionConventionBuilder>() != null );
            Arg.InRange( year, 1, 9999, nameof( year ) );
            Arg.InRange( month, 1, 12, nameof( month ) );
            Arg.InRange( day, 1, 31, nameof( day ) );

            builder.HasDeprecatedApiVersion( new ApiVersion( new DateTime( year, month, day ), status ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is deprecated by the configured controller.
        /// </summary>
        /// <param name="builder">The extended <see cref="ControllerApiVersionConventionBuilder"/>.</param>
        /// <param name="groupVersion">The group version.</param>
        /// <returns>The original <see cref="ControllerApiVersionConventionBuilder"/>.</returns>
        public static ControllerApiVersionConventionBuilder HasDeprecatedApiVersion( this ControllerApiVersionConventionBuilder builder, DateTime groupVersion )
        {
            Arg.NotNull( builder, nameof( builder ) );
            Contract.Ensures( Contract.Result<ControllerApiVersionConventionBuilder>() != null );

            builder.HasDeprecatedApiVersion( new ApiVersion( groupVersion ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is deprecated by the configured controller.
        /// </summary>
        /// <param name="builder">The extended <see cref="ControllerApiVersionConventionBuilder"/>.</param>
        /// <param name="groupVersion">The group version.</param>
        /// <param name="status">The version status.</param>
        /// <returns>The original <see cref="ControllerApiVersionConventionBuilder"/>.</returns>
        public static ControllerApiVersionConventionBuilder HasDeprecatedApiVersion( this ControllerApiVersionConventionBuilder builder, DateTime groupVersion, string status )
        {
            Arg.NotNull( builder, nameof( builder ) );
            Arg.NotNullOrEmpty( status, nameof( status ) );
            Contract.Ensures( Contract.Result<ControllerApiVersionConventionBuilder>() != null );

            builder.HasDeprecatedApiVersion( new ApiVersion( groupVersion, status ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API versions are deprecated by the configured controller.
        /// </summary>
        /// <param name="builder">The extended <see cref="ControllerApiVersionConventionBuilder"/>.</param>
        /// <param name="apiVersions">The <see cref="IEnumerable{T}">sequence</see> of <see cref="ApiVersion">API versions</see> deprecated by the controller.</param>
        /// <returns>The original <see cref="ControllerApiVersionConventionBuilder"/>.</returns>
        public static ControllerApiVersionConventionBuilder HasDeprecatedApiVersions( this ControllerApiVersionConventionBuilder builder, IEnumerable<ApiVersion> apiVersions )
        {
            Arg.NotNull( builder, nameof( builder ) );
            Arg.NotNull( apiVersions, nameof( apiVersions ) );
            Contract.Ensures( Contract.Result<ControllerApiVersionConventionBuilder>() != null );

            foreach ( var apiVersion in apiVersions )
            {
                builder.HasDeprecatedApiVersion( apiVersion );
            }

            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is advertised by the configured controller.
        /// </summary>
        /// <param name="builder">The extended <see cref="ControllerApiVersionConventionBuilder"/>.</param>
        /// <param name="majorVersion">The value for a major version only scheme.</param>
        /// <returns>The original <see cref="ControllerApiVersionConventionBuilder"/>.</returns>
        public static ControllerApiVersionConventionBuilder AdvertisesApiVersion( this ControllerApiVersionConventionBuilder builder, int majorVersion )
        {
            Arg.NotNull( builder, nameof( builder ) );
            Contract.Ensures( Contract.Result<ControllerApiVersionConventionBuilder>() != null );
            Arg.GreaterThanOrEqualTo( majorVersion, 0, nameof( majorVersion ) );

            builder.AdvertisesApiVersion( new ApiVersion( majorVersion, 0 ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is advertised by the configured controller.
        /// </summary>
        /// <param name="builder">The extended <see cref="ControllerApiVersionConventionBuilder"/>.</param>
        /// <param name="majorVersion">The value for a major version only scheme.</param>
        /// <param name="status">The version status.</param>
        /// <returns>The original <see cref="ControllerApiVersionConventionBuilder"/>.</returns>
        public static ControllerApiVersionConventionBuilder AdvertisesApiVersion( this ControllerApiVersionConventionBuilder builder, int majorVersion, string status )
        {
            Arg.NotNull( builder, nameof( builder ) );
            Contract.Ensures( Contract.Result<ControllerApiVersionConventionBuilder>() != null );
            Arg.GreaterThanOrEqualTo( majorVersion, 0, nameof( majorVersion ) );

            builder.AdvertisesApiVersion( new ApiVersion( majorVersion, 0, status ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is advertised by the configured controller.
        /// </summary>
        /// <param name="builder">The extended <see cref="ControllerApiVersionConventionBuilder"/>.</param>
        /// <param name="majorVersion">The major version number.</param>
        /// <param name="minorVersion">The minor version number.</param>
        /// <returns>The original <see cref="ControllerApiVersionConventionBuilder"/>.</returns>
        public static ControllerApiVersionConventionBuilder AdvertisesApiVersion( this ControllerApiVersionConventionBuilder builder, int majorVersion, int minorVersion )
        {
            Arg.NotNull( builder, nameof( builder ) );
            Contract.Ensures( Contract.Result<ControllerApiVersionConventionBuilder>() != null );
            Arg.GreaterThanOrEqualTo( majorVersion, 0, nameof( majorVersion ) );
            Arg.GreaterThanOrEqualTo( minorVersion, 0, nameof( minorVersion ) );

            builder.AdvertisesApiVersion( new ApiVersion( majorVersion, minorVersion ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is advertised by the configured controller.
        /// </summary>
        /// <param name="builder">The extended <see cref="ControllerApiVersionConventionBuilder"/>.</param>
        /// <param name="majorVersion">The major version number.</param>
        /// <param name="minorVersion">The minor version number.</param>
        /// <param name="status">The version status.</param>
        /// <returns>The original <see cref="ControllerApiVersionConventionBuilder"/>.</returns>
        public static ControllerApiVersionConventionBuilder AdvertisesApiVersion( this ControllerApiVersionConventionBuilder builder, int majorVersion, int minorVersion, string status )
        {
            Arg.NotNull( builder, nameof( builder ) );
            Arg.NotNullOrEmpty( status, nameof( status ) );
            Contract.Ensures( Contract.Result<ControllerApiVersionConventionBuilder>() != null );
            Arg.GreaterThanOrEqualTo( majorVersion, 0, nameof( majorVersion ) );
            Arg.GreaterThanOrEqualTo( minorVersion, 0, nameof( minorVersion ) );

            builder.AdvertisesApiVersion( new ApiVersion( majorVersion, minorVersion, status ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is advertised by the configured controller.
        /// </summary>
        /// <param name="builder">The extended <see cref="ControllerApiVersionConventionBuilder"/>.</param>
        /// <param name="year">The version year.</param>
        /// <param name="month">The version month.</param>
        /// <param name="day">The version day.</param>
        /// <returns>The original <see cref="ControllerApiVersionConventionBuilder"/>.</returns>
        public static ControllerApiVersionConventionBuilder AdvertisesApiVersion( this ControllerApiVersionConventionBuilder builder, int year, int month, int day )
        {
            Arg.NotNull( builder, nameof( builder ) );
            Contract.Ensures( Contract.Result<ControllerApiVersionConventionBuilder>() != null );
            Arg.InRange( year, 1, 9999, nameof( year ) );
            Arg.InRange( month, 1, 12, nameof( month ) );
            Arg.InRange( day, 1, 31, nameof( day ) );

            builder.AdvertisesApiVersion( new ApiVersion( new DateTime( year, month, day ) ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is advertised by the configured controller.
        /// </summary>
        /// <param name="builder">The extended <see cref="ControllerApiVersionConventionBuilder"/>.</param>
        /// <param name="year">The version year.</param>
        /// <param name="month">The version month.</param>
        /// <param name="day">The version day.</param>
        /// <param name="status">The version status.</param>
        /// <returns>The original <see cref="ControllerApiVersionConventionBuilder"/>.</returns>
        public static ControllerApiVersionConventionBuilder AdvertisesApiVersion( this ControllerApiVersionConventionBuilder builder, int year, int month, int day, string status )
        {
            Arg.NotNull( builder, nameof( builder ) );
            Arg.NotNullOrEmpty( status, nameof( status ) );
            Contract.Ensures( Contract.Result<ControllerApiVersionConventionBuilder>() != null );
            Arg.InRange( year, 1, 9999, nameof( year ) );
            Arg.InRange( month, 1, 12, nameof( month ) );
            Arg.InRange( day, 1, 31, nameof( day ) );

            builder.AdvertisesApiVersion( new ApiVersion( new DateTime( year, month, day ), status ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is advertised by the configured controller.
        /// </summary>
        /// <param name="builder">The extended <see cref="ControllerApiVersionConventionBuilder"/>.</param>
        /// <param name="groupVersion">The group version.</param>
        /// <returns>The original <see cref="ControllerApiVersionConventionBuilder"/>.</returns>
        public static ControllerApiVersionConventionBuilder AdvertisesApiVersion( this ControllerApiVersionConventionBuilder builder, DateTime groupVersion )
        {
            Arg.NotNull( builder, nameof( builder ) );
            Contract.Ensures( Contract.Result<ControllerApiVersionConventionBuilder>() != null );

            builder.AdvertisesApiVersion( new ApiVersion( groupVersion ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is advertised by the configured controller.
        /// </summary>
        /// <param name="builder">The extended <see cref="ControllerApiVersionConventionBuilder"/>.</param>
        /// <param name="groupVersion">The group version.</param>
        /// <param name="status">The version status.</param>
        /// <returns>The original <see cref="ControllerApiVersionConventionBuilder"/>.</returns>
        public static ControllerApiVersionConventionBuilder AdvertisesApiVersion( this ControllerApiVersionConventionBuilder builder, DateTime groupVersion, string status )
        {
            Arg.NotNull( builder, nameof( builder ) );
            Arg.NotNullOrEmpty( status, nameof( status ) );
            Contract.Ensures( Contract.Result<ControllerApiVersionConventionBuilder>() != null );

            builder.AdvertisesApiVersion( new ApiVersion( groupVersion, status ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API versions are advertised by the configured controller.
        /// </summary>
        /// <param name="builder">The extended <see cref="ControllerApiVersionConventionBuilder"/>.</param>
        /// <param name="apiVersions">The <see cref="IEnumerable{T}">sequence</see> of <see cref="ApiVersion">API versions</see> advertised by the controller.</param>
        /// <returns>The original <see cref="ControllerApiVersionConventionBuilder"/>.</returns>
        public static ControllerApiVersionConventionBuilder AdvertisesApiVersions( this ControllerApiVersionConventionBuilder builder, IEnumerable<ApiVersion> apiVersions )
        {
            Arg.NotNull( builder, nameof( builder ) );
            Arg.NotNull( apiVersions, nameof( apiVersions ) );
            Contract.Ensures( Contract.Result<ControllerApiVersionConventionBuilder>() != null );

            foreach ( var apiVersion in apiVersions )
            {
                builder.AdvertisesApiVersion( apiVersion );
            }

            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is advertised and deprecated by the configured controller.
        /// </summary>
        /// <param name="builder">The extended <see cref="ControllerApiVersionConventionBuilder"/>.</param>
        /// <param name="majorVersion">The value for a major version only scheme.</param>
        /// <returns>The original <see cref="ControllerApiVersionConventionBuilder"/>.</returns>
        public static ControllerApiVersionConventionBuilder AdvertisesDeprecatedApiVersion( this ControllerApiVersionConventionBuilder builder, int majorVersion )
        {
            Arg.NotNull( builder, nameof( builder ) );
            Contract.Ensures( Contract.Result<ControllerApiVersionConventionBuilder>() != null );
            Arg.GreaterThanOrEqualTo( majorVersion, 0, nameof( majorVersion ) );

            builder.AdvertisesDeprecatedApiVersion( new ApiVersion( majorVersion, 0 ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is advertised and deprecated by the configured controller.
        /// </summary>
        /// <param name="builder">The extended <see cref="ControllerApiVersionConventionBuilder"/>.</param>
        /// <param name="majorVersion">The value for a major version only scheme.</param>
        /// <param name="status">The version status.</param>
        /// <returns>The original <see cref="ControllerApiVersionConventionBuilder"/>.</returns>
        public static ControllerApiVersionConventionBuilder AdvertisesDeprecatedApiVersion( this ControllerApiVersionConventionBuilder builder, int majorVersion, string status )
        {
            Arg.NotNull( builder, nameof( builder ) );
            Contract.Ensures( Contract.Result<ControllerApiVersionConventionBuilder>() != null );
            Arg.GreaterThanOrEqualTo( majorVersion, 0, nameof( majorVersion ) );

            builder.AdvertisesDeprecatedApiVersion( new ApiVersion( majorVersion, 0, status ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is advertised and deprecated by the configured controller.
        /// </summary>
        /// <param name="builder">The extended <see cref="ControllerApiVersionConventionBuilder"/>.</param>
        /// <param name="majorVersion">The major version number.</param>
        /// <param name="minorVersion">The minor version number.</param>
        /// <returns>The original <see cref="ControllerApiVersionConventionBuilder"/>.</returns>
        public static ControllerApiVersionConventionBuilder AdvertisesDeprecatedApiVersion( this ControllerApiVersionConventionBuilder builder, int majorVersion, int minorVersion )
        {
            Arg.NotNull( builder, nameof( builder ) );
            Contract.Ensures( Contract.Result<ControllerApiVersionConventionBuilder>() != null );
            Arg.GreaterThanOrEqualTo( majorVersion, 0, nameof( majorVersion ) );
            Arg.GreaterThanOrEqualTo( minorVersion, 0, nameof( minorVersion ) );

            builder.AdvertisesDeprecatedApiVersion( new ApiVersion( majorVersion, minorVersion ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is advertised and deprecated by the configured controller.
        /// </summary>
        /// <param name="builder">The extended <see cref="ControllerApiVersionConventionBuilder"/>.</param>
        /// <param name="majorVersion">The major version number.</param>
        /// <param name="minorVersion">The minor version number.</param>
        /// <param name="status">The version status.</param>
        /// <returns>The original <see cref="ControllerApiVersionConventionBuilder"/>.</returns>
        public static ControllerApiVersionConventionBuilder AdvertisesDeprecatedApiVersion( this ControllerApiVersionConventionBuilder builder, int majorVersion, int minorVersion, string status )
        {
            Arg.NotNull( builder, nameof( builder ) );
            Arg.NotNullOrEmpty( status, nameof( status ) );
            Contract.Ensures( Contract.Result<ControllerApiVersionConventionBuilder>() != null );
            Arg.GreaterThanOrEqualTo( majorVersion, 0, nameof( majorVersion ) );
            Arg.GreaterThanOrEqualTo( minorVersion, 0, nameof( minorVersion ) );

            builder.AdvertisesDeprecatedApiVersion( new ApiVersion( majorVersion, minorVersion, status ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is advertised and deprecated by the configured controller.
        /// </summary>
        /// <param name="builder">The extended <see cref="ControllerApiVersionConventionBuilder"/>.</param>
        /// <param name="year">The version year.</param>
        /// <param name="month">The version month.</param>
        /// <param name="day">The version day.</param>
        /// <returns>The original <see cref="ControllerApiVersionConventionBuilder"/>.</returns>
        public static ControllerApiVersionConventionBuilder AdvertisesDeprecatedApiVersion( this ControllerApiVersionConventionBuilder builder, int year, int month, int day )
        {
            Arg.NotNull( builder, nameof( builder ) );
            Contract.Ensures( Contract.Result<ControllerApiVersionConventionBuilder>() != null );
            Arg.InRange( year, 1, 9999, nameof( year ) );
            Arg.InRange( month, 1, 12, nameof( month ) );
            Arg.InRange( day, 1, 31, nameof( day ) );

            builder.AdvertisesDeprecatedApiVersion( new ApiVersion( new DateTime( year, month, day ) ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is advertised and deprecated by the configured controller.
        /// </summary>
        /// <param name="builder">The extended <see cref="ControllerApiVersionConventionBuilder"/>.</param>
        /// <param name="year">The version year.</param>
        /// <param name="month">The version month.</param>
        /// <param name="day">The version day.</param>
        /// <param name="status">The version status.</param>
        /// <returns>The original <see cref="ControllerApiVersionConventionBuilder"/>.</returns>
        public static ControllerApiVersionConventionBuilder AdvertisesDeprecatedApiVersion( this ControllerApiVersionConventionBuilder builder, int year, int month, int day, string status )
        {
            Arg.NotNull( builder, nameof( builder ) );
            Arg.NotNullOrEmpty( status, nameof( status ) );
            Contract.Ensures( Contract.Result<ControllerApiVersionConventionBuilder>() != null );
            Arg.InRange( year, 1, 9999, nameof( year ) );
            Arg.InRange( month, 1, 12, nameof( month ) );
            Arg.InRange( day, 1, 31, nameof( day ) );

            builder.AdvertisesDeprecatedApiVersion( new ApiVersion( new DateTime( year, month, day ), status ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is advertised and deprecated by the configured controller.
        /// </summary>
        /// <param name="builder">The extended <see cref="ControllerApiVersionConventionBuilder"/>.</param>
        /// <param name="groupVersion">The group version.</param>
        /// <returns>The original <see cref="ControllerApiVersionConventionBuilder"/>.</returns>
        public static ControllerApiVersionConventionBuilder AdvertisesDeprecatedApiVersion( this ControllerApiVersionConventionBuilder builder, DateTime groupVersion )
        {
            Arg.NotNull( builder, nameof( builder ) );
            Contract.Ensures( Contract.Result<ControllerApiVersionConventionBuilder>() != null );

            builder.AdvertisesDeprecatedApiVersion( new ApiVersion( groupVersion ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is advertised and deprecated by the configured controller.
        /// </summary>
        /// <param name="builder">The extended <see cref="ControllerApiVersionConventionBuilder"/>.</param>
        /// <param name="groupVersion">The group version.</param>
        /// <param name="status">The version status.</param>
        /// <returns>The original <see cref="ControllerApiVersionConventionBuilder"/>.</returns>
        public static ControllerApiVersionConventionBuilder AdvertisesDeprecatedApiVersion( this ControllerApiVersionConventionBuilder builder, DateTime groupVersion, string status )
        {
            Arg.NotNull( builder, nameof( builder ) );
            Arg.NotNullOrEmpty( status, nameof( status ) );
            Contract.Ensures( Contract.Result<ControllerApiVersionConventionBuilder>() != null );

            builder.AdvertisesDeprecatedApiVersion( new ApiVersion( groupVersion, status ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API versions are advertised and deprecated by the configured controller.
        /// </summary>
        /// <param name="builder">The extended <see cref="ControllerApiVersionConventionBuilder"/>.</param>
        /// <param name="apiVersions">The <see cref="IEnumerable{T}">sequence</see> of deprecated <see cref="ApiVersion">API versions</see> advertised by the controller.</param>
        /// <returns>The original <see cref="ControllerApiVersionConventionBuilder"/>.</returns>
        public static ControllerApiVersionConventionBuilder AdvertisesDeprecatedApiVersions( this ControllerApiVersionConventionBuilder builder, IEnumerable<ApiVersion> apiVersions )
        {
            Arg.NotNull( builder, nameof( builder ) );
            Arg.NotNull( apiVersions, nameof( apiVersions ) );
            Contract.Ensures( Contract.Result<ControllerApiVersionConventionBuilder>() != null );

            foreach ( var apiVersion in apiVersions )
            {
                builder.AdvertisesDeprecatedApiVersion( apiVersion );
            }

            return builder;
        }
    }
}