// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Builder;

/// <summary>
/// Represents the default API version set builder factory.
/// </summary>
public class DefaultApiVersionSetBuilderFactory : IApiVersionSetBuilderFactory
{
    /// <inheritdoc />
    public ApiVersionSetBuilder Create( string? name = default ) => CreateInstance( name );

    /// <summary>
    /// Creates and returns a new builder instance.
    /// </summary>
    /// <param name="name">The optional name associated with the builder.</param>
    /// <returns>A new <see cref="ApiVersionSetBuilder">API version set builder</see>.</returns>
    protected virtual ApiVersionSetBuilder CreateInstance( string? name ) => new( name );
}