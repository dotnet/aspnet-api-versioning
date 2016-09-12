#if WEBAPI
namespace Microsoft.Web.Http.Versioning.Conventions
#else
namespace Microsoft.AspNetCore.Mvc.Versioning.Conventions
#endif
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.Contracts;
#if WEBAPI
    using System.Web.Http.Controllers;
#endif

    /// <summary>
    /// Provides extension methods for <see cref="ActionApiVersionConventionBuilder{T}"/>.
    /// </summary>
#if !WEBAPI
    [CLSCompliant( false )]
#endif
    public static class ActionApiVersionConventionBuilderExtensions
    {
        /// <summary>
        /// Indicates that the specified API version is mapped to the configured controller action.
        /// </summary>
        /// <typeparam name="T">The type of controller.</typeparam>
        /// <param name="builder">The extended <see cref="ActionApiVersionConventionBuilder{T}"/>.</param>
        /// <param name="majorVersion">The value for a major version only scheme.</param>
        /// <returns>The original <see cref="ActionApiVersionConventionBuilder{T}"/>.</returns>
        public static ActionApiVersionConventionBuilder<T> MapToApiVersion<T>( this ActionApiVersionConventionBuilder<T> builder, int majorVersion )
#if WEBAPI
            where T : IHttpController
#endif
        {
            Arg.NotNull( builder, nameof( builder ) );
            Contract.Ensures( Contract.Result<ActionApiVersionConventionBuilder<T>>() != null );
            Arg.GreaterThanOrEqualTo( majorVersion, 0, nameof( majorVersion ) );

            builder.MapToApiVersion( new ApiVersion( majorVersion, 0 ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is mapped to the configured controller action.
        /// </summary>
        /// <typeparam name="T">The type of controller.</typeparam>
        /// <param name="builder">The extended <see cref="ActionApiVersionConventionBuilder{T}"/>.</param>
        /// <param name="majorVersion">The value for a major version only scheme.</param>
        /// <param name="status">The version status.</param>
        /// <returns>The original <see cref="ActionApiVersionConventionBuilder{T}"/>.</returns>
        public static ActionApiVersionConventionBuilder<T> MapToApiVersion<T>( this ActionApiVersionConventionBuilder<T> builder, int majorVersion, string status )
#if WEBAPI
            where T : IHttpController
#endif
        {
            Arg.NotNull( builder, nameof( builder ) );
            Contract.Ensures( Contract.Result<ActionApiVersionConventionBuilder<T>>() != null );
            Arg.GreaterThanOrEqualTo( majorVersion, 0, nameof( majorVersion ) );

            builder.MapToApiVersion( new ApiVersion( majorVersion, 0, status ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is mapped to the configured controller action.
        /// </summary>
        /// <typeparam name="T">The type of controller.</typeparam>
        /// <param name="builder">The extended <see cref="ActionApiVersionConventionBuilder{T}"/>.</param>
        /// <param name="majorVersion">The major version number.</param>
        /// <param name="minorVersion">The minor version number.</param>
        /// <returns>The original <see cref="ActionApiVersionConventionBuilder{T}"/>.</returns>
        public static ActionApiVersionConventionBuilder<T> MapToApiVersion<T>( this ActionApiVersionConventionBuilder<T> builder, int majorVersion, int minorVersion )
#if WEBAPI
            where T : IHttpController
#endif
        {
            Arg.NotNull( builder, nameof( builder ) );
            Contract.Ensures( Contract.Result<ActionApiVersionConventionBuilder<T>>() != null );
            Arg.GreaterThanOrEqualTo( majorVersion, 0, nameof( majorVersion ) );
            Arg.GreaterThanOrEqualTo( minorVersion, 0, nameof( minorVersion ) );

            builder.MapToApiVersion( new ApiVersion( majorVersion, minorVersion ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is mapped to the configured controller action.
        /// </summary>
        /// <typeparam name="T">The type of controller.</typeparam>
        /// <param name="builder">The extended <see cref="ActionApiVersionConventionBuilder{T}"/>.</param>
        /// <param name="majorVersion">The major version number.</param>
        /// <param name="minorVersion">The minor version number.</param>
        /// <param name="status">The version status.</param>
        /// <returns>The original <see cref="ActionApiVersionConventionBuilder{T}"/>.</returns>
        public static ActionApiVersionConventionBuilder<T> MapToApiVersion<T>( this ActionApiVersionConventionBuilder<T> builder, int majorVersion, int minorVersion, string status )
#if WEBAPI
            where T : IHttpController
#endif
        {
            Arg.NotNull( builder, nameof( builder ) );
            Arg.NotNullOrEmpty( status, nameof( status ) );
            Contract.Ensures( Contract.Result<ActionApiVersionConventionBuilder<T>>() != null );
            Arg.GreaterThanOrEqualTo( majorVersion, 0, nameof( majorVersion ) );
            Arg.GreaterThanOrEqualTo( minorVersion, 0, nameof( minorVersion ) );

            builder.MapToApiVersion( new ApiVersion( majorVersion, minorVersion, status ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is mapped to the configured controller action.
        /// </summary>
        /// <typeparam name="T">The type of controller.</typeparam>
        /// <param name="builder">The extended <see cref="ActionApiVersionConventionBuilder{T}"/>.</param>
        /// <param name="year">The version year.</param>
        /// <param name="month">The version month.</param>
        /// <param name="day">The version day.</param>
        /// <returns>The original <see cref="ActionApiVersionConventionBuilder{T}"/>.</returns>
        public static ActionApiVersionConventionBuilder<T> MapToApiVersion<T>( this ActionApiVersionConventionBuilder<T> builder, int year, int month, int day )
#if WEBAPI
            where T : IHttpController
#endif
        {
            Arg.NotNull( builder, nameof( builder ) );
            Contract.Ensures( Contract.Result<ActionApiVersionConventionBuilder<T>>() != null );
            Arg.InRange( year, 1, 9999, nameof( year ) );
            Arg.InRange( month, 1, 12, nameof( month ) );
            Arg.InRange( day, 1, 31, nameof( day ) );

            builder.MapToApiVersion( new ApiVersion( new DateTime( year, month, day ) ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is mapped to the configured controller action.
        /// </summary>
        /// <typeparam name="T">The type of controller.</typeparam>
        /// <param name="builder">The extended <see cref="ActionApiVersionConventionBuilder{T}"/>.</param>
        /// <param name="year">The version year.</param>
        /// <param name="month">The version month.</param>
        /// <param name="day">The version day.</param>
        /// <param name="status">The version status.</param>
        /// <returns>The original <see cref="ActionApiVersionConventionBuilder{T}"/>.</returns>
        public static ActionApiVersionConventionBuilder<T> MapToApiVersion<T>( this ActionApiVersionConventionBuilder<T> builder, int year, int month, int day, string status )
#if WEBAPI
            where T : IHttpController
#endif
        {
            Arg.NotNull( builder, nameof( builder ) );
            Arg.NotNullOrEmpty( status, nameof( status ) );
            Contract.Ensures( Contract.Result<ActionApiVersionConventionBuilder<T>>() != null );
            Arg.InRange( year, 1, 9999, nameof( year ) );
            Arg.InRange( month, 1, 12, nameof( month ) );
            Arg.InRange( day, 1, 31, nameof( day ) );

            builder.MapToApiVersion( new ApiVersion( new DateTime( year, month, day ), status ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is mapped to the configured controller action.
        /// </summary>
        /// <typeparam name="T">The type of controller.</typeparam>
        /// <param name="builder">The extended <see cref="ActionApiVersionConventionBuilder{T}"/>.</param>
        /// <param name="groupVersion">The group version.</param>
        /// <returns>The original <see cref="ActionApiVersionConventionBuilder{T}"/>.</returns>
        public static ActionApiVersionConventionBuilder<T> MapToApiVersion<T>( this ActionApiVersionConventionBuilder<T> builder, DateTime groupVersion )
#if WEBAPI
            where T : IHttpController
#endif
        {
            Arg.NotNull( builder, nameof( builder ) );
            Contract.Ensures( Contract.Result<ActionApiVersionConventionBuilder<T>>() != null );

            builder.MapToApiVersion( new ApiVersion( groupVersion ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is mapped to the configured controller action.
        /// </summary>
        /// <typeparam name="T">The type of controller.</typeparam>
        /// <param name="builder">The extended <see cref="ActionApiVersionConventionBuilder{T}"/>.</param>
        /// <param name="groupVersion">The group version.</param>
        /// <param name="status">The version status.</param>
        /// <returns>The original <see cref="ActionApiVersionConventionBuilder{T}"/>.</returns>
        public static ActionApiVersionConventionBuilder<T> MapToApiVersion<T>( this ActionApiVersionConventionBuilder<T> builder, DateTime groupVersion, string status )
#if WEBAPI
            where T : IHttpController
#endif
        {
            Arg.NotNull( builder, nameof( builder ) );
            Arg.NotNullOrEmpty( status, nameof( status ) );
            Contract.Ensures( Contract.Result<ActionApiVersionConventionBuilder<T>>() != null );

            builder.MapToApiVersion( new ApiVersion( groupVersion, status ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API versions are mapped to the configured controller action.
        /// </summary>
        /// <typeparam name="T">The type of controller.</typeparam>
        /// <param name="builder">The extended <see cref="ActionApiVersionConventionBuilder{T}"/>.</param>
        /// <param name="apiVersions">The <see cref="IEnumerable{T}">sequence</see> of <see cref="ApiVersion">API versions</see> supported by the controller.</param>
        /// <returns>The original <see cref="ActionApiVersionConventionBuilder{T}"/>.</returns>
        public static ActionApiVersionConventionBuilder<T> MapToApiVersions<T>( this ActionApiVersionConventionBuilder<T> builder, IEnumerable<ApiVersion> apiVersions )
#if WEBAPI
            where T : IHttpController
#endif
        {
            Arg.NotNull( builder, nameof( builder ) );
            Arg.NotNull( apiVersions, nameof( apiVersions ) );
            Contract.Ensures( Contract.Result<ActionApiVersionConventionBuilder<T>>() != null );

            foreach ( var apiVersion in apiVersions )
            {
                builder.MapToApiVersion( apiVersion );
            }

            return builder;
        }
    }
}