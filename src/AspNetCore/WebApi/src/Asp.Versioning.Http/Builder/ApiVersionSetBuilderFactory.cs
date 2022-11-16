// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Builder;

/// <summary>
/// Creates and returns a new API version set builder.
/// </summary>
/// <param name="name">The name of the API associated with the builder, if any.</param>
/// <returns>A new <see cref="ApiVersionSetBuilder">API version set builder</see>.</returns>
public delegate ApiVersionSetBuilder ApiVersionSetBuilderFactory( string? name = default );