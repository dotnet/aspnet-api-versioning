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
    /// Provides extension methods for <see cref="ActionApiVersionConventionBuilder"/> and <see cref="ActionApiVersionConventionBuilder"/> types.
    /// </summary>
#if !WEBAPI
    [CLSCompliant( false )]
#endif
    public static partial class ActionApiVersionConventionBuilderExtensions
    {
        /// <summary>
        /// Indicates that the specified API version is mapped to the configured controller action.
        /// </summary>
        /// <param name="builder">The extended <see cref="ActionApiVersionConventionBuilder"/>.</param>
        /// <param name="majorVersion">The value for a major version only scheme.</param>
        /// <returns>The original <see cref="ActionApiVersionConventionBuilder"/>.</returns>
        public static ActionApiVersionConventionBuilder MapToApiVersion( this ActionApiVersionConventionBuilder builder, int majorVersion )
        {
            Arg.NotNull( builder, nameof( builder ) );
            Contract.Ensures( Contract.Result<ActionApiVersionConventionBuilder>() != null );
            Arg.GreaterThanOrEqualTo( majorVersion, 0, nameof( majorVersion ) );

            builder.MapToApiVersion( new ApiVersion( majorVersion, 0 ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is mapped to the configured controller action.
        /// </summary>
        /// <param name="builder">The extended <see cref="ActionApiVersionConventionBuilder"/>.</param>
        /// <param name="majorVersion">The value for a major version only scheme.</param>
        /// <param name="status">The version status.</param>
        /// <returns>The original <see cref="ActionApiVersionConventionBuilder"/>.</returns>
        public static ActionApiVersionConventionBuilder MapToApiVersion( this ActionApiVersionConventionBuilder builder, int majorVersion, string status )
        {
            Arg.NotNull( builder, nameof( builder ) );
            Contract.Ensures( Contract.Result<ActionApiVersionConventionBuilder>() != null );
            Arg.GreaterThanOrEqualTo( majorVersion, 0, nameof( majorVersion ) );

            builder.MapToApiVersion( new ApiVersion( majorVersion, 0, status ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is mapped to the configured controller action.
        /// </summary>
        /// <param name="builder">The extended <see cref="ActionApiVersionConventionBuilder"/>.</param>
        /// <param name="majorVersion">The major version number.</param>
        /// <param name="minorVersion">The minor version number.</param>
        /// <returns>The original <see cref="ActionApiVersionConventionBuilder"/>.</returns>
        public static ActionApiVersionConventionBuilder MapToApiVersion( this ActionApiVersionConventionBuilder builder, int majorVersion, int minorVersion )
        {
            Arg.NotNull( builder, nameof( builder ) );
            Contract.Ensures( Contract.Result<ActionApiVersionConventionBuilder>() != null );
            Arg.GreaterThanOrEqualTo( majorVersion, 0, nameof( majorVersion ) );
            Arg.GreaterThanOrEqualTo( minorVersion, 0, nameof( minorVersion ) );

            builder.MapToApiVersion( new ApiVersion( majorVersion, minorVersion ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is mapped to the configured controller action.
        /// </summary>
        /// <param name="builder">The extended <see cref="ActionApiVersionConventionBuilder"/>.</param>
        /// <param name="majorVersion">The major version number.</param>
        /// <param name="minorVersion">The minor version number.</param>
        /// <param name="status">The version status.</param>
        /// <returns>The original <see cref="ActionApiVersionConventionBuilder"/>.</returns>
        public static ActionApiVersionConventionBuilder MapToApiVersion( this ActionApiVersionConventionBuilder builder, int majorVersion, int minorVersion, string status )
        {
            Arg.NotNull( builder, nameof( builder ) );
            Arg.NotNullOrEmpty( status, nameof( status ) );
            Contract.Ensures( Contract.Result<ActionApiVersionConventionBuilder>() != null );
            Arg.GreaterThanOrEqualTo( majorVersion, 0, nameof( majorVersion ) );
            Arg.GreaterThanOrEqualTo( minorVersion, 0, nameof( minorVersion ) );

            builder.MapToApiVersion( new ApiVersion( majorVersion, minorVersion, status ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is mapped to the configured controller action.
        /// </summary>
        /// <param name="builder">The extended <see cref="ActionApiVersionConventionBuilder"/>.</param>
        /// <param name="year">The version year.</param>
        /// <param name="month">The version month.</param>
        /// <param name="day">The version day.</param>
        /// <returns>The original <see cref="ActionApiVersionConventionBuilder"/>.</returns>
        public static ActionApiVersionConventionBuilder MapToApiVersion( this ActionApiVersionConventionBuilder builder, int year, int month, int day )
        {
            Arg.NotNull( builder, nameof( builder ) );
            Contract.Ensures( Contract.Result<ActionApiVersionConventionBuilder>() != null );
            Arg.InRange( year, 1, 9999, nameof( year ) );
            Arg.InRange( month, 1, 12, nameof( month ) );
            Arg.InRange( day, 1, 31, nameof( day ) );

            builder.MapToApiVersion( new ApiVersion( new DateTime( year, month, day ) ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is mapped to the configured controller action.
        /// </summary>
        /// <param name="builder">The extended <see cref="ActionApiVersionConventionBuilder"/>.</param>
        /// <param name="year">The version year.</param>
        /// <param name="month">The version month.</param>
        /// <param name="day">The version day.</param>
        /// <param name="status">The version status.</param>
        /// <returns>The original <see cref="ActionApiVersionConventionBuilder"/>.</returns>
        public static ActionApiVersionConventionBuilder MapToApiVersion( this ActionApiVersionConventionBuilder builder, int year, int month, int day, string status )
        {
            Arg.NotNull( builder, nameof( builder ) );
            Arg.NotNullOrEmpty( status, nameof( status ) );
            Contract.Ensures( Contract.Result<ActionApiVersionConventionBuilder>() != null );
            Arg.InRange( year, 1, 9999, nameof( year ) );
            Arg.InRange( month, 1, 12, nameof( month ) );
            Arg.InRange( day, 1, 31, nameof( day ) );

            builder.MapToApiVersion( new ApiVersion( new DateTime( year, month, day ), status ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is mapped to the configured controller action.
        /// </summary>
        /// <param name="builder">The extended <see cref="ActionApiVersionConventionBuilder"/>.</param>
        /// <param name="groupVersion">The group version.</param>
        /// <returns>The original <see cref="ActionApiVersionConventionBuilder"/>.</returns>
        public static ActionApiVersionConventionBuilder MapToApiVersion( this ActionApiVersionConventionBuilder builder, DateTime groupVersion )
        {
            Arg.NotNull( builder, nameof( builder ) );
            Contract.Ensures( Contract.Result<ActionApiVersionConventionBuilder>() != null );

            builder.MapToApiVersion( new ApiVersion( groupVersion ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is mapped to the configured controller action.
        /// </summary>
        /// <param name="builder">The extended <see cref="ActionApiVersionConventionBuilder"/>.</param>
        /// <param name="groupVersion">The group version.</param>
        /// <param name="status">The version status.</param>
        /// <returns>The original <see cref="ActionApiVersionConventionBuilder"/>.</returns>
        public static ActionApiVersionConventionBuilder MapToApiVersion( this ActionApiVersionConventionBuilder builder, DateTime groupVersion, string status )
        {
            Arg.NotNull( builder, nameof( builder ) );
            Arg.NotNullOrEmpty( status, nameof( status ) );
            Contract.Ensures( Contract.Result<ActionApiVersionConventionBuilder>() != null );

            builder.MapToApiVersion( new ApiVersion( groupVersion, status ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API versions are mapped to the configured controller action.
        /// </summary>
        /// <param name="builder">The extended <see cref="ActionApiVersionConventionBuilder"/>.</param>
        /// <param name="apiVersions">The <see cref="IEnumerable{T}">sequence</see> of <see cref="ApiVersion">API versions</see> supported by the controller.</param>
        /// <returns>The original <see cref="ActionApiVersionConventionBuilder"/>.</returns>
        public static ActionApiVersionConventionBuilder MapToApiVersions( this ActionApiVersionConventionBuilder builder, IEnumerable<ApiVersion> apiVersions )
        {
            Arg.NotNull( builder, nameof( builder ) );
            Arg.NotNull( apiVersions, nameof( apiVersions ) );
            Contract.Ensures( Contract.Result<ActionApiVersionConventionBuilder>() != null );

            foreach ( var apiVersion in apiVersions )
            {
                builder.MapToApiVersion( apiVersion );
            }

            return builder;
        }
    }
}