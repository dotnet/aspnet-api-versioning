// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Microsoft.AspNetCore.Builder;

using Asp.Versioning;
using Asp.Versioning.Builder;
using Microsoft.AspNetCore.Http;
using System.Collections;
using System.Globalization;
using System.Runtime.Serialization;
using static Asp.Versioning.ApiVersionProviderOptions;

/// <summary>
/// Provides extension methods for <see cref="IEndpointConventionBuilder"/> and <see cref="IVersionedEndpointRouteBuilder"/>.
/// </summary>
[CLSCompliant( false )]
public static class IEndpointConventionBuilderExtensions
{
    /// <summary>
    /// Applies the specified API version set to the endpoint.
    /// </summary>
    /// <typeparam name="TBuilder">The type of builder.</typeparam>
    /// <param name="builder">The extended builder.</param>
    /// <param name="apiVersionSet">The <see cref="ApiVersionSet">API version set</see> the endpoint will use.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    public static TBuilder WithApiVersionSet<TBuilder>(
        this TBuilder builder,
        ApiVersionSet apiVersionSet )
        where TBuilder : notnull, IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull( apiVersionSet );

        builder.Add( endpoint => AddMetadata( endpoint, apiVersionSet ) );
        builder.Finally( EndpointBuilderFinalizer.FinalizeEndpoints );

        return builder;
    }

    /// <summary>
    /// Indicates that the specified API version is mapped to the configured endpoint.
    /// </summary>
    /// <typeparam name="TBuilder">The extended type.</typeparam>
    /// <param name="builder">The extended endpoint convention builder.</param>
    /// <param name="majorVersion">The major version number.</param>
    /// <param name="minorVersion">The optional minor version number.</param>
    /// <param name="status">The optional version status.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    public static TBuilder MapToApiVersion<TBuilder>( this TBuilder builder, int majorVersion, int? minorVersion = default, string? status = default )
        where TBuilder : notnull, IEndpointConventionBuilder => builder.MapToApiVersion( new ApiVersion( majorVersion, minorVersion, status ) );

    /// <summary>
    /// Indicates that the specified API version is mapped to the configured endpoint.
    /// </summary>
    /// <typeparam name="TBuilder">The extended type.</typeparam>
    /// <param name="builder">The extended endpoint convention builder.</param>
    /// <param name="version">The version number.</param>
    /// <param name="status">The optional version status.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    public static TBuilder MapToApiVersion<TBuilder>( this TBuilder builder, double version, string? status = default )
        where TBuilder : notnull, IEndpointConventionBuilder => builder.MapToApiVersion( new ApiVersion( version, status ) );

    /// <summary>
    /// Indicates that the specified API version is mapped to the configured endpoint.
    /// </summary>
    /// <typeparam name="TBuilder">The extended type.</typeparam>
    /// <param name="builder">The extended endpoint convention builder.</param>
    /// <param name="year">The version year.</param>
    /// <param name="month">The version month.</param>
    /// <param name="day">The version day.</param>
    /// <param name="status">The optional version status.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    public static TBuilder MapToApiVersion<TBuilder>( this TBuilder builder, int year, int month, int day, string? status = default )
        where TBuilder : notnull, IEndpointConventionBuilder => builder.MapToApiVersion( new ApiVersion( new DateOnly( year, month, day ), status ) );

    /// <summary>
    /// Indicates that the specified API version is mapped to the configured endpoint.
    /// </summary>
    /// <typeparam name="TBuilder">The extended type.</typeparam>
    /// <param name="builder">The extended endpoint convention builder.</param>
    /// <param name="groupVersion">The group version.</param>
    /// <param name="status">The optional version status.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    public static TBuilder MapToApiVersion<TBuilder>( this TBuilder builder, DateOnly groupVersion, string? status = default )
        where TBuilder : notnull, IEndpointConventionBuilder => builder.MapToApiVersion( new ApiVersion( groupVersion, status ) );

    /// <summary>
    /// Maps the specified API version to the configured endpoint.
    /// </summary>
    /// <typeparam name="TBuilder">The extended type.</typeparam>
    /// <param name="builder">The extended endpoint convention builder.</param>
    /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> to map to the endpoint.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    public static TBuilder MapToApiVersion<TBuilder>( this TBuilder builder, ApiVersion apiVersion )
        where TBuilder : notnull, IEndpointConventionBuilder
    {
        builder.Add( endpoint => AddMetadata( endpoint, Convention.MapToApiVersion( apiVersion ) ) );
        return builder;
    }

    /// <summary>
    /// Indicates that the endpoint is API version-neutral.
    /// </summary>
    /// <typeparam name="TBuilder">The extended type.</typeparam>
    /// <param name="builder">The extended endpoint convention builder.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    public static TBuilder IsApiVersionNeutral<TBuilder>( this TBuilder builder )
        where TBuilder : notnull, IEndpointConventionBuilder
    {
        builder.Add( endpoint => AddMetadata( endpoint, new ApiVersionNeutralAttribute() ) );
        return builder;
    }

    /// <summary>
    /// Indicates that the specified API version is supported by the configured endpoint.
    /// </summary>
    /// <typeparam name="TBuilder">The extended type.</typeparam>
    /// <param name="builder">The extended endpoint convention builder.</param>
    /// <param name="majorVersion">The major version number.</param>
    /// <param name="minorVersion">The optional minor version number.</param>
    /// <param name="status">The optional version status.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    public static TBuilder HasApiVersion<TBuilder>( this TBuilder builder, int majorVersion, int? minorVersion = default, string? status = default )
        where TBuilder : notnull, IEndpointConventionBuilder => builder.HasApiVersion( new ApiVersion( majorVersion, minorVersion, status ) );

    /// <summary>
    /// Indicates that the specified API version is supported by the configured endpoint.
    /// </summary>
    /// <typeparam name="TBuilder">The extended type.</typeparam>
    /// <param name="builder">The extended endpoint convention builder.</param>
    /// <param name="version">The version number.</param>
    /// <param name="status">The optional version status.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    public static TBuilder HasApiVersion<TBuilder>( this TBuilder builder, double version, string? status = default )
        where TBuilder : notnull, IEndpointConventionBuilder => builder.HasApiVersion( new ApiVersion( version, status ) );

    /// <summary>
    /// Indicates that the specified API version is supported by the configured endpoint.
    /// </summary>
    /// <typeparam name="TBuilder">The extended type.</typeparam>
    /// <param name="builder">The extended endpoint convention builder.</param>
    /// <param name="year">The version year.</param>
    /// <param name="month">The version month.</param>
    /// <param name="day">The version day.</param>
    /// <param name="status">The optional version status.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    public static TBuilder HasApiVersion<TBuilder>( this TBuilder builder, int year, int month, int day, string? status = default )
        where TBuilder : notnull, IEndpointConventionBuilder => builder.HasApiVersion( new ApiVersion( new DateOnly( year, month, day ), status ) );

    /// <summary>
    /// Indicates that the specified API version is supported by the configured endpoint.
    /// </summary>
    /// <typeparam name="TBuilder">The extended type.</typeparam>
    /// <param name="builder">The extended endpoint convention builder.</param>
    /// <param name="groupVersion">The group version.</param>
    /// <param name="status">The optional version status.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    public static TBuilder HasApiVersion<TBuilder>( this TBuilder builder, DateOnly groupVersion, string? status = default )
        where TBuilder : notnull, IEndpointConventionBuilder => builder.HasApiVersion( new ApiVersion( groupVersion, status ) );

    /// <summary>
    /// Indicates that the specified API version is supported by the configured endpoint.
    /// </summary>
    /// <typeparam name="TBuilder">The extended type.</typeparam>
    /// <param name="builder">The extended endpoint convention builder.</param>
    /// <param name="apiVersion">The supported <see cref="ApiVersion">API version</see> implemented by the endpoint.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    public static TBuilder HasApiVersion<TBuilder>( this TBuilder builder, ApiVersion apiVersion )
        where TBuilder : notnull, IEndpointConventionBuilder
    {
        builder.Add(
            endpoint =>
            {
                AddMetadata( endpoint, Convention.HasApiVersion( apiVersion ) );
                AdvertiseInApiVersionSet( endpoint.Metadata, apiVersion );
            } );

        return builder;
    }

    /// <summary>
    /// Indicates that the specified API version is deprecated by the configured endpoint.
    /// </summary>
    /// <typeparam name="TBuilder">The extended type.</typeparam>
    /// <param name="builder">The extended endpoint convention builder.</param>
    /// <param name="majorVersion">The major version number.</param>
    /// <param name="minorVersion">The optional minor version number.</param>
    /// <param name="status">The optional version status.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    public static TBuilder HasDeprecatedApiVersion<TBuilder>( this TBuilder builder, int majorVersion, int? minorVersion = default, string? status = default )
        where TBuilder : notnull, IEndpointConventionBuilder => builder.HasDeprecatedApiVersion( new ApiVersion( majorVersion, minorVersion, status ) );

    /// <summary>
    /// Indicates that the specified API version is deprecated by the configured endpoint.
    /// </summary>
    /// <typeparam name="TBuilder">The extended type.</typeparam>
    /// <param name="builder">The extended endpoint convention builder.</param>
    /// <param name="version">The version number.</param>
    /// <param name="status">The optional version status.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    public static TBuilder HasDeprecatedApiVersion<TBuilder>( this TBuilder builder, double version, string? status = default )
        where TBuilder : notnull, IEndpointConventionBuilder => builder.HasDeprecatedApiVersion( new ApiVersion( version, status ) );

    /// <summary>
    /// Indicates that the specified API version is deprecated by the configured endpoint.
    /// </summary>
    /// <typeparam name="TBuilder">The extended type.</typeparam>
    /// <param name="builder">The extended endpoint convention builder.</param>
    /// <param name="year">The version year.</param>
    /// <param name="month">The version month.</param>
    /// <param name="day">The version day.</param>
    /// <param name="status">The optional version status.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    public static TBuilder HasDeprecatedApiVersion<TBuilder>( this TBuilder builder, int year, int month, int day, string? status = default )
        where TBuilder : notnull, IEndpointConventionBuilder => builder.HasDeprecatedApiVersion( new ApiVersion( new DateOnly( year, month, day ), status ) );

    /// <summary>
    /// Indicates that the specified API version is deprecated by the configured endpoint.
    /// </summary>
    /// <typeparam name="TBuilder">The extended type.</typeparam>
    /// <param name="builder">The extended endpoint convention builder.</param>
    /// <param name="groupVersion">The group version.</param>
    /// <param name="status">The optional version status.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    public static TBuilder HasDeprecatedApiVersion<TBuilder>( this TBuilder builder, DateOnly groupVersion, string? status = default )
        where TBuilder : notnull, IEndpointConventionBuilder => builder.HasDeprecatedApiVersion( new ApiVersion( groupVersion, status ) );

    /// <summary>
    /// Indicates that the specified API version is deprecated by the configured endpoint.
    /// </summary>
    /// <typeparam name="TBuilder">The extended type.</typeparam>
    /// <param name="builder">The extended endpoint convention builder.</param>
    /// <param name="apiVersion">The deprecated <see cref="ApiVersion">API version</see> implemented by the endpoint.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    public static TBuilder HasDeprecatedApiVersion<TBuilder>( this TBuilder builder, ApiVersion apiVersion )
        where TBuilder : notnull, IEndpointConventionBuilder
    {
        builder.Add(
            endpoint =>
            {
                AddMetadata( endpoint, Convention.HasDeprecatedApiVersion( apiVersion ) );
                AdvertiseDeprecatedInApiVersionSet( endpoint.Metadata, apiVersion );
            } );

        return builder;
    }

    /// <summary>
    /// Indicates that the specified API version is advertised by the configured endpoint.
    /// </summary>
    /// <typeparam name="TBuilder">The extended type.</typeparam>
    /// <param name="builder">The extended endpoint convention builder.</param>
    /// <param name="majorVersion">The major version number.</param>
    /// <param name="minorVersion">The optional minor version number.</param>
    /// <param name="status">The optional version status.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    public static TBuilder AdvertisesApiVersion<TBuilder>( this TBuilder builder, int majorVersion, int? minorVersion = default, string? status = default )
        where TBuilder : notnull, IEndpointConventionBuilder => builder.AdvertisesApiVersion( new ApiVersion( majorVersion, minorVersion, status ) );

    /// <summary>
    /// Indicates that the specified API version is advertised by the configured endpoint.
    /// </summary>
    /// <typeparam name="TBuilder">The extended type.</typeparam>
    /// <param name="builder">The extended endpoint convention builder.</param>
    /// <param name="version">The version number.</param>
    /// <param name="status">The optional version status.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    public static TBuilder AdvertisesApiVersion<TBuilder>( this TBuilder builder, double version, string? status = default )
        where TBuilder : notnull, IEndpointConventionBuilder => builder.AdvertisesApiVersion( new ApiVersion( version, status ) );

    /// <summary>
    /// Indicates that the specified API version is advertised by the configured endpoint.
    /// </summary>
    /// <typeparam name="TBuilder">The extended type.</typeparam>
    /// <param name="builder">The extended endpoint convention builder.</param>
    /// <param name="year">The version year.</param>
    /// <param name="month">The version month.</param>
    /// <param name="day">The version day.</param>
    /// <param name="status">The optional version status.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    public static TBuilder AdvertisesApiVersion<TBuilder>( this TBuilder builder, int year, int month, int day, string? status = default )
        where TBuilder : notnull, IEndpointConventionBuilder => builder.AdvertisesApiVersion( new ApiVersion( new DateOnly( year, month, day ), status ) );

    /// <summary>
    /// Indicates that the specified API version is advertised by the configured endpoint.
    /// </summary>
    /// <typeparam name="TBuilder">The extended type.</typeparam>
    /// <param name="builder">The extended endpoint convention builder.</param>
    /// <param name="groupVersion">The group version.</param>
    /// <param name="status">The optional version status.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    public static TBuilder AdvertisesApiVersion<TBuilder>( this TBuilder builder, DateOnly groupVersion, string? status = default )
        where TBuilder : notnull, IEndpointConventionBuilder => builder.AdvertisesApiVersion( new ApiVersion( groupVersion, status ) );

    /// <summary>
    /// Indicates that the specified API version is advertised by the configured endpoint.
    /// </summary>
    /// <typeparam name="TBuilder">The extended type.</typeparam>
    /// <param name="builder">The extended endpoint convention builder.</param>
    /// <param name="apiVersion">The advertised <see cref="ApiVersion">API version</see> not directly implemented by the endpoint.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    public static TBuilder AdvertisesApiVersion<TBuilder>( this TBuilder builder, ApiVersion apiVersion )
        where TBuilder : notnull, IEndpointConventionBuilder
    {
        builder.Add(
            endpoint =>
            {
                AddMetadata( endpoint, Convention.AdvertisesApiVersion( apiVersion ) );
                AdvertiseInApiVersionSet( endpoint.Metadata, apiVersion );
            } );

        return builder;
    }

    /// <summary>
    /// Indicates that the specified API version is advertised and deprecated by the configured endpoint.
    /// </summary>
    /// <typeparam name="TBuilder">The extended type.</typeparam>
    /// <param name="builder">The extended endpoint convention builder.</param>
    /// <param name="majorVersion">The major version number.</param>
    /// <param name="minorVersion">The optional minor version number.</param>
    /// <param name="status">The optional version status.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    public static TBuilder AdvertisesDeprecatedApiVersion<TBuilder>( this TBuilder builder, int majorVersion, int? minorVersion = default, string? status = default )
        where TBuilder : notnull, IEndpointConventionBuilder => builder.AdvertisesDeprecatedApiVersion( new ApiVersion( majorVersion, minorVersion, status ) );

    /// <summary>
    /// Indicates that the specified API version is advertised and deprecated by the configured endpoint.
    /// </summary>
    /// <typeparam name="TBuilder">The extended type.</typeparam>
    /// <param name="builder">The extended endpoint convention builder.</param>
    /// <param name="version">The version number.</param>
    /// <param name="status">The optional version status.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    public static TBuilder AdvertisesDeprecatedApiVersion<TBuilder>( this TBuilder builder, double version, string? status = default )
        where TBuilder : notnull, IEndpointConventionBuilder => builder.AdvertisesDeprecatedApiVersion( new ApiVersion( version, status ) );

    /// <summary>
    /// Indicates that the specified API version is advertised and deprecated by the configured endpoint.
    /// </summary>
    /// <typeparam name="TBuilder">The extended type.</typeparam>
    /// <param name="builder">The extended endpoint convention builder.</param>
    /// <param name="year">The version year.</param>
    /// <param name="month">The version month.</param>
    /// <param name="day">The version day.</param>
    /// <param name="status">The version status.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    public static TBuilder AdvertisesDeprecatedApiVersion<TBuilder>( this TBuilder builder, int year, int month, int day, string? status = default )
        where TBuilder : notnull, IEndpointConventionBuilder => builder.AdvertisesDeprecatedApiVersion( new ApiVersion( new DateOnly( year, month, day ), status ) );

    /// <summary>
    /// Indicates that the specified API version is advertised and deprecated by the configured endpoint.
    /// </summary>
    /// <typeparam name="TBuilder">The extended type.</typeparam>
    /// <param name="builder">The extended endpoint convention builder.</param>
    /// <param name="groupVersion">The group version.</param>
    /// <param name="status">The optional version status.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    public static TBuilder AdvertisesDeprecatedApiVersion<TBuilder>( this TBuilder builder, DateOnly groupVersion, string? status = default )
        where TBuilder : notnull, IEndpointConventionBuilder => builder.AdvertisesDeprecatedApiVersion( new ApiVersion( groupVersion, status ) );

    /// <summary>
    /// Indicates that the specified API version is advertised and deprecated by the configured endpoint.
    /// </summary>
    /// <typeparam name="TBuilder">The extended type.</typeparam>
    /// <param name="builder">The extended endpoint convention builder.</param>
    /// <param name="apiVersion">The advertised, but deprecated <see cref="ApiVersion">API version</see> not directly implemented by the endpoint.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    public static TBuilder AdvertisesDeprecatedApiVersion<TBuilder>( this TBuilder builder, ApiVersion apiVersion )
        where TBuilder : notnull, IEndpointConventionBuilder
    {
        builder.Add(
            endpoint =>
            {
                AddMetadata( endpoint, Convention.AdvertisesDeprecatedApiVersion( apiVersion ) );
                AdvertiseDeprecatedInApiVersionSet( endpoint.Metadata, apiVersion );
            } );

        return builder;
    }

    /// <summary>
    /// Indicates that the endpoint will report its API versions.
    /// </summary>
    /// <typeparam name="TBuilder">The extended type.</typeparam>
    /// <param name="builder">The extended endpoint convention builder.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    public static TBuilder ReportApiVersions<TBuilder>( this TBuilder builder )
        where TBuilder : notnull, IEndpointConventionBuilder
    {
        builder.Add( endpoint => AddMetadata( endpoint, Convention.ReportApiVersions ) );
        return builder;
    }

    private static void AddMetadata( EndpointBuilder builder, ApiVersionSet versionSet )
    {
        var metadata = builder.Metadata;
        var grouped = builder.ApplicationServices.GetService( typeof( ApiVersionSetBuilder ) ) is not null;

        if ( grouped )
        {
            throw new InvalidOperationException( SR.MultipleVersionSets );
        }

        for ( var i = 0; i < metadata.Count; i++ )
        {
            if ( metadata[i] is ApiVersionSet )
            {
                throw new InvalidOperationException( SR.MultipleVersionSets );
            }
        }

        metadata.Add( versionSet );

        if ( !string.IsNullOrEmpty( versionSet.Name ) )
        {
            metadata.Insert( 0, new TagsAttribute( versionSet.Name ) );
        }
    }

    private static void AddMetadata( EndpointBuilder builder, object item )
    {
        var metadata = builder.Metadata;
        var grouped = builder.ApplicationServices.GetService( typeof( ApiVersionSetBuilder ) ) is not null;

        metadata.Add( item );

        if ( grouped )
        {
            return;
        }

        for ( var i = metadata.Count - 1; i >= 0; i-- )
        {
            if ( metadata[i] is ApiVersionSet )
            {
                return;
            }
        }

        throw new InvalidOperationException(
            string.Format(
                CultureInfo.CurrentCulture,
                Format.NoVersionSet,
                builder.DisplayName,
                nameof( IEndpointRouteBuilderExtensions.NewVersionedApi ),
                nameof( IEndpointRouteBuilderExtensions.WithApiVersionSet ) ) );
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

#pragma warning disable IDE0079
#pragma warning disable CA2201 // Do not raise reserved exception types
        public ApiVersion this[int index] => index == 0 ? item : throw new IndexOutOfRangeException();
#pragma warning restore CA2201 // Do not raise reserved exception types
#pragma warning restore IDE0079

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