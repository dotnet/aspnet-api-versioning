// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0130

namespace Microsoft.AspNetCore.Builder;

using Asp.Versioning;
using Asp.Versioning.Builder;
using Microsoft.AspNetCore.Http;
using System.Collections;
using System.Globalization;
using static Asp.Versioning.ApiVersionProviderOptions;

/// <summary>
/// Provides extension methods for <see cref="IEndpointConventionBuilder"/> and <see cref="IVersionedEndpointRouteBuilder"/>.
/// </summary>
[CLSCompliant( false )]
public static class IEndpointConventionBuilderExtensions
{
    /// <typeparam name="TBuilder">The type of builder.</typeparam>
    /// <returns>The original <paramref name="builder"/>.</returns>
    extension<TBuilder>( TBuilder builder ) where TBuilder : notnull, IEndpointConventionBuilder
    {
        /// <summary>
        /// Applies the specified API version set to the endpoint.
        /// </summary>
        /// <param name="apiVersionSet">The <see cref="ApiVersionSet">API version set</see> the endpoint will use.</param>
        public TBuilder WithApiVersionSet( ApiVersionSet apiVersionSet )
        {
            ArgumentNullException.ThrowIfNull( apiVersionSet );

            builder.Add( endpoint => AddMetadata( endpoint, apiVersionSet ) );
            builder.Finally( EndpointBuilderFinalizer.FinalizeEndpoints );

            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is mapped to the configured endpoint.
        /// </summary>
        /// <param name="majorVersion">The major version number.</param>
        /// <param name="minorVersion">The optional minor version number.</param>
        /// <param name="status">The optional version status.</param>
        public TBuilder MapToApiVersion( int majorVersion, int? minorVersion = default, string? status = default ) =>
            builder.MapToApiVersion( new ApiVersion( majorVersion, minorVersion, status ) );

        /// <summary>
        /// Indicates that the specified API version is mapped to the configured endpoint.
        /// </summary>
        /// <param name="version">The version number.</param>
        /// <param name="status">The optional version status.</param>
        public TBuilder MapToApiVersion( double version, string? status = default ) =>
            builder.MapToApiVersion( new ApiVersion( version, status ) );

        /// <summary>
        /// Indicates that the specified API version is mapped to the configured endpoint.
        /// </summary>
        /// <param name="year">The version year.</param>
        /// <param name="month">The version month.</param>
        /// <param name="day">The version day.</param>
        /// <param name="status">The optional version status.</param>
        public TBuilder MapToApiVersion( int year, int month, int day, string? status = default ) =>
            builder.MapToApiVersion( new ApiVersion( new DateOnly( year, month, day ), status ) );

        /// <summary>
        /// Indicates that the specified API version is mapped to the configured endpoint.
        /// </summary>
        /// <param name="groupVersion">The group version.</param>
        /// <param name="status">The optional version status.</param>
        public TBuilder MapToApiVersion( DateOnly groupVersion, string? status = default ) =>
            builder.MapToApiVersion( new ApiVersion( groupVersion, status ) );

        /// <summary>
        /// Maps the specified API version to the configured endpoint.
        /// </summary>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> to map to the endpoint.</param>
        public TBuilder MapToApiVersion( ApiVersion apiVersion )
        {
            builder.Add( endpoint => AddMetadata( endpoint, Convention.MapToApiVersion( apiVersion ) ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the endpoint is API version-neutral.
        /// </summary>
        public TBuilder IsApiVersionNeutral()
        {
            builder.Add( endpoint => AddMetadata( endpoint, new ApiVersionNeutralAttribute() ) );
            return builder;
        }

        /// <summary>
        /// Indicates that the specified API version is supported by the configured endpoint.
        /// </summary>
        /// <param name="majorVersion">The major version number.</param>
        /// <param name="minorVersion">The optional minor version number.</param>
        /// <param name="status">The optional version status.</param>
        public TBuilder HasApiVersion( int majorVersion, int? minorVersion = default, string? status = default ) =>
            builder.HasApiVersion( new ApiVersion( majorVersion, minorVersion, status ) );

        /// <summary>
        /// Indicates that the specified API version is supported by the configured endpoint.
        /// </summary>
        /// <param name="version">The version number.</param>
        /// <param name="status">The optional version status.</param>
        public TBuilder HasApiVersion( double version, string? status = default ) =>
            builder.HasApiVersion( new ApiVersion( version, status ) );

        /// <summary>
        /// Indicates that the specified API version is supported by the configured endpoint.
        /// </summary>
        /// <param name="year">The version year.</param>
        /// <param name="month">The version month.</param>
        /// <param name="day">The version day.</param>
        /// <param name="status">The optional version status.</param>
        public TBuilder HasApiVersion( int year, int month, int day, string? status = default ) =>
            builder.HasApiVersion( new ApiVersion( new DateOnly( year, month, day ), status ) );

        /// <summary>
        /// Indicates that the specified API version is supported by the configured endpoint.
        /// </summary>
        /// <param name="groupVersion">The group version.</param>
        /// <param name="status">The optional version status.</param>
        public TBuilder HasApiVersion( DateOnly groupVersion, string? status = default ) =>
            builder.HasApiVersion( new ApiVersion( groupVersion, status ) );

        /// <summary>
        /// Indicates that the specified API version is supported by the configured endpoint.
        /// </summary>
        /// <param name="apiVersion">The supported <see cref="ApiVersion">API version</see> implemented by the endpoint.</param>
        public TBuilder HasApiVersion( ApiVersion apiVersion )
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
        /// <param name="majorVersion">The major version number.</param>
        /// <param name="minorVersion">The optional minor version number.</param>
        /// <param name="status">The optional version status.</param>
        public TBuilder HasDeprecatedApiVersion( int majorVersion, int? minorVersion = default, string? status = default ) =>
            builder.HasDeprecatedApiVersion( new ApiVersion( majorVersion, minorVersion, status ) );

        /// <summary>
        /// Indicates that the specified API version is deprecated by the configured endpoint.
        /// </summary>
        /// <param name="version">The version number.</param>
        /// <param name="status">The optional version status.</param>
        public TBuilder HasDeprecatedApiVersion( double version, string? status = default ) =>
            builder.HasDeprecatedApiVersion( new ApiVersion( version, status ) );

        /// <summary>
        /// Indicates that the specified API version is deprecated by the configured endpoint.
        /// </summary>
        /// <param name="year">The version year.</param>
        /// <param name="month">The version month.</param>
        /// <param name="day">The version day.</param>
        /// <param name="status">The optional version status.</param>
        public TBuilder HasDeprecatedApiVersion( int year, int month, int day, string? status = default ) =>
            builder.HasDeprecatedApiVersion( new ApiVersion( new DateOnly( year, month, day ), status ) );

        /// <summary>
        /// Indicates that the specified API version is deprecated by the configured endpoint.
        /// </summary>
        /// <param name="groupVersion">The group version.</param>
        /// <param name="status">The optional version status.</param>
        public TBuilder HasDeprecatedApiVersion( DateOnly groupVersion, string? status = default ) =>
            builder.HasDeprecatedApiVersion( new ApiVersion( groupVersion, status ) );

        /// <summary>
        /// Indicates that the specified API version is deprecated by the configured endpoint.
        /// </summary>
        /// <param name="apiVersion">The deprecated <see cref="ApiVersion">API version</see> implemented by the endpoint.</param>
        public TBuilder HasDeprecatedApiVersion( ApiVersion apiVersion )
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
        /// <param name="majorVersion">The major version number.</param>
        /// <param name="minorVersion">The optional minor version number.</param>
        /// <param name="status">The optional version status.</param>
        public TBuilder AdvertisesApiVersion( int majorVersion, int? minorVersion = default, string? status = default ) =>
            builder.AdvertisesApiVersion( new ApiVersion( majorVersion, minorVersion, status ) );

        /// <summary>
        /// Indicates that the specified API version is advertised by the configured endpoint.
        /// </summary>
        /// <param name="version">The version number.</param>
        /// <param name="status">The optional version status.</param>
        public TBuilder AdvertisesApiVersion( double version, string? status = default )
            => builder.AdvertisesApiVersion( new ApiVersion( version, status ) );

        /// <summary>
        /// Indicates that the specified API version is advertised by the configured endpoint.
        /// </summary>
        /// <param name="year">The version year.</param>
        /// <param name="month">The version month.</param>
        /// <param name="day">The version day.</param>
        /// <param name="status">The optional version status.</param>
        public TBuilder AdvertisesApiVersion( int year, int month, int day, string? status = default ) =>
            builder.AdvertisesApiVersion( new ApiVersion( new DateOnly( year, month, day ), status ) );

        /// <summary>
        /// Indicates that the specified API version is advertised by the configured endpoint.
        /// </summary>
        /// <param name="groupVersion">The group version.</param>
        /// <param name="status">The optional version status.</param>
        public TBuilder AdvertisesApiVersion( DateOnly groupVersion, string? status = default ) =>
            builder.AdvertisesApiVersion( new ApiVersion( groupVersion, status ) );

        /// <summary>
        /// Indicates that the specified API version is advertised by the configured endpoint.
        /// </summary>
        /// <param name="apiVersion">The advertised <see cref="ApiVersion">API version</see> not directly implemented by the endpoint.</param>
        public TBuilder AdvertisesApiVersion( ApiVersion apiVersion )
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
        /// <param name="majorVersion">The major version number.</param>
        /// <param name="minorVersion">The optional minor version number.</param>
        /// <param name="status">The optional version status.</param>
        public TBuilder AdvertisesDeprecatedApiVersion( int majorVersion, int? minorVersion = default, string? status = default ) =>
            builder.AdvertisesDeprecatedApiVersion( new ApiVersion( majorVersion, minorVersion, status ) );

        /// <summary>
        /// Indicates that the specified API version is advertised and deprecated by the configured endpoint.
        /// </summary>
        /// <param name="version">The version number.</param>
        /// <param name="status">The optional version status.</param>
        public TBuilder AdvertisesDeprecatedApiVersion( double version, string? status = default ) =>
            builder.AdvertisesDeprecatedApiVersion( new ApiVersion( version, status ) );

        /// <summary>
        /// Indicates that the specified API version is advertised and deprecated by the configured endpoint.
        /// </summary>
        /// <param name="year">The version year.</param>
        /// <param name="month">The version month.</param>
        /// <param name="day">The version day.</param>
        /// <param name="status">The version status.</param>
        public TBuilder AdvertisesDeprecatedApiVersion( int year, int month, int day, string? status = default ) =>
            builder.AdvertisesDeprecatedApiVersion( new ApiVersion( new DateOnly( year, month, day ), status ) );

        /// <summary>
        /// Indicates that the specified API version is advertised and deprecated by the configured endpoint.
        /// </summary>
        /// <param name="groupVersion">The group version.</param>
        /// <param name="status">The optional version status.</param>
        public TBuilder AdvertisesDeprecatedApiVersion( DateOnly groupVersion, string? status = default ) =>
            builder.AdvertisesDeprecatedApiVersion( new ApiVersion( groupVersion, status ) );

        /// <summary>
        /// Indicates that the specified API version is advertised and deprecated by the configured endpoint.
        /// </summary>
        /// <param name="apiVersion">The advertised, but deprecated <see cref="ApiVersion">API version</see> not
        /// directly implemented by the endpoint.</param>
        public TBuilder AdvertisesDeprecatedApiVersion( ApiVersion apiVersion )
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
        public TBuilder ReportApiVersions()
        {
            builder.Add( endpoint => AddMetadata( endpoint, Convention.ReportApiVersions ) );
            return builder;
        }
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