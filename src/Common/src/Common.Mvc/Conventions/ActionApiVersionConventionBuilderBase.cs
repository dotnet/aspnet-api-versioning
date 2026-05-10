// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Conventions;

using static Asp.Versioning.ApiVersionProviderOptions;

/// <summary>
/// Represents the base implementation of a builder for API versions applied to a controller action.
/// </summary>
public abstract partial class ActionApiVersionConventionBuilderBase : ApiVersionConventionBuilderBase
{
    private HashSet<ApiVersion>? mapped;
    private List<IntroducedApiVersion>? introduced;

    /// <summary>
    /// Initializes a new instance of the <see cref="ActionApiVersionConventionBuilderBase"/> class.
    /// </summary>
    protected ActionApiVersionConventionBuilderBase() => NamingConvention = ControllerNameConvention.Default;

    /// <summary>
    /// Initializes a new instance of the <see cref="ActionApiVersionConventionBuilderBase"/> class.
    /// </summary>
    /// <param name="namingConvention">The <see cref="IControllerNameConvention">controller naming convention</see>.</param>
    protected ActionApiVersionConventionBuilderBase( IControllerNameConvention namingConvention ) => NamingConvention = namingConvention;

    /// <inheritdoc />
    protected override bool IsEmpty =>
        ( mapped is null || mapped.Count == 0 ) &&
        ( introduced is null || introduced.Count == 0 ) &&
        base.IsEmpty;

    /// <summary>
    /// Gets the collection of API versions mapped to the current action.
    /// </summary>
    /// <value>A <see cref="ICollection{T}">collection</see> of mapped <see cref="ApiVersion">API versions</see>.</value>
    protected ICollection<ApiVersion> MappedVersions => mapped ??= [];

    /// <summary>
    /// Gets a value indicating whether any explicit API version mappings are configured.
    /// </summary>
    /// <value>True if explicit API version mappings are configured; otherwise, false.</value>
    protected bool HasMappedVersions => ( mapped is not null && mapped.Count > 0 ) || ( introduced is not null && introduced.Count > 0 );

    /// <summary>
    /// Gets the collection of API versions in which the current action was introduced.
    /// </summary>
    /// <value>A <see cref="ICollection{T}">collection</see> of introduced <see cref="ApiVersion">API versions</see>.</value>
    /// <remarks>When multiple introduced API versions are configured, the latest version is used as the effective introduction.</remarks>
    protected ICollection<IntroducedApiVersion> IntroducedVersions => introduced ??= [];

    /// <summary>
    /// Gets the controller naming convention associated with the builder.
    /// </summary>
    /// <value>The <see cref="IControllerNameConvention">controller naming convention</see>.</value>
    public IControllerNameConvention NamingConvention { get; }

    /// <inheritdoc />
    protected override void MergeAttributesWithConventions( IReadOnlyList<object> attributes )
    {
        ArgumentNullException.ThrowIfNull( attributes );

        base.MergeAttributesWithConventions( attributes );

        for ( var i = 0; i < attributes.Count; i++ )
        {
            if ( attributes[i] is IIntroducedInApiVersionProvider introducedProvider )
            {
                for ( var j = 0; j < introducedProvider.Versions.Count; j++ )
                {
                    IntroducedVersions.Add( new( introducedProvider.Versions[j], introducedProvider.StatusCode ) );
                }
            }
            else if ( attributes[i] is IApiVersionProvider provider && provider.Options == Mapped )
            {
                MappedVersions.UnionWith( provider.Versions );
            }
        }
    }

    /// <summary>
    /// Adds the introduced API version metadata to the specified action.
    /// </summary>
    /// <param name="add">The callback used to add metadata.</param>
    protected void AddIntroducedApiVersionMetadata( Action<IntroducedInApiVersionMetadata> add )
    {
        ArgumentNullException.ThrowIfNull( add );

        var metadata = GetIntroducedApiVersionMetadata();

        for ( var i = 0; i < metadata.Length; i++ )
        {
            add( metadata[i] );
        }
    }

    /// <summary>
    /// Gets the introduced API version metadata for the current action.
    /// </summary>
    /// <returns>The introduced API version metadata.</returns>
    protected IntroducedInApiVersionMetadata[] GetIntroducedApiVersionMetadata()
    {
        if ( introduced is null || introduced.Count == 0 )
        {
            return [];
        }

        var metadata = new IntroducedInApiVersionMetadata[introduced.Count];

        introduced.Sort( static ( left, right ) => left.Version.CompareTo( right.Version ) );

        for ( var i = 0; i < introduced.Count; i++ )
        {
            var item = introduced[i];
            metadata[i] = new( item.Version, item.StatusCode );
        }

        return metadata;
    }

    /// <summary>
    /// Expands introduced API versions into the effective mapped API versions.
    /// </summary>
    /// <param name="declaredVersions">The API versions declared by the controller.</param>
    /// <returns>The effective mapped API versions.</returns>
    protected ICollection<ApiVersion> ExpandMappedVersions( IReadOnlyList<ApiVersion> declaredVersions )
    {
        ArgumentNullException.ThrowIfNull( declaredVersions );

        if ( introduced is null || introduced.Count == 0 )
        {
            return mapped ?? [];
        }

        var versions = mapped is null ? [] : new HashSet<ApiVersion>( mapped );

        var effectiveIntroduced = introduced[0].Version;

        for ( var i = 1; i < introduced.Count; i++ )
        {
            if ( introduced[i].Version > effectiveIntroduced )
            {
                effectiveIntroduced = introduced[i].Version;
            }
        }

        for ( var i = 0; i < declaredVersions.Count; i++ )
        {
            var declaredVersion = declaredVersions[i];

            if ( declaredVersion >= effectiveIntroduced )
            {
                versions.Add( declaredVersion );
            }
        }

        return versions;
    }

    /// <summary>
    /// Represents the API version in which an action was introduced.
    /// </summary>
    protected sealed class IntroducedApiVersion
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IntroducedApiVersion"/> class.
        /// </summary>
        /// <param name="version">The API version in which the action was introduced.</param>
        /// <param name="statusCode">The status code for earlier controller-declared API versions.</param>
        public IntroducedApiVersion( ApiVersion version, int statusCode )
        {
            Version = version;
            StatusCode = statusCode;
        }

        /// <summary>
        /// Gets the API version in which the action was introduced.
        /// </summary>
        public ApiVersion Version { get; }

        /// <summary>
        /// Gets the status code for earlier controller-declared API versions.
        /// </summary>
        public int StatusCode { get; }
    }
}