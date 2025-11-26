// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.ApiExplorer;

using Asp.Versioning.ApiExplorer.Internal;
using Microsoft.Extensions.Options;

/// <summary>
/// Represents the default implementation of an object that discovers and describes the API version information within an application.
/// </summary>
[CLSCompliant( false )]
public class GroupedApiVersionDescriptionProvider : IApiVersionDescriptionProvider
{
    private readonly ApiVersionDescriptionCollection<GroupedApiVersionMetadata> collection;
    private readonly IOptions<ApiExplorerOptions> options;

    /// <summary>
    /// Initializes a new instance of the <see cref="GroupedApiVersionDescriptionProvider"/> class.
    /// </summary>
    /// <param name="providers">The <see cref="IEnumerable{T}">sequence</see> of
    /// <see cref="IApiVersionMetadataCollationProvider">API version metadata collation providers.</see>.</param>
    /// <param name="sunsetPolicyManager">The <see cref="ISunsetPolicyManager">manager</see> used to resolve sunset policies.</param>
    /// <param name="deprecationPolicyManager">The <see cref="IDeprecationPolicyManager">manager</see> used to resolve deprecation policies.</param>
    /// <param name="apiExplorerOptions">The <see cref="IOptions{TOptions}">container</see> of configured
    /// <see cref="ApiExplorerOptions">API explorer options</see>.</param>
    public GroupedApiVersionDescriptionProvider(
        IEnumerable<IApiVersionMetadataCollationProvider> providers,
        ISunsetPolicyManager sunsetPolicyManager,
        IDeprecationPolicyManager deprecationPolicyManager,
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
    /// <value>The associated <see cref="ISunsetPolicyManager">sunset policy manager</see>.</value>
    protected ISunsetPolicyManager SunsetPolicyManager { get; }

    /// <summary>
    /// Gets the manager used to resolve deprecation policies.
    /// </summary>
    /// <value>The associated <see cref="IDeprecationPolicyManager">deprecation policy manager</see>.</value>
    protected IDeprecationPolicyManager DeprecationPolicyManager { get; }

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
    /// <param name="metadata">The <see cref="IReadOnlyList{T}">read-only list</see> of
    /// <see cref="GroupedApiVersionMetadata">grouped API version metadata</see> within the application.</param>
    /// <returns>A <see cref="IReadOnlyList{T}">read-only list</see> of <see cref="ApiVersionDescription">API
    /// version descriptions</see>.</returns>
    protected virtual IReadOnlyList<ApiVersionDescription> Describe( IReadOnlyList<GroupedApiVersionMetadata> metadata )
    {
        ArgumentNullException.ThrowIfNull( metadata );
        return DescriptionProvider.Describe( metadata, SunsetPolicyManager, DeprecationPolicyManager, Options );
    }

    /// <summary>
    /// Represents the API version metadata applied to an endpoint with an optional group name.
    /// </summary>
    protected class GroupedApiVersionMetadata :
        ApiVersionMetadata,
        IEquatable<GroupedApiVersionMetadata>,
        IGroupedApiVersionMetadata,
        IGroupedApiVersionMetadataFactory<GroupedApiVersionMetadata>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupedApiVersionMetadata"/> class.
        /// </summary>
        /// <param name="groupName">The associated group name.</param>
        /// <param name="metadata">The existing metadata to initialize from.</param>
        public GroupedApiVersionMetadata( string? groupName, ApiVersionMetadata metadata )
            : base( metadata ) => GroupName = groupName;

        /// <summary>
        /// Gets the associated group name.
        /// </summary>
        /// <value>The associated group name, if any.</value>
        public string? GroupName { get; }

        static GroupedApiVersionMetadata IGroupedApiVersionMetadataFactory<GroupedApiVersionMetadata>.New(
            string? groupName,
            ApiVersionMetadata metadata ) => new( groupName, metadata );

        /// <inheritdoc />
        public bool Equals( GroupedApiVersionMetadata? other ) =>
            other is not null && other.GetHashCode() == GetHashCode();

        /// <inheritdoc />
        public override bool Equals( object? obj ) =>
            obj is not null &&
            GetType().Equals( obj.GetType() ) &&
            GetHashCode() == obj.GetHashCode();

        /// <inheritdoc />
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