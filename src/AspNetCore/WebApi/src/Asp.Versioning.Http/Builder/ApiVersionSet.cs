// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Builder;

/// <summary>
/// Represents an API version set.
/// </summary>
public class ApiVersionSet
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApiVersionSet"/> class.
    /// </summary>
    /// <param name="builder">The associated <see cref="ApiVersionSetBuilder">builder</see>.</param>
    /// <param name="name">The optional API name.</param>
    public ApiVersionSet( ApiVersionSetBuilder builder, string? name )
    {
        Builder = builder ?? throw new ArgumentNullException( nameof( builder ) );
        Name = name;
        ReportApiVersions = builder.WillReportApiVersions;
    }

    /// <summary>
    /// Gets the configured API name, if any.
    /// </summary>
    /// <value>The configured API name or <c>null</c>.</value>
    public string? Name { get; }

    /// <summary>
    /// Gets a value indicating whether all APIs in the version set will report their API versions.
    /// </summary>
    /// <value>True if all APIs in the version set will report their API versions; otherwise, false.</value>
    public bool ReportApiVersions { get; }

    // intentionally internal for 6.0, until EndpointBuilder.ServiceProvider is exposed in 7.0
    // REF: https://github.com/dotnet/aspnetcore/pull/41238/files#diff-f8807c470bcc3a077fb176668a46df57b4bb99c992b6b7b375665f8bf3903c94R510
    internal IServiceProvider? ServiceProvider { get; set; }

    /// <summary>
    /// Gets the associated builder.
    /// </summary>
    /// <value>The associated <see cref="ApiVersionSetBuilder">builder</see>.</value>
    protected ApiVersionSetBuilder Builder { get; }

    /// <summary>
    /// Builds and returns the API version model for the version set.
    /// </summary>
    /// <param name="options">The configured <see cref="ApiVersioningOptions">API versioning options</see>.</param>
    /// <returns>A new <see cref="ApiVersionModel">API version model</see>.</returns>
    public virtual ApiVersionModel Build( ApiVersioningOptions options ) => Builder.BuildApiVersionModel( options );

    /// <summary>
    /// Advertises that the specified API version is supported in the version set.
    /// </summary>
    /// <param name="apiVersion">The advertised <see cref="ApiVersion">API version</see>.</param>
    public virtual void AdvertisesApiVersion( ApiVersion apiVersion ) =>
        Builder.AdvertisesApiVersion( apiVersion );

    /// <summary>
    /// Advertises that the specified API version is deprecated in the version set.
    /// </summary>
    /// <param name="apiVersion">The advertised, but deprecated <see cref="ApiVersion">API version</see>.</param>
    public virtual void AdvertisesDeprecatedApiVersion( ApiVersion apiVersion ) =>
        Builder.AdvertisesDeprecatedApiVersion( apiVersion );
}