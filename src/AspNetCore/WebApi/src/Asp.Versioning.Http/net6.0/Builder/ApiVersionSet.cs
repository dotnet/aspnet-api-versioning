// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Builder;

using Microsoft.Extensions.Options;
using Opts = Microsoft.Extensions.Options.Options;

/// <summary>
/// Represents an API version set.
/// </summary>
public class ApiVersionSet
{
    private readonly IOptions<ApiVersioningOptions> options;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiVersionSet"/> class.
    /// </summary>
    /// <param name="builder">The associated <see cref="ApiVersionSetBuilder">builder</see>.</param>
    /// <param name="name">The optional API name.</param>
    /// <param name="parameterSource">The configured <see cref="IApiVersionParameterSource">API version
    /// parameter source</see>.</param>
    /// <param name="options">The configured <see cref="ApiVersioningOptions">API versioning options</see>.</param>
    public ApiVersionSet(
        ApiVersionSetBuilder builder,
        string? name,
        IApiVersionParameterSource parameterSource,
        ApiVersioningOptions options ) :
        this( builder, name, parameterSource, Opts.Create( options ) )
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiVersionSet"/> class.
    /// </summary>
    /// <param name="builder">The associated <see cref="ApiVersionSetBuilder">builder</see>.</param>
    /// <param name="name">The optional API name.</param>
    /// <param name="parameterSource">The configured <see cref="IApiVersionParameterSource">API version
    /// parameter source</see>.</param>
    /// <param name="options">The configured <see cref="ApiVersioningOptions">API versioning options</see>.</param>
    public ApiVersionSet(
        ApiVersionSetBuilder builder,
        string? name,
        IApiVersionParameterSource parameterSource,
        IOptions<ApiVersioningOptions> options )
    {
        Builder = builder ?? throw new ArgumentNullException( nameof( builder ) );
        Name = name;
        ReportApiVersions = builder.WillReportApiVersions;
        ParameterSource = parameterSource;
        this.options = options;
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

    /// <summary>
    /// Gets the configured API version parameter source.
    /// </summary>
    /// <value>The configured <see cref="IApiVersionParameterSource">API version parameter source</see>.</value>
    public IApiVersionParameterSource ParameterSource { get; }

    /// <summary>
    /// Gets the configured API versioning options.
    /// </summary>
    /// <value>The configured <see cref="ApiVersioningOptions">API versioning options</see>.</value>
    public ApiVersioningOptions Options => options.Value;

    /// <summary>
    /// Gets the associated builder.
    /// </summary>
    /// <value>The associated <see cref="ApiVersionSetBuilder">builder</see>.</value>
    protected ApiVersionSetBuilder Builder { get; }

    /// <summary>
    /// Builds and returns the API version model for the version set.
    /// </summary>
    /// <returns>A new <see cref="ApiVersionModel">API version model</see>.</returns>
    public virtual ApiVersionModel Build() => Builder.BuildApiVersionModel();

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