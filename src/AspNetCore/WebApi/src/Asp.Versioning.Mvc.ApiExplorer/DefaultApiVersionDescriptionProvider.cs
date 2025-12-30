// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.ApiExplorer;

using Asp.Versioning.ApiExplorer.Internal;
using Microsoft.Extensions.Options;

/// <summary>
/// Represents the default implementation of an object that discovers and describes the API version information within an application.
/// </summary>
[CLSCompliant( false )]
public class DefaultApiVersionDescriptionProvider : IApiVersionDescriptionProvider
{
    private readonly ApiVersionDescriptionCollection<GroupedApiVersionMetadata> collection;
    private readonly IOptions<ApiExplorerOptions> options;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultApiVersionDescriptionProvider"/> class.
    /// </summary>
    /// <param name="providers">The <see cref="IEnumerable{T}">sequence</see> of
    /// <see cref="IApiVersionMetadataCollationProvider">API version metadata collation providers.</see>.</param>
    /// <param name="sunsetPolicyManager">The <see cref="IPolicyManager{TPolicy}">manager</see> used to resolve sunset policies.</param>
    /// <param name="deprecationPolicyManager">The <see cref="IPolicyManager{TPolicy}">manager</see> used to resolve deprecation policies.</param>
    /// <param name="apiExplorerOptions">The <see cref="IOptions{TOptions}">container</see> of configured
    /// <see cref="ApiExplorerOptions">API explorer options</see>.</param>
    public DefaultApiVersionDescriptionProvider(
        IEnumerable<IApiVersionMetadataCollationProvider> providers,
        IPolicyManager<SunsetPolicy> sunsetPolicyManager,
        IPolicyManager<DeprecationPolicy> deprecationPolicyManager,
        IOptions<ApiExplorerOptions> apiExplorerOptions )
    {
        collection = new( Describe, providers ?? throw new ArgumentNullException( nameof( providers ) ) );
        SunsetPolicyManager = sunsetPolicyManager;
        DeprecationPolicyManager = deprecationPolicyManager;
        options = apiExplorerOptions;
    }

    /// <summary>
    /// Gets the manager used to resolve sunset policies.
    /// </summary>
    /// <value>The associated <see cref="IPolicyManager{TPolicy}">sunset policy manager</see>.</value>
    protected IPolicyManager<SunsetPolicy> SunsetPolicyManager { get; }

    /// <summary>
    /// Gets the manager used to resolve deprecation policies.
    /// </summary>
    /// <value>The associated <see cref="IPolicyManager{TPolicy}">deprecation policy manager</see>.</value>
    protected IPolicyManager<DeprecationPolicy> DeprecationPolicyManager { get; }

    /// <summary>
    /// Gets the options associated with the API explorer.
    /// </summary>
    /// <value>The current <see cref="ApiExplorerOptions">API explorer options</see>.</value>
    protected ApiExplorerOptions Options => options.Value;

    /// <inheritdoc />
    public IReadOnlyList<ApiVersionDescription> ApiVersionDescriptions => collection.Items;

    /// <summary>
    /// Provides a list of API version descriptions from a list of application API version metadata.
    /// </summary>
    /// <param name="metadata">The <see cref="IReadOnlyList{T}">read-only list</see> of <see cref="ApiVersionMetadata">API version metadata</see>
    /// within the application.</param>
    /// <returns>A <see cref="IReadOnlyList{T}">read-only list</see> of <see cref="ApiVersionDescription">API version descriptions</see>.</returns>
    protected virtual IReadOnlyList<ApiVersionDescription> Describe( IReadOnlyList<ApiVersionMetadata> metadata )
    {
        ArgumentNullException.ThrowIfNull( metadata );

        // TODO: consider refactoring and removing GroupedApiVersionDescriptionProvider as both implementations are now
        // effectively the same. this cast is safe as an internal implementation detail. if this method is
        // overridden, then this code doesn't even run
        //
        // REF: https://github.com/dotnet/aspnet-api-versioning/issues/1066
        if ( metadata is GroupedApiVersionMetadata[] groupedMetadata )
        {
            return DescriptionProvider.Describe( groupedMetadata, SunsetPolicyManager, DeprecationPolicyManager, Options );
        }

        return [];
    }

    private sealed class GroupedApiVersionMetadata :
        ApiVersionMetadata,
        IEquatable<GroupedApiVersionMetadata>,
        IGroupedApiVersionMetadata,
        IGroupedApiVersionMetadataFactory<GroupedApiVersionMetadata>
    {
        private GroupedApiVersionMetadata( string? groupName, ApiVersionMetadata metadata )
            : base( metadata ) => GroupName = groupName;

        public string? GroupName { get; }

        static GroupedApiVersionMetadata IGroupedApiVersionMetadataFactory<GroupedApiVersionMetadata>.New(
            string? groupName,
            ApiVersionMetadata metadata ) => new( groupName, metadata );

        public bool Equals( GroupedApiVersionMetadata? other ) =>
            other is not null && other.GetHashCode() == GetHashCode();

        public override bool Equals( object? obj ) =>
            obj is not null &&
            GetType().Equals( obj.GetType() ) &&
            GetHashCode() == obj.GetHashCode();

        public override int GetHashCode()
        {
            var hash = default( HashCode );

            if ( !string.IsNullOrEmpty( GroupName ) )
            {
                hash.Add( GroupName, StringComparer.Ordinal );
            }

            hash.Add( base.GetHashCode() );

            return hash.ToHashCode();
        }
    }
}