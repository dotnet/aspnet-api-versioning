// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Builder;

using Asp.Versioning.Conventions;

/// <summary>
/// Represents the builder for an API version set.
/// </summary>
public class ApiVersionSetBuilder : ApiVersionConventionBuilderBase, IDeclareApiVersionConventionBuilder
{
    private readonly string? name;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiVersionSetBuilder"/> class.
    /// </summary>
    /// <param name="name">The name of the API, if any.</param>
    public ApiVersionSetBuilder( string? name ) => this.name = name;

    /// <summary>
    /// Gets or sets a value indicating whether requests report the API version compatibility information in responses.
    /// </summary>
    /// <value>True if API versions are reported; otherwise, false.</value>
    protected internal bool WillReportApiVersions { get; set; }

    /// <summary>
    /// Builds and returns a new API versioning configuration.
    /// </summary>
    /// <returns>A new <see cref="ApiVersionSet">API versioning configuration</see>.</returns>
    public virtual ApiVersionSet Build() => new( this, name );

    /// <summary>
    /// Indicates that all APIs in the version set will report their versions.
    /// </summary>
    /// <returns>The original <see cref="ApiVersionSetBuilder"/> instance.</returns>
    public virtual ApiVersionSetBuilder ReportApiVersions()
    {
        WillReportApiVersions = true;
        return this;
    }

    /// <summary>
    /// Indicates that all APIs in the version set are API version-neutral.
    /// </summary>
    /// <returns>The original <see cref="ApiVersionSetBuilder"/>.</returns>
    public virtual ApiVersionSetBuilder IsApiVersionNeutral()
    {
        VersionNeutral = true;
        return this;
    }

    /// <summary>
    /// Indicates that the specified API version is supported by all APIs in the version set.
    /// </summary>
    /// <param name="apiVersion">The supported <see cref="ApiVersion">API version</see>.</param>
    /// <returns>The original <see cref="ApiVersionSetBuilder"/>.</returns>
    public virtual ApiVersionSetBuilder HasApiVersion( ApiVersion apiVersion )
    {
        SupportedVersions.Add( apiVersion );
        return this;
    }

    /// <summary>
    /// Indicates that the specified API version is deprecated by all APIs in the version set.
    /// </summary>
    /// <param name="apiVersion">The deprecated <see cref="ApiVersion">API version</see>.</param>
    /// <returns>The original <see cref="ApiVersionSetBuilder"/>.</returns>
    public virtual ApiVersionSetBuilder HasDeprecatedApiVersion( ApiVersion apiVersion )
    {
        DeprecatedVersions.Add( apiVersion );
        return this;
    }

    /// <summary>
    /// Indicates that the specified API version is advertised by all APIs in the version set.
    /// </summary>
    /// <param name="apiVersion">The advertised <see cref="ApiVersion">API version</see>.</param>
    /// <returns>The original <see cref="ApiVersionSetBuilder"/>.</returns>
    public virtual ApiVersionSetBuilder AdvertisesApiVersion( ApiVersion apiVersion )
    {
        AdvertisedVersions.Add( apiVersion );
        return this;
    }

    /// <summary>
    /// Indicates that the specified API version is advertised and deprecated by all APIs in the version set.
    /// </summary>
    /// <param name="apiVersion">The advertised, but deprecated <see cref="ApiVersion">API version</see>.</param>
    /// <returns>The original <see cref="ApiVersionSetBuilder"/>.</returns>
    public virtual ApiVersionSetBuilder AdvertisesDeprecatedApiVersion( ApiVersion apiVersion )
    {
        DeprecatedAdvertisedVersions.Add( apiVersion );
        return this;
    }

    void IDeclareApiVersionConventionBuilder.IsApiVersionNeutral() => IsApiVersionNeutral();

    void IDeclareApiVersionConventionBuilder.HasApiVersion( ApiVersion apiVersion ) => HasApiVersion( apiVersion );

    void IDeclareApiVersionConventionBuilder.HasDeprecatedApiVersion( ApiVersion apiVersion ) => HasDeprecatedApiVersion( apiVersion );

    void IDeclareApiVersionConventionBuilder.AdvertisesApiVersion( ApiVersion apiVersion ) => AdvertisesApiVersion( apiVersion );

    void IDeclareApiVersionConventionBuilder.AdvertisesDeprecatedApiVersion( ApiVersion apiVersion ) => AdvertisesDeprecatedApiVersion( apiVersion );

    /// <summary>
    /// Builds and returns an API version model.
    /// </summary>
    /// <param name="options">The configured <see cref="ApiVersioningOptions">API versioning options</see>.</param>
    /// <returns>A new <see cref="ApiVersionModel">API version model</see>.</returns>
    protected internal virtual ApiVersionModel BuildApiVersionModel( ApiVersioningOptions options )
    {
        ArgumentNullException.ThrowIfNull( options );

        if ( VersionNeutral )
        {
            return ApiVersionModel.Neutral;
        }

        if ( IsEmpty )
        {
            return new( options.DefaultApiVersion );
        }

        return new(
            declaredVersions: SupportedVersions.Union( DeprecatedVersions ),
            SupportedVersions,
            DeprecatedVersions,
            AdvertisedVersions,
            DeprecatedAdvertisedVersions );
    }
}