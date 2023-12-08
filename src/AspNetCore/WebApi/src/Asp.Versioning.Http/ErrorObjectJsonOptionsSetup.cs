// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable CA1812 // Avoid uninstantiated internal classes

namespace Asp.Versioning;

using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;

/// <summary>
/// Adds the ErrorObjectJsonContext to the current JsonSerializerOptions.
///
/// This allows for consistent serialization behavior for ErrorObject regardless if the
/// default reflection-based serializer is used and makes it trim/NativeAOT compatible.
/// </summary>
internal sealed class ErrorObjectJsonOptionsSetup : IConfigureOptions<JsonOptions>
{
    // Always insert the ErrorObjectJsonContext to the beginning of the chain at the time this Configure
    // is invoked. This JsonTypeInfoResolver will be before the default reflection-based resolver, and
    // before any other resolvers currently added. If apps need to customize serialization, they can
    // prepend a custom ErrorObject resolver to the chain in an IConfigureOptions<JsonOptions> registered.
    public void Configure( JsonOptions options ) =>
        options.SerializerOptions.TypeInfoResolverChain.Insert( 0, ErrorObjectWriter.ErrorObjectJsonContext.Default );
}