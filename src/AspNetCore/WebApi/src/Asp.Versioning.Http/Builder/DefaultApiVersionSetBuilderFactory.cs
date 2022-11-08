// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Builder;

/// <summary>
/// Represents the default API version set builder factory.
/// </summary>
public class DefaultApiVersionSetBuilderFactory : IApiVersionSetBuilderFactory
{
    private readonly IServiceProvider serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultApiVersionSetBuilderFactory"/> class.
    /// </summary>
    /// <param name="serviceProvider">The underlying <see cref="IServiceProvider">service provider</see>.</param>
    public DefaultApiVersionSetBuilderFactory( IServiceProvider serviceProvider ) =>
        this.serviceProvider = serviceProvider;

    /// <inheritdoc />
    public ApiVersionSetBuilder Create( string? name = default )
    {
        var instance = CreateInstance( name );
        instance.ServiceProvider = serviceProvider;
        return instance;
    }

    /// <summary>
    /// Creates and returns a new builder instance.
    /// </summary>
    /// <param name="name">The optional name associated with the builder.</param>
    /// <returns>A new <see cref="ApiVersionSetBuilder">API version set builder</see>.</returns>
    protected virtual ApiVersionSetBuilder CreateInstance( string? name ) => new( name );
}