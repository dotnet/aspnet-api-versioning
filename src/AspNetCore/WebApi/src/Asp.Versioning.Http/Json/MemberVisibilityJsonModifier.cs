// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Json;

using Microsoft.AspNetCore.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using static System.Globalization.CultureInfo;

/// <summary>
/// Applies API version member visibility to a JSON contract.
/// </summary>
/// <remarks>
/// <para>
/// A contract is resolved once per type, but the API version is only known per request. The range a member is
/// visible in is therefore resolved here, while the comparison against the requested API version is deferred to
/// the point the member is read or written.
/// </para>
/// <para>
/// A member that is visible in every API version is left untouched, so a contract with no filtered members costs
/// nothing beyond the one-time resolution performed here.
/// </para>
/// </remarks>
internal sealed class MemberVisibilityJsonModifier(
    IAnnotation<MemberInfo, ApiVersionRange> annotation,
    IHttpContextAccessor httpContextAccessor )
{
    private static readonly CompositeFormat UnmappedMember = CompositeFormat.Parse( SR.UnmappedMember );

    internal void Modify( JsonTypeInfo typeInfo )
    {
        if ( typeInfo.Kind != JsonTypeInfoKind.Object )
        {
            return;
        }

        var properties = typeInfo.Properties;

        for ( var i = 0; i < properties.Count; i++ )
        {
            var property = properties[i];

            if ( property.AttributeProvider is not MemberInfo member )
            {
                continue;
            }

            // a member that is not annotated needs no per-request evaluation at all and is left untouched
            if ( !annotation.TryGet( member, out var apiVersions ) )
            {
                continue;
            }

            Hide( typeInfo, property, apiVersions );
        }
    }

    private void Hide( JsonTypeInfo typeInfo, JsonPropertyInfo property, ApiVersionRange apiVersions )
    {
        var shouldSerialize = property.ShouldSerialize;

        property.ShouldSerialize = ( obj, value ) =>
            IsVisible( apiVersions ) && ( shouldSerialize is null || shouldSerialize( obj, value ) );

        if ( property.Set is not { } set )
        {
            return;
        }

        var name = property.Name;
        var type = typeInfo.Type;

        // a member that is not visible does not exist as far as the client is concerned, so supplying it is
        // reported the same way as supplying a member that was never defined
        property.Set = ( obj, value ) =>
        {
            if ( !IsVisible( apiVersions ) )
            {
                throw new JsonException( string.Format( CurrentCulture, UnmappedMember, name, type.Name ) );
            }

            set( obj, value );
        };
    }

    // a request that did not resolve an API version is passed through untouched. there is no version to compare
    // a member against, so no member can be shown to be out of range. the feature is read rather than the
    // HttpContext.ApiVersioningFeature extension so that a request which never went through API versioning does
    // not have a feature created for it here
    private bool IsVisible( ApiVersionRange apiVersions ) =>
        httpContextAccessor.HttpContext?.Features.Get<IApiVersioningFeature>()
            is not { RequestedApiVersion: { } apiVersion }
        || apiVersions.Contains( apiVersion );
}