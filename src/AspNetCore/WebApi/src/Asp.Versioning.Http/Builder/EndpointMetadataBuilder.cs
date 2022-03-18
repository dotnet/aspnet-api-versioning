// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Builder;

using Asp.Versioning.Conventions;
using Microsoft.AspNetCore.Builder;

/// <summary>
/// Represents a metadata builder for API versions applied to an endpoint.
/// </summary>
[CLSCompliant( false )]
public class EndpointMetadataBuilder : ApiVersionConventionBuilderBase, IEndpointMetadataBuilder, IEndpointConventionBuilder
{
    private HashSet<ApiVersion>? mapped;

    /// <summary>
    /// Initializes a new instance of the <see cref="EndpointMetadataBuilder"/> class.
    /// </summary>
    /// <param name="apiBuilder">The parent <see cref="IVersionedApiBuilder"/>.</param>
    /// <param name="conventionBuilder">The parent <see cref="IEndpointConventionBuilder"/>.</param>
    public EndpointMetadataBuilder( IVersionedApiBuilder apiBuilder, IEndpointConventionBuilder conventionBuilder )
    {
        ApiBuilder = apiBuilder ?? throw new ArgumentNullException( nameof( apiBuilder ) );
        ConventionBuilder = conventionBuilder ?? throw new ArgumentNullException( nameof( conventionBuilder ) );
        ServiceProvider = apiBuilder.ServiceProvider;
        ReportApiVersions = apiBuilder.ReportApiVersions;
    }

    /// <inheritdoc />
    public IServiceProvider ServiceProvider { get; }

    /// <inheritdoc />
    public bool ReportApiVersions { get; set; }

    /// <inheritdoc />
    protected override bool IsEmpty => ( mapped is null || mapped.Count == 0 ) && base.IsEmpty;

    /// <summary>
    /// Gets the collection of API versions mapped to the current action.
    /// </summary>
    /// <value>A <see cref="ICollection{T}">collection</see> of mapped <see cref="ApiVersion">API versions</see>.</value>
    protected ICollection<ApiVersion> MappedVersions => mapped ??= new();

    /// <summary>
    /// Gets the underlying versioned API builder.
    /// </summary>
    /// <value>The underlying <see cref="IVersionedApiBuilder">versioned API builder</see>.</value>
    protected IVersionedApiBuilder ApiBuilder { get; }

    /// <summary>
    /// Gets the underlying endpoint convention builder.
    /// </summary>
    /// <value>The underlying <see cref="IEndpointConventionBuilder">endpoint convention builder</see>.</value>
    protected IEndpointConventionBuilder ConventionBuilder { get; }

    /// <inheritdoc />
    public virtual ApiVersionMetadata Build()
    {
        ApiVersionModel? apiModel;

        if ( VersionNeutral || ( apiModel = ApiBuilder.Build() ).IsApiVersionNeutral )
        {
            if ( string.IsNullOrEmpty( ApiBuilder.Name ) )
            {
                return ApiVersionMetadata.Neutral;
            }
            else
            {
                return new( ApiVersionModel.Neutral, ApiVersionModel.Neutral, ApiBuilder.Name );
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
                return new( apiModel, ApiVersionModel.Empty, ApiBuilder.Name );
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
                ApiBuilder.Name );
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
                ApiBuilder.Name );
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
            ApiBuilder.Name );
    }

    /// <summary>
    /// Maps the specified API version to the configured controller action.
    /// </summary>
    /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> to map to the action.</param>
    /// <returns>The original <see cref="EndpointMetadataBuilder"/>.</returns>
    public virtual EndpointMetadataBuilder MapToApiVersion( ApiVersion apiVersion )
    {
        MappedVersions.Add( apiVersion );
        return this;
    }

    /// <summary>
    /// Indicates that the action is API version-neutral.
    /// </summary>
    /// <returns>The original <see cref="EndpointMetadataBuilder"/>.</returns>
    public virtual EndpointMetadataBuilder IsApiVersionNeutral()
    {
        VersionNeutral = true;
        return this;
    }

    /// <summary>
    /// Indicates that the specified API version is supported by the configured action.
    /// </summary>
    /// <param name="apiVersion">The supported <see cref="ApiVersion">API version</see> implemented by the action.</param>
    /// <returns>The original <see cref="EndpointMetadataBuilder"/>.</returns>
    public virtual EndpointMetadataBuilder HasApiVersion( ApiVersion apiVersion )
    {
        SupportedVersions.Add( apiVersion );
        return this;
    }

    /// <summary>
    /// Indicates that the specified API version is deprecated by the configured action.
    /// </summary>
    /// <param name="apiVersion">The deprecated <see cref="ApiVersion">API version</see> implemented by the action.</param>
    /// <returns>The original <see cref="EndpointMetadataBuilder"/>.</returns>
    public virtual EndpointMetadataBuilder HasDeprecatedApiVersion( ApiVersion apiVersion )
    {
        DeprecatedVersions.Add( apiVersion );
        return this;
    }

    /// <summary>
    /// Indicates that the specified API version is advertised by the configured action.
    /// </summary>
    /// <param name="apiVersion">The advertised <see cref="ApiVersion">API version</see> not directly implemented by the action.</param>
    /// <returns>The original <see cref="EndpointMetadataBuilder"/>.</returns>
    public virtual EndpointMetadataBuilder AdvertisesApiVersion( ApiVersion apiVersion )
    {
        AdvertisedVersions.Add( apiVersion );
        return this;
    }

    /// <summary>
    /// Indicates that the specified API version is advertised and deprecated by the configured action.
    /// </summary>
    /// <param name="apiVersion">The advertised, but deprecated <see cref="ApiVersion">API version</see> not directly implemented by the action.</param>
    /// <returns>The original <see cref="EndpointMetadataBuilder"/>.</returns>
    public virtual EndpointMetadataBuilder AdvertisesDeprecatedApiVersion( ApiVersion apiVersion )
    {
        DeprecatedAdvertisedVersions.Add( apiVersion );
        return this;
    }

    /// <inheritdoc />
    public virtual void Add( Action<EndpointBuilder> convention ) => ConventionBuilder.Add( convention );

    void IDeclareApiVersionConventionBuilder.IsApiVersionNeutral() => IsApiVersionNeutral();

    void IDeclareApiVersionConventionBuilder.HasApiVersion( ApiVersion apiVersion ) => HasApiVersion( apiVersion );

    void IDeclareApiVersionConventionBuilder.HasDeprecatedApiVersion( ApiVersion apiVersion ) => HasDeprecatedApiVersion( apiVersion );

    void IDeclareApiVersionConventionBuilder.AdvertisesApiVersion( ApiVersion apiVersion ) => AdvertisesApiVersion( apiVersion );

    void IDeclareApiVersionConventionBuilder.AdvertisesDeprecatedApiVersion( ApiVersion apiVersion ) => AdvertisesDeprecatedApiVersion( apiVersion );

    void IMapToApiVersionConventionBuilder.MapToApiVersion( ApiVersion apiVersion ) => MapToApiVersion( apiVersion );
}