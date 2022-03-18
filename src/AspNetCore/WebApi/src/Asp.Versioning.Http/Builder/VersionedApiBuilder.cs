// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Builder;

using Asp.Versioning;
using Asp.Versioning.Conventions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

/// <summary>
/// Represents a builder for versions applied to an API.
/// </summary>
[CLSCompliant( false )]
public class VersionedApiBuilder : ApiVersionConventionBuilderBase, IVersionedApiBuilder
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VersionedApiBuilder"/> class.
    /// </summary>
    /// <param name="routeBuilder">The underlying endpoint route builder.</param>
    /// <param name="name">The optional display name of the API.</param>
    public VersionedApiBuilder( IEndpointRouteBuilder routeBuilder, string? name = default )
    {
        RouteBuilder = routeBuilder;
        Name = name;
    }

    /// <inheritdoc />
    public IServiceProvider ServiceProvider => RouteBuilder.ServiceProvider;

    /// <inheritdoc />
    public ICollection<EndpointDataSource> DataSources => RouteBuilder.DataSources;

    /// <inheritdoc />
    public bool ReportApiVersions { get; set; }

    /// <inheritdoc />
    public string? Name { get; }

    /// <summary>
    /// Gets the underlying endpoint route builder.
    /// </summary>
    /// <value>The underlying <see cref="IEndpointRouteBuilder">endpoint route builder</see>.</value>
    protected IEndpointRouteBuilder RouteBuilder { get; }

    /// <summary>
    /// Configures the endpoint mappings for a versioned API.
    /// </summary>
    /// <param name="map">The action used to map endpoints using the provided
    /// <see cref="IVersionedEndpointBuilder">builder</see>.</param>
    public virtual void HasMapping( Action<VersionedEndpointBuilder> map )
    {
        if ( map == null )
        {
            throw new ArgumentNullException( nameof( map ) );
        }

        map( NewEndpointBuilder() );
    }

    /// <inheritdoc />
    public virtual ApiVersionModel Build()
    {
        if ( VersionNeutral )
        {
            return ApiVersionModel.Neutral;
        }

        if ( IsEmpty )
        {
            var options = RouteBuilder.ServiceProvider.GetRequiredService<IOptions<ApiVersioningOptions>>().Value;
            return new( options.DefaultApiVersion );
        }

        return new(
            declaredVersions: SupportedVersions.Union( DeprecatedVersions ),
            SupportedVersions,
            DeprecatedVersions,
            AdvertisedVersions,
            DeprecatedAdvertisedVersions );
    }

    /// <summary>
    /// Indicates that the controller is API version-neutral.
    /// </summary>
    /// <returns>The original <see cref="VersionedApiBuilder"/>.</returns>
    public virtual VersionedApiBuilder IsApiVersionNeutral()
    {
        VersionNeutral = true;
        return this;
    }

    /// <summary>
    /// Indicates that the specified API version is supported by the configured controller.
    /// </summary>
    /// <param name="apiVersion">The supported <see cref="ApiVersion">API version</see> implemented by the controller.</param>
    /// <returns>The original <see cref="VersionedApiBuilder"/>.</returns>
    public virtual VersionedApiBuilder HasApiVersion( ApiVersion apiVersion )
    {
        SupportedVersions.Add( apiVersion );
        return this;
    }

    /// <summary>
    /// Indicates that the specified API version is deprecated by the configured controller.
    /// </summary>
    /// <param name="apiVersion">The deprecated <see cref="ApiVersion">API version</see> implemented by the controller.</param>
    /// <returns>The original <see cref="VersionedApiBuilder"/>.</returns>
    public virtual VersionedApiBuilder HasDeprecatedApiVersion( ApiVersion apiVersion )
    {
        DeprecatedVersions.Add( apiVersion );
        return this;
    }

    /// <summary>
    /// Indicates that the specified API version is advertised by the configured controller.
    /// </summary>
    /// <param name="apiVersion">The advertised <see cref="ApiVersion">API version</see> not directly implemented by the controller.</param>
    /// <returns>The original <see cref="VersionedApiBuilder"/>.</returns>
    public virtual VersionedApiBuilder AdvertisesApiVersion( ApiVersion apiVersion )
    {
        AdvertisedVersions.Add( apiVersion );
        return this;
    }

    /// <summary>
    /// Indicates that the specified API version is advertised and deprecated by the configured controller.
    /// </summary>
    /// <param name="apiVersion">The advertised, but deprecated <see cref="ApiVersion">API version</see> not directly implemented by the controller.</param>
    /// <returns>The original <see cref="VersionedApiBuilder"/>.</returns>
    public virtual VersionedApiBuilder AdvertisesDeprecatedApiVersion( ApiVersion apiVersion )
    {
        DeprecatedAdvertisedVersions.Add( apiVersion );
        return this;
    }

    /// <summary>
    /// Creates and returns a new endpoint builder.
    /// </summary>
    /// <returns>A new <see cref="VersionedEndpointBuilder">Minimal API endpoint builder</see>.</returns>
    protected virtual VersionedEndpointBuilder NewEndpointBuilder() => new( this );

    void IDeclareApiVersionConventionBuilder.IsApiVersionNeutral() => IsApiVersionNeutral();

    void IDeclareApiVersionConventionBuilder.HasApiVersion( ApiVersion apiVersion ) => HasApiVersion( apiVersion );

    void IDeclareApiVersionConventionBuilder.HasDeprecatedApiVersion( ApiVersion apiVersion ) => HasDeprecatedApiVersion( apiVersion );

    void IDeclareApiVersionConventionBuilder.AdvertisesApiVersion( ApiVersion apiVersion ) => AdvertisesApiVersion( apiVersion );

    void IDeclareApiVersionConventionBuilder.AdvertisesDeprecatedApiVersion( ApiVersion apiVersion ) => AdvertisesDeprecatedApiVersion( apiVersion );

    void IVersionedApiBuilder.HasMapping( Action<IVersionedEndpointBuilder> map )
    {
        if ( map == null )
        {
            throw new ArgumentNullException( nameof( map ) );
        }

        map( NewEndpointBuilder() );
    }
}