// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Builder;

using Asp.Versioning.Conventions;
using Microsoft.AspNetCore.Builder;

/// <summary>
/// Represents a versioned <see cref="IEndpointConventionBuilder"/>.
/// </summary>
[CLSCompliant( false )]
public class VersionedEndpointConventionBuilder :
    ApiVersionConventionBuilderBase,
    IVersionedEndpointConventionBuilder
{
    private readonly IEndpointConventionBuilder inner;
    private HashSet<ApiVersion>? mapped;

    /// <summary>
    /// Initializes a new instance of the <see cref="VersionedEndpointConventionBuilder"/> class.
    /// </summary>
    /// <param name="inner">The inner <see cref="IEndpointConventionBuilder"/> the new
    /// instance decorates.</param>
    /// <param name="apiVersionSet">The associated <see cref="ApiVersionSet">API version
    /// set</see>.</param>
    public VersionedEndpointConventionBuilder(
        IEndpointConventionBuilder inner,
        ApiVersionSet apiVersionSet )
    {
        this.inner = inner ?? throw new ArgumentNullException( nameof( inner ) );
        VersionSet = apiVersionSet ?? throw new ArgumentNullException( nameof( apiVersionSet ) );
    }

    /// <inheritdoc />
    public bool ReportApiVersions { get; set; }

    /// <summary>
    /// Gets the associated API version set.
    /// </summary>
    /// <value>The associated <see cref="ApiVersionSet">API version set</see>.</value>
    protected ApiVersionSet VersionSet { get; }

    /// <inheritdoc />
    protected override bool IsEmpty => ( mapped is null || mapped.Count == 0 ) && base.IsEmpty;

    /// <summary>
    /// Gets the collection of API versions mapped to the current action.
    /// </summary>
    /// <value>A <see cref="ICollection{T}">collection</see> of mapped <see cref="ApiVersion">API versions</see>.</value>
    protected ICollection<ApiVersion> MappedVersions => mapped ??= new();

    /// <summary>
    /// Maps the specified API version to the configured controller action.
    /// </summary>
    /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> to map to the action.</param>
    /// <returns>The original <see cref="IVersionedEndpointConventionBuilder"/>.</returns>
    public virtual IVersionedEndpointConventionBuilder MapToApiVersion( ApiVersion apiVersion )
    {
        MappedVersions.Add( apiVersion );
        return this;
    }

    /// <summary>
    /// Indicates that the action is API version-neutral.
    /// </summary>
    /// <returns>The original <see cref="IVersionedEndpointConventionBuilder"/>.</returns>
    public virtual IVersionedEndpointConventionBuilder IsApiVersionNeutral()
    {
        VersionNeutral = true;
        return this;
    }

    /// <summary>
    /// Indicates that the specified API version is supported by the configured action.
    /// </summary>
    /// <param name="apiVersion">The supported <see cref="ApiVersion">API version</see> implemented by the action.</param>
    /// <returns>The original <see cref="IVersionedEndpointConventionBuilder"/>.</returns>
    public virtual IVersionedEndpointConventionBuilder HasApiVersion( ApiVersion apiVersion )
    {
        SupportedVersions.Add( apiVersion );
        VersionSet.AdvertisesApiVersion( apiVersion );
        return this;
    }

    /// <summary>
    /// Indicates that the specified API version is deprecated by the configured action.
    /// </summary>
    /// <param name="apiVersion">The deprecated <see cref="ApiVersion">API version</see> implemented by the action.</param>
    /// <returns>The original <see cref="IVersionedEndpointConventionBuilder"/>.</returns>
    public virtual IVersionedEndpointConventionBuilder HasDeprecatedApiVersion( ApiVersion apiVersion )
    {
        DeprecatedVersions.Add( apiVersion );
        VersionSet.AdvertisesDeprecatedApiVersion( apiVersion );
        return this;
    }

    /// <summary>
    /// Indicates that the specified API version is advertised by the configured action.
    /// </summary>
    /// <param name="apiVersion">The advertised <see cref="ApiVersion">API version</see> not directly implemented by the action.</param>
    /// <returns>The original <see cref="IVersionedEndpointConventionBuilder"/>.</returns>
    public virtual IVersionedEndpointConventionBuilder AdvertisesApiVersion( ApiVersion apiVersion )
    {
        AdvertisedVersions.Add( apiVersion );
        VersionSet.AdvertisesApiVersion( apiVersion );
        return this;
    }

    /// <summary>
    /// Indicates that the specified API version is advertised and deprecated by the configured action.
    /// </summary>
    /// <param name="apiVersion">The advertised, but deprecated <see cref="ApiVersion">API version</see> not directly implemented by the action.</param>
    /// <returns>The original <see cref="IVersionedEndpointConventionBuilder"/>.</returns>
    public virtual IVersionedEndpointConventionBuilder AdvertisesDeprecatedApiVersion( ApiVersion apiVersion )
    {
        DeprecatedAdvertisedVersions.Add( apiVersion );
        VersionSet.AdvertisesDeprecatedApiVersion( apiVersion );
        return this;
    }

    /// <inheritdoc />
    public virtual void Add( Action<EndpointBuilder> convention ) => inner.Add( convention );

    /// <summary>
    /// Builds and returns a new API version metadata.
    /// </summary>
    /// <returns>A new <see cref="ApiVersionMetadata">API version metadata</see>.</returns>
    protected virtual ApiVersionMetadata Build()
    {
        var name = VersionSet.Name;
        ApiVersionModel? apiModel;

        if ( VersionNeutral || ( apiModel = VersionSet.Build() ).IsApiVersionNeutral )
        {
            if ( string.IsNullOrEmpty( name ) )
            {
                return ApiVersionMetadata.Neutral;
            }
            else
            {
                return new( ApiVersionModel.Neutral, ApiVersionModel.Neutral, name );
            }
        }

        ApiVersion[] emptyVersions;
        var inheritedSupported = apiModel.SupportedApiVersions;
        var inheritedDeprecated = apiModel.DeprecatedApiVersions;
        var noInheritedApiVersions = inheritedSupported.Count == 0 &&
                                     inheritedDeprecated.Count == 0;

        if ( IsEmpty )
        {
            if ( noInheritedApiVersions )
            {
                return new( apiModel, ApiVersionModel.Empty, name );
            }

            emptyVersions = Array.Empty<ApiVersion>();

            return new(
                apiModel,
                new(
                    declaredVersions: emptyVersions,
                    inheritedSupported,
                    inheritedDeprecated,
                    emptyVersions,
                    emptyVersions ),
                name );
        }

        if ( mapped is null || mapped.Count == 0 )
        {
            return new(
                ApiVersionModel.Empty,
                new(
                    declaredVersions: SupportedVersions.Union( DeprecatedVersions ),
                    SupportedVersions.Union( inheritedSupported ),
                    DeprecatedVersions.Union( inheritedDeprecated ),
                    AdvertisedVersions,
                    DeprecatedAdvertisedVersions ),
                name );
        }

        emptyVersions = Array.Empty<ApiVersion>();

        return new(
            apiModel,
            new(
                declaredVersions: mapped,
                supportedVersions: inheritedSupported,
                deprecatedVersions: inheritedDeprecated,
                advertisedVersions: emptyVersions,
                deprecatedAdvertisedVersions: emptyVersions ),
            name );
    }

    ApiVersionMetadata IVersionedEndpointConventionBuilder.Build() => Build();

    void IDeclareApiVersionConventionBuilder.IsApiVersionNeutral() => IsApiVersionNeutral();

    void IDeclareApiVersionConventionBuilder.HasApiVersion( ApiVersion apiVersion ) => HasApiVersion( apiVersion );

    void IDeclareApiVersionConventionBuilder.HasDeprecatedApiVersion( ApiVersion apiVersion ) => HasDeprecatedApiVersion( apiVersion );

    void IDeclareApiVersionConventionBuilder.AdvertisesApiVersion( ApiVersion apiVersion ) => AdvertisesApiVersion( apiVersion );

    void IDeclareApiVersionConventionBuilder.AdvertisesDeprecatedApiVersion( ApiVersion apiVersion ) => AdvertisesDeprecatedApiVersion( apiVersion );

    void IMapToApiVersionConventionBuilder.MapToApiVersion( ApiVersion apiVersion ) => MapToApiVersion( apiVersion );
}