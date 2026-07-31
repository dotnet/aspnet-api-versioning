// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable CA1812

namespace Asp.Versioning.Json;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using System.Reflection;
using System.Text.Json.Serialization.Metadata;

/// <summary>
/// Applies API version member visibility to the JSON options used by minimal APIs.
/// </summary>
/// <remarks>The modifier is added after all other configuration has run so that it observes every resolver the
/// application registered, including any added by the application itself.</remarks>
internal sealed class MemberVisibilityJsonOptionsSetup(
    IAnnotation<MemberInfo, ApiVersionRange> annotation,
    IHttpContextAccessor httpContextAccessor ) : IPostConfigureOptions<JsonOptions>
{
    private readonly MemberVisibilityJsonModifier modifier = new( annotation, httpContextAccessor );

    public void PostConfigure( string? name, JsonOptions options )
    {
        var serializerOptions = options.SerializerOptions;

        if ( serializerOptions.TypeInfoResolver is { } resolver )
        {
            serializerOptions.TypeInfoResolver = resolver.WithAddedModifier( modifier.Modify );
        }
    }
}