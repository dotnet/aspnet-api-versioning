// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Microsoft.AspNetCore.Builder;

using Asp.Versioning;
using Asp.Versioning.Builder;
using Microsoft.AspNetCore.Http;
using System.Collections;
using static Asp.Versioning.ApiVersionProviderOptions;

/// <summary>
/// Provides extension methods for <see cref="RouteHandlerBuilder"/>.
/// </summary>
[CLSCompliant( false )]
public static class RouteHandlerBuilderExtensions
{
    /// <summary>
    /// Indicates that the specified API version is mapped to the configured endpoint.
    /// </summary>
    /// <param name="builder">The extended <see cref="RouteHandlerBuilder">route handler builder</see>.</param>
    /// <param name="majorVersion">The major version number.</param>
    /// <param name="minorVersion">The optional minor version number.</param>
    /// <param name="status">The optional version status.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    public static RouteHandlerBuilder MapToApiVersion( this RouteHandlerBuilder builder, int majorVersion, int? minorVersion = default, string? status = default ) =>
        builder.MapToApiVersion( new ApiVersion( majorVersion, minorVersion, status ) );

    /// <summary>
    /// Indicates that the specified API version is mapped to the configured endpoint.
    /// </summary>
    /// <param name="builder">The extended <see cref="RouteHandlerBuilder">route handler builder</see>.</param>
    /// <param name="version">The version number.</param>
    /// <param name="status">The optional version status.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    public static RouteHandlerBuilder MapToApiVersion( this RouteHandlerBuilder builder, double version, string? status = default ) =>
        builder.MapToApiVersion( new ApiVersion( version, status ) );

    /// <summary>
    /// Indicates that the specified API version is mapped to the configured endpoint.
    /// </summary>
    /// <param name="builder">The extended <see cref="RouteHandlerBuilder">route handler builder</see>.</param>
    /// <param name="year">The version year.</param>
    /// <param name="month">The version month.</param>
    /// <param name="day">The version day.</param>
    /// <param name="status">The optional version status.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    public static RouteHandlerBuilder MapToApiVersion( this RouteHandlerBuilder builder, int year, int month, int day, string? status = default ) =>
        builder.MapToApiVersion( new ApiVersion( new DateOnly( year, month, day ), status ) );

    /// <summary>
    /// Indicates that the specified API version is mapped to the configured endpoint.
    /// </summary>
    /// <param name="builder">The extended <see cref="RouteHandlerBuilder">route handler builder</see>.</param>
    /// <param name="groupVersion">The group version.</param>
    /// <param name="status">The optional version status.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    public static RouteHandlerBuilder MapToApiVersion( this RouteHandlerBuilder builder, DateOnly groupVersion, string? status = default ) =>
        builder.MapToApiVersion( new ApiVersion( groupVersion, status ) );

    /// <summary>
    /// Maps the specified API version to the configured endpoint.
    /// </summary>
    /// <param name="builder">The extended <see cref="RouteHandlerBuilder">route handler builder</see>.</param>
    /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> to map to the endpoint.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    public static RouteHandlerBuilder MapToApiVersion( this RouteHandlerBuilder builder, ApiVersion apiVersion )
    {
        builder.Add( endpoint => endpoint.Metadata.Add( Convention.MapToApiVersion( apiVersion ) ) );
        return builder;
    }

    /// <summary>
    /// Indicates that the endpoint is API version-neutral.
    /// </summary>
    /// <param name="builder">The extended <see cref="RouteHandlerBuilder">route handler builder</see>.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    public static RouteHandlerBuilder IsApiVersionNeutral( this RouteHandlerBuilder builder )
    {
        builder.Add( endpoint => endpoint.Metadata.Add( new ApiVersionNeutralAttribute() ) );
        return builder;
    }

    /// <summary>
    /// Indicates that the specified API version is supported by the configured endpoint.
    /// </summary>
    /// <param name="builder">The extended <see cref="RouteHandlerBuilder">route handler builder</see>.</param>
    /// <param name="majorVersion">The major version number.</param>
    /// <param name="minorVersion">The optional minor version number.</param>
    /// <param name="status">The optional version status.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    public static RouteHandlerBuilder HasApiVersion( this RouteHandlerBuilder builder, int majorVersion, int? minorVersion = default, string? status = default ) =>
        builder.HasApiVersion( new ApiVersion( majorVersion, minorVersion, status ) );

    /// <summary>
    /// Indicates that the specified API version is supported by the configured endpoint.
    /// </summary>
    /// <param name="builder">The extended <see cref="RouteHandlerBuilder">route handler builder</see>.</param>
    /// <param name="version">The version number.</param>
    /// <param name="status">The optional version status.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    public static RouteHandlerBuilder HasApiVersion( this RouteHandlerBuilder builder, double version, string? status = default ) =>
        builder.HasApiVersion( new ApiVersion( version, status ) );

    /// <summary>
    /// Indicates that the specified API version is supported by the configured endpoint.
    /// </summary>
    /// <param name="builder">The extended <see cref="RouteHandlerBuilder">route handler builder</see>.</param>
    /// <param name="year">The version year.</param>
    /// <param name="month">The version month.</param>
    /// <param name="day">The version day.</param>
    /// <param name="status">The optional version status.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    public static RouteHandlerBuilder HasApiVersion( this RouteHandlerBuilder builder, int year, int month, int day, string? status = default ) =>
        builder.HasApiVersion( new ApiVersion( new DateOnly( year, month, day ), status ) );

    /// <summary>
    /// Indicates that the specified API version is supported by the configured endpoint.
    /// </summary>
    /// <param name="builder">The extended <see cref="RouteHandlerBuilder">route handler builder</see>.</param>
    /// <param name="groupVersion">The group version.</param>
    /// <param name="status">The optional version status.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    public static RouteHandlerBuilder HasApiVersion( this RouteHandlerBuilder builder, DateOnly groupVersion, string? status = default ) =>
        builder.HasApiVersion( new ApiVersion( groupVersion, status ) );

    /// <summary>
    /// Indicates that the specified API version is supported by the configured endpoint.
    /// </summary>
    /// <param name="builder">The extended <see cref="RouteHandlerBuilder">route handler builder</see>.</param>
    /// <param name="apiVersion">The supported <see cref="ApiVersion">API version</see> implemented by the endpoint.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    public static RouteHandlerBuilder HasApiVersion( this RouteHandlerBuilder builder, ApiVersion apiVersion )
    {
        builder.Add(
            endpoint =>
            {
                var metadata = endpoint.Metadata;
                metadata.Add( Convention.HasApiVersion( apiVersion ) );
                AdvertiseInApiVersionSet( metadata, apiVersion );
            } );

        return builder;
    }

    /// <summary>
    /// Indicates that the specified API version is deprecated by the configured endpoint.
    /// </summary>
    /// <param name="builder">The extended <see cref="RouteHandlerBuilder">route handler builder</see>.</param>
    /// <param name="majorVersion">The major version number.</param>
    /// <param name="minorVersion">The optional minor version number.</param>
    /// <param name="status">The optional version status.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    public static RouteHandlerBuilder HasDeprecatedApiVersion( this RouteHandlerBuilder builder, int majorVersion, int? minorVersion = default, string? status = default ) =>
        builder.HasDeprecatedApiVersion( new ApiVersion( majorVersion, minorVersion, status ) );

    /// <summary>
    /// Indicates that the specified API version is deprecated by the configured endpoint.
    /// </summary>
    /// <param name="builder">The extended <see cref="RouteHandlerBuilder">route handler builder</see>.</param>
    /// <param name="version">The version number.</param>
    /// <param name="status">The optional version status.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    public static RouteHandlerBuilder HasDeprecatedApiVersion( this RouteHandlerBuilder builder, double version, string? status = default ) =>
        builder.HasDeprecatedApiVersion( new ApiVersion( version, status ) );

    /// <summary>
    /// Indicates that the specified API version is deprecated by the configured endpoint.
    /// </summary>
    /// <param name="builder">The extended <see cref="RouteHandlerBuilder">route handler builder</see>.</param>
    /// <param name="year">The version year.</param>
    /// <param name="month">The version month.</param>
    /// <param name="day">The version day.</param>
    /// <param name="status">The optional version status.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    public static RouteHandlerBuilder HasDeprecatedApiVersion( this RouteHandlerBuilder builder, int year, int month, int day, string? status = default ) =>
        builder.HasDeprecatedApiVersion( new ApiVersion( new DateOnly( year, month, day ), status ) );

    /// <summary>
    /// Indicates that the specified API version is deprecated by the configured endpoint.
    /// </summary>
    /// <param name="builder">The extended <see cref="RouteHandlerBuilder">route handler builder</see>.</param>
    /// <param name="groupVersion">The group version.</param>
    /// <param name="status">The optional version status.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    public static RouteHandlerBuilder HasDeprecatedApiVersion( this RouteHandlerBuilder builder, DateOnly groupVersion, string? status = default ) =>
        builder.HasDeprecatedApiVersion( new ApiVersion( groupVersion, status ) );

    /// <summary>
    /// Indicates that the specified API version is deprecated by the configured endpoint.
    /// </summary>
    /// <param name="builder">The extended <see cref="RouteHandlerBuilder">route handler builder</see>.</param>
    /// <param name="apiVersion">The deprecated <see cref="ApiVersion">API version</see> implemented by the endpoint.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    public static RouteHandlerBuilder HasDeprecatedApiVersion( this RouteHandlerBuilder builder, ApiVersion apiVersion )
    {
        builder.Add(
            endpoint =>
            {
                var metadata = endpoint.Metadata;
                metadata.Add( Convention.HasDeprecatedApiVersion( apiVersion ) );
                AdvertiseDeprecatedInApiVersionSet( metadata, apiVersion );
            } );

        return builder;
    }

    /// <summary>
    /// Indicates that the specified API version is advertised by the configured endpoint.
    /// </summary>
    /// <param name="builder">The extended <see cref="RouteHandlerBuilder">route handler builder</see>.</param>
    /// <param name="majorVersion">The major version number.</param>
    /// <param name="minorVersion">The optional minor version number.</param>
    /// <param name="status">The optional version status.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    public static RouteHandlerBuilder AdvertisesApiVersion( this RouteHandlerBuilder builder, int majorVersion, int? minorVersion = default, string? status = default ) =>
        builder.AdvertisesApiVersion( new ApiVersion( majorVersion, minorVersion, status ) );

    /// <summary>
    /// Indicates that the specified API version is advertised by the configured endpoint.
    /// </summary>
    /// <param name="builder">The extended <see cref="RouteHandlerBuilder">route handler builder</see>.</param>
    /// <param name="version">The version number.</param>
    /// <param name="status">The optional version status.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    public static RouteHandlerBuilder AdvertisesApiVersion( this RouteHandlerBuilder builder, double version, string? status = default ) =>
        builder.AdvertisesApiVersion( new ApiVersion( version, status ) );

    /// <summary>
    /// Indicates that the specified API version is advertised by the configured endpoint.
    /// </summary>
    /// <param name="builder">The extended <see cref="RouteHandlerBuilder">route handler builder</see>.</param>
    /// <param name="year">The version year.</param>
    /// <param name="month">The version month.</param>
    /// <param name="day">The version day.</param>
    /// <param name="status">The optional version status.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    public static RouteHandlerBuilder AdvertisesApiVersion( this RouteHandlerBuilder builder, int year, int month, int day, string? status = default ) =>
        builder.AdvertisesApiVersion( new ApiVersion( new DateOnly( year, month, day ), status ) );

    /// <summary>
    /// Indicates that the specified API version is advertised by the configured endpoint.
    /// </summary>
    /// <param name="builder">The extended <see cref="RouteHandlerBuilder">route handler builder</see>.</param>
    /// <param name="groupVersion">The group version.</param>
    /// <param name="status">The optional version status.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    public static RouteHandlerBuilder AdvertisesApiVersion( this RouteHandlerBuilder builder, DateOnly groupVersion, string? status = default ) =>
        builder.AdvertisesApiVersion( new ApiVersion( groupVersion, status ) );

    /// <summary>
    /// Indicates that the specified API version is advertised by the configured endpoint.
    /// </summary>
    /// <param name="builder">The extended <see cref="RouteHandlerBuilder">route handler builder</see>.</param>
    /// <param name="apiVersion">The advertised <see cref="ApiVersion">API version</see> not directly implemented by the endpoint.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    public static RouteHandlerBuilder AdvertisesApiVersion( this RouteHandlerBuilder builder, ApiVersion apiVersion )
    {
        builder.Add(
            endpoint =>
            {
                var metadata = endpoint.Metadata;
                metadata.Add( Convention.AdvertisesApiVersion( apiVersion ) );
                AdvertiseInApiVersionSet( metadata, apiVersion );
            } );

        return builder;
    }

    /// <summary>
    /// Indicates that the specified API version is advertised and deprecated by the configured endpoint.
    /// </summary>
    /// <param name="builder">The extended <see cref="RouteHandlerBuilder">route handler builder</see>.</param>
    /// <param name="majorVersion">The major version number.</param>
    /// <param name="minorVersion">The optional minor version number.</param>
    /// <param name="status">The optional version status.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    public static RouteHandlerBuilder AdvertisesDeprecatedApiVersion( this RouteHandlerBuilder builder, int majorVersion, int? minorVersion = default, string? status = default ) =>
        builder.AdvertisesDeprecatedApiVersion( new ApiVersion( majorVersion, minorVersion, status ) );

    /// <summary>
    /// Indicates that the specified API version is advertised and deprecated by the configured endpoint.
    /// </summary>
    /// <param name="builder">The extended <see cref="RouteHandlerBuilder">route handler builder</see>.</param>
    /// <param name="version">The version number.</param>
    /// <param name="status">The optional version status.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    public static RouteHandlerBuilder AdvertisesDeprecatedApiVersion( this RouteHandlerBuilder builder, double version, string? status = default ) =>
        builder.AdvertisesDeprecatedApiVersion( new ApiVersion( version, status ) );

    /// <summary>
    /// Indicates that the specified API version is advertised and deprecated by the configured endpoint.
    /// </summary>
    /// <param name="builder">The extended <see cref="RouteHandlerBuilder">route handler builder</see>.</param>
    /// <param name="year">The version year.</param>
    /// <param name="month">The version month.</param>
    /// <param name="day">The version day.</param>
    /// <param name="status">The version status.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    public static RouteHandlerBuilder AdvertisesDeprecatedApiVersion( this RouteHandlerBuilder builder, int year, int month, int day, string? status = default ) =>
        builder.AdvertisesDeprecatedApiVersion( new ApiVersion( new DateOnly( year, month, day ), status ) );

    /// <summary>
    /// Indicates that the specified API version is advertised and deprecated by the configured endpoint.
    /// </summary>
    /// <param name="builder">The extended <see cref="RouteHandlerBuilder">route handler builder</see>.</param>
    /// <param name="groupVersion">The group version.</param>
    /// <param name="status">The optional version status.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    public static RouteHandlerBuilder AdvertisesDeprecatedApiVersion( this RouteHandlerBuilder builder, DateOnly groupVersion, string? status = default ) =>
        builder.AdvertisesDeprecatedApiVersion( new ApiVersion( groupVersion, status ) );

    /// <summary>
    /// Indicates that the specified API version is advertised and deprecated by the configured endpoint.
    /// </summary>
    /// <param name="builder">The extended <see cref="RouteHandlerBuilder">route handler builder</see>.</param>
    /// <param name="apiVersion">The advertised, but deprecated <see cref="ApiVersion">API version</see> not directly implemented by the endpoint.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    public static RouteHandlerBuilder AdvertisesDeprecatedApiVersion( this RouteHandlerBuilder builder, ApiVersion apiVersion )
    {
        builder.Add(
            endpoint =>
            {
                var metadata = endpoint.Metadata;
                metadata.Add( Convention.AdvertisesDeprecatedApiVersion( apiVersion ) );
                AdvertiseDeprecatedInApiVersionSet( metadata, apiVersion );
            } );

        return builder;
    }

    /// <summary>
    /// Indicates that the endpoint will report its API versions.
    /// </summary>
    /// <param name="builder">The extended <see cref="RouteHandlerBuilder">route handler builder</see>.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    public static RouteHandlerBuilder ReportApiVersions( this RouteHandlerBuilder builder )
    {
        builder.Add( endpoint => endpoint.Metadata.Add( Convention.ReportApiVersions ) );
        return builder;
    }

    private static void AdvertiseInApiVersionSet( IList<object> metadata, ApiVersion apiVersion )
    {
        for ( var i = metadata.Count - 1; i >= 0; i-- )
        {
            if ( metadata[i] is ApiVersionSet versionSet )
            {
                versionSet.AdvertisesApiVersion( apiVersion );
                break;
            }
        }
    }

    private static void AdvertiseDeprecatedInApiVersionSet( IList<object> metadata, ApiVersion apiVersion )
    {
        for ( var i = metadata.Count - 1; i >= 0; i-- )
        {
            if ( metadata[i] is ApiVersionSet versionSet )
            {
                versionSet.AdvertisesDeprecatedApiVersion( apiVersion );
                break;
            }
        }
    }

    private sealed class SingleItemReadOnlyList : IReadOnlyList<ApiVersion>
    {
        private readonly ApiVersion item;

        internal SingleItemReadOnlyList( ApiVersion item ) => this.item = item;

        public ApiVersion this[int index] => index == 0 ? item : throw new IndexOutOfRangeException();

        public int Count => 1;

        public IEnumerator<ApiVersion> GetEnumerator()
        {
            yield return item;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    private sealed class ReportApiVersionsConvention : IReportApiVersions
    {
        public ApiVersionMapping Mapping => ApiVersionMapping.None;

        public void Report( HttpResponse response, ApiVersionModel apiVersionModel ) { }
    }

    private sealed class Convention : IApiVersionProvider
    {
        private static ReportApiVersionsConvention? reportApiVersions;

        private Convention( ApiVersion version, ApiVersionProviderOptions options )
        {
            Versions = new SingleItemReadOnlyList( version );
            Options = options;
        }

        public ApiVersionProviderOptions Options { get; }

        public IReadOnlyList<ApiVersion> Versions { get; }

        internal static IReportApiVersions ReportApiVersions => reportApiVersions ??= new();

        internal static Convention HasApiVersion( ApiVersion version ) => new( version, None );

        internal static Convention HasDeprecatedApiVersion( ApiVersion version ) => new( version, Deprecated );

        internal static Convention MapToApiVersion( ApiVersion version ) => new( version, Mapped );

        internal static Convention AdvertisesApiVersion( ApiVersion version ) => new( version, Advertised );

        internal static Convention AdvertisesDeprecatedApiVersion( ApiVersion version ) => new( version, Advertised | Deprecated );
    }
}