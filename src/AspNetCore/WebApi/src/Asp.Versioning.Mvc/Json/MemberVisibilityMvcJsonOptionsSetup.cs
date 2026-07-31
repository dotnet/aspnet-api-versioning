// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable CA1812

namespace Asp.Versioning.Json;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Reflection;
using System.Text.Json.Serialization.Metadata;

/// <summary>
/// Applies API version member visibility to the JSON options used by MVC.
/// </summary>
/// <remarks>MVC resolves its own JSON options, which are distinct from the options used by minimal APIs. Both are
/// configured so that a member is filtered the same way regardless of how the endpoint was defined.</remarks>
internal sealed class MemberVisibilityMvcJsonOptionsSetup(
    IAnnotation<MemberInfo, ApiVersionRange> annotation,
    IHttpContextAccessor httpContextAccessor ) : IPostConfigureOptions<JsonOptions>
{
    private readonly MemberVisibilityJsonModifier modifier = new( annotation, httpContextAccessor );

    public void PostConfigure( string? name, JsonOptions options )
    {
        var serializerOptions = options.JsonSerializerOptions;

        if ( serializerOptions.TypeInfoResolver is { } resolver )
        {
            serializerOptions.TypeInfoResolver = resolver.WithAddedModifier( modifier.Modify );
        }
    }
}