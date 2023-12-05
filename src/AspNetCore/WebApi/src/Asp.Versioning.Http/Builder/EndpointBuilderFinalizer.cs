// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Builder;

using Asp.Versioning;
using Asp.Versioning.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Runtime.CompilerServices;
using static Asp.Versioning.ApiVersionParameterLocation;
using static Asp.Versioning.ApiVersionProviderOptions;

internal static class EndpointBuilderFinalizer
{
    internal static void FinalizeEndpoints( EndpointBuilder endpointBuilder )
    {
        var versionSet = GetApiVersionSet( endpointBuilder.Metadata );
        Finialize( endpointBuilder, versionSet );
    }

    internal static void FinalizeRoutes( EndpointBuilder endpointBuilder )
    {
        var versionSet = endpointBuilder.ApplicationServices.GetService<ApiVersionSet>();
        Finialize( endpointBuilder, versionSet );
    }

    private static void Finialize( EndpointBuilder endpointBuilder, ApiVersionSet? versionSet )
    {
        if ( versionSet is null )
        {
            // this could only happen if the ApiVersionSet was removed elsewhere from the metadata
            endpointBuilder.Metadata.Add( ApiVersionMetadata.Empty );
            return;
        }

        var services = endpointBuilder.ApplicationServices;
        var endpointMetadata = endpointBuilder.Metadata;
        var options = services.GetRequiredService<IOptions<ApiVersioningOptions>>().Value;
        var metadata = Build( endpointMetadata, versionSet, options );
        var reportApiVersions = ReportApiVersions( endpointMetadata ) ||
                                options.ReportApiVersions ||
                                versionSet.ReportApiVersions;

        endpointBuilder.Metadata.Add( metadata );

        var requestDelegate = default( RequestDelegate );

        if ( reportApiVersions )
        {
            requestDelegate = EnsureRequestDelegate( requestDelegate, endpointBuilder.RequestDelegate );

            var reporter = services.GetRequiredService<IReportApiVersions>();

            requestDelegate = new ReportApiVersionsDecorator( requestDelegate, reporter, metadata );
            endpointBuilder.RequestDelegate = requestDelegate;
        }

        var parameterSource = services.GetRequiredService<IApiVersionParameterSource>();

        if ( parameterSource.VersionsByMediaType() )
        {
            var parameterName = parameterSource.GetParameterName( MediaTypeParameter );

            if ( !string.IsNullOrEmpty( parameterName ) )
            {
                requestDelegate = EnsureRequestDelegate( requestDelegate, endpointBuilder.RequestDelegate );
                requestDelegate = new ContentTypeApiVersionDecorator( requestDelegate, parameterName );
                endpointBuilder.RequestDelegate = requestDelegate;
            }
        }
    }

    private static bool IsApiVersionNeutral( IList<object> metadata )
    {
        var versionNeutral = false;

        for ( var i = metadata.Count - 1; i >= 0; i-- )
        {
            if ( metadata[i] is IApiVersionNeutral )
            {
                versionNeutral = true;
                metadata.RemoveAt( i );
                break;
            }
        }

        if ( versionNeutral )
        {
            for ( var i = metadata.Count - 1; i >= 0; i-- )
            {
                switch ( metadata[i] )
                {
                    case IApiVersionProvider:
                    case IApiVersionNeutral:
                        metadata.RemoveAt( i );
                        break;
                }
            }
        }

        return versionNeutral;
    }

    private static bool ReportApiVersions( IList<object> metadata )
    {
        var result = false;

        for ( var i = metadata.Count - 1; i >= 0; i-- )
        {
            if ( metadata[i] is IReportApiVersions )
            {
                result = true;
                metadata.RemoveAt( i );
            }
        }

        return result;
    }

    private static ApiVersionSet? GetApiVersionSet( IList<object> metadata )
    {
        for ( var i = metadata.Count - 1; i >= 0; i-- )
        {
            if ( metadata[i] is ApiVersionSet versionSet )
            {
                metadata.RemoveAt( i );
                return versionSet;
            }
        }

        return default;
    }

    private static bool TryGetApiVersions( IList<object> metadata, out ApiVersionBuckets buckets )
    {
        if ( IsApiVersionNeutral( metadata ) )
        {
            buckets = default;
            return false;
        }

        var mapped = default( SortedSet<ApiVersion> );
        var supported = default( SortedSet<ApiVersion> );
        var deprecated = default( SortedSet<ApiVersion> );
        var advertised = default( SortedSet<ApiVersion> );
        var deprecatedAdvertised = default( SortedSet<ApiVersion> );

        for ( var i = metadata.Count - 1; i >= 0; i-- )
        {
            var item = metadata[i];

            if ( item is not IApiVersionProvider provider )
            {
                continue;
            }

            metadata.RemoveAt( i );

            var versions = provider.Versions;
            var target = provider.Options switch
            {
                None => supported ??= [],
                Mapped => mapped ??= [],
                Deprecated => deprecated ??= [],
                Advertised => advertised ??= [],
                Advertised | Deprecated => deprecatedAdvertised ??= [],
                _ => default,
            };

            if ( target is null )
            {
                continue;
            }

            for ( var j = 0; j < versions.Count; j++ )
            {
                target.Add( versions[j] );
            }
        }

        buckets = new(
            mapped?.ToArray() ?? [],
            supported?.ToArray() ?? [],
            deprecated?.ToArray() ?? [],
            advertised?.ToArray() ?? [],
            deprecatedAdvertised?.ToArray() ?? [] );

        return true;
    }

    private static ApiVersionMetadata Build( IList<object> metadata, ApiVersionSet versionSet, ApiVersioningOptions options )
    {
        var name = versionSet.Name;
        ApiVersionModel? apiModel;

        if ( !TryGetApiVersions( metadata, out var buckets ) ||
            ( apiModel = versionSet.Build( options ) ).IsApiVersionNeutral )
        {
            if ( string.IsNullOrEmpty( name ) )
            {
                return ApiVersionMetadata.Neutral;
            }

            return new( ApiVersionModel.Neutral, ApiVersionModel.Neutral, name );
        }

        ApiVersionModel endpointModel;
        ApiVersion[] emptyVersions;
        var inheritedSupported = apiModel.SupportedApiVersions;
        var inheritedDeprecated = apiModel.DeprecatedApiVersions;

        if ( buckets.AreEmpty )
        {
            var noInheritedApiVersions = inheritedSupported.Count == 0 &&
                                         inheritedDeprecated.Count == 0;

            if ( noInheritedApiVersions )
            {
                endpointModel = ApiVersionModel.Empty;
            }
            else
            {
                emptyVersions = [];
                endpointModel = new(
                    declaredVersions: emptyVersions,
                    inheritedSupported,
                    inheritedDeprecated,
                    emptyVersions,
                    emptyVersions );
            }
        }
        else
        {
            var (mapped, supported, deprecated, advertised, advertisedDeprecated) = buckets;

            if ( mapped.Count == 0 )
            {
                endpointModel = new(
                    declaredVersions: supported.Union( deprecated ),
                    supported.Union( inheritedSupported ),
                    deprecated.Union( inheritedDeprecated ),
                    advertised,
                    advertisedDeprecated );
            }
            else
            {
                emptyVersions = [];
                endpointModel = new(
                    declaredVersions: mapped,
                    supportedVersions: inheritedSupported,
                    deprecatedVersions: inheritedDeprecated,
                    advertisedVersions: emptyVersions,
                    deprecatedAdvertisedVersions: emptyVersions );
            }
        }

        return new( apiModel, endpointModel, name );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static RequestDelegate EnsureRequestDelegate( RequestDelegate? current, RequestDelegate? original ) =>
        ( current ?? original ) ??
        throw new InvalidOperationException(
            string.Format(
                CultureInfo.CurrentCulture,
                Format.UnsetRequestDelegate,
                nameof( RequestDelegate ),
                nameof( RouteEndpoint ) ) );

    private record struct ApiVersionBuckets(
        IReadOnlyList<ApiVersion> Mapped,
        IReadOnlyList<ApiVersion> Supported,
        IReadOnlyList<ApiVersion> Deprecated,
        IReadOnlyList<ApiVersion> Advertised,
        IReadOnlyList<ApiVersion> AdvertisedDeprecated )
    {
        internal readonly bool AreEmpty = Mapped.Count == 0
                                          && Supported.Count == 0
                                          && Deprecated.Count == 0
                                          && Advertised.Count == 0
                                          && AdvertisedDeprecated.Count == 0;
    }
}